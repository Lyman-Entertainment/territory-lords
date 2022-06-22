using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Cache;
using territory_lords.Data.Models;


namespace territory_lords.Shared
{
    //This needs to be refactored and lots of stuff needs to be pulled out. The job of Game Board Display should be to display, not handle click events and hub connections
    public partial class GameBoardDisplay : IAsyncDisposable
    {
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] GameBoardCache BoardCache { get; set; }
        [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }

        [Parameter]
        public GameBoard TheGameBoard { get; set; }

        private HubConnection GameHubConnection;
        private territory_lords.Data.Models.Units.IUnit? PlayerActiveUnit = null;
        private Guid? CurrentPlayerGuid = default;
 
        protected override async Task OnInitializedAsync()
        {
            var authUser = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            var possibleGuidString = authUser.FindFirst(c => c.Type.Contains("objectidentifier"))?.Value;
            if(possibleGuidString != null)
            {
                CurrentPlayerGuid = new Guid(possibleGuidString);
            }
            

            //build a connection to the game hub
            GameHubConnection = new HubConnectionBuilder()
                .WithUrl(
                    NavigationManager.ToAbsoluteUri("/gamehub"),
                    config => config.UseDefaultCredentials = true)
                .WithAutomaticReconnect()
                .Build();

            //What to do when the TileUpdate event comes in
            GameHubConnection.On<string, string>("TileUpdate", (gameBoardId, serializedGameTile) =>
            {
                if (gameBoardId == TheGameBoard.GameBoardId)
                {
                    GameTile gameTile = JsonConvert.DeserializeObject<GameTile>(serializedGameTile,new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    
                    //we don't want to display the other players active units so if there is a unit in here set it's active state to false
                    if(gameTile?.Unit != null)
                    {
                        gameTile.Unit.Active = false;
                    }
                    TheGameBoard.Board[gameTile.RowIndex, gameTile.ColumnIndex] = gameTile;
                    StateHasChanged();
                }

            });

            //try and actually connect the connection to the game hub
            try
            {

                await GameHubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                var t = ex.ToString();
                
            }finally
            {
                var g = GameHubConnection;
            }
        }

        //send a Tile Update event
        private void SendTileUpdate(GameTile gameTile)
        {
            GameHubConnection.SendAsync("SendTileUpdate", TheGameBoard.GameBoardId, gameTile.ToJson());
        }

        public bool IsConnected =>
            GameHubConnection.State == HubConnectionState.Connected;

        //At some point I think this should be handled by a game manager or something
        /// <summary>
        /// Figure out what happens when a player clicks a tile
        /// </summary>
        /// <param name="gameTile"></param>
        private void HandleGameBoardSquareClick(GameTile gameTile)
        {
            //there's got to be a better way to do this than all these if statements

            if (gameTile.LandType == LandType.Ocean)
            {
                //don't do anything now. You can't do anything with ocean tiles yet
            }
            else
            {
               
                //if there is a unit on this tile we need to figure out if it's the player's unit to select it as active or an enemy unit the player is attacking
                if(gameTile.Unit != null)
                {
                    
                    var localUnit = gameTile.Unit;
                    localUnit.ColumnIndex = gameTile.ColumnIndex;
                    localUnit.RowIndex = gameTile.RowIndex;

                    //this is their own unit
                    if (gameTile.Unit.OwningPlayer?.Id == CurrentPlayerGuid)
                    {
                        //all we need to do is switch the active flag on the localUnit and either set or null out the ActiveUnit

                        //see if they clicked on a different unit than the active unit so we can make the current active unit stop being currently active
                        if (PlayerActiveUnit != null &&
                            (localUnit.ColumnIndex != PlayerActiveUnit?.ColumnIndex
                            || localUnit.RowIndex != PlayerActiveUnit?.RowIndex))
                        {
                            //the user clicked on a different unit so unset the old active
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            PlayerActiveUnit.Active = false;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                        }

                        localUnit.Active = !localUnit.Active;
                        PlayerActiveUnit = localUnit.Active ? localUnit : null;
                    }
                    else//this is not their own unit
                    {
                        //for right now we don't care if the unit is in range
                        //TODO: make sure the unit can move here
                        if (PlayerActiveUnit != null)
                        {
                            //this is the EXACT same code as moving a unit
                            //TODO: Make moving a unit a method to call
                            //clear the unit at the old coordinates
                            GameTile? oldTile = TheGameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
                            if (oldTile != null)
                            {
                                oldTile.Unit = null;
                                //send an update to everyone
                                SendTileUpdate(oldTile);
                                //gameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex).Unit = null;
                            }

                            //update the unit to be in the new place
                            gameTile.Unit = PlayerActiveUnit;
                            gameTile.Unit.ColumnIndex = gameTile.ColumnIndex;
                            gameTile.Unit.RowIndex = gameTile.RowIndex;
                            gameTile.Unit.Active = false;

                            //then set the active unit to be nothing because we just moved a unit
                            PlayerActiveUnit = null;
                        }
                            
                    }

                    

                    
                }
                else //this is an empty tile that they might be moving a unit to
                {
                    
                    if (PlayerActiveUnit != null)
                    {
                        //clear the unit at the old coordinates
                        GameTile? oldTile = TheGameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
                        if (oldTile != null)
                        {
                            oldTile.Unit = null;
                            //send an update to everyone
                            SendTileUpdate(oldTile);
                            //gameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex).Unit = null;
                        }

                        //update the unit to be in the new place
                        gameTile.Unit = PlayerActiveUnit;
                        gameTile.Unit.ColumnIndex = gameTile.ColumnIndex;
                        gameTile.Unit.RowIndex = gameTile.RowIndex;
                        gameTile.Unit.Active = false;

                        //then set the active unit to be nothing because we just moved a unit
                        PlayerActiveUnit = null;
                    }
                }

                
            }

            //update the game board cache
            BoardCache.UpdateGameCache(TheGameBoard);

            //What are ya silly? I'm still gonna send it
            SendTileUpdate(gameTile);
        }

        /// <summary>
        /// Using a gameTile return the correct css class for background display images
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns></returns>
        private string GetCorrectBackgroundCSSClass(GameTile gameTile)
        {
            //this is for ocean tiles only right now. also this needs to be reworked to be more readalbe and extensible
            //if we're on the edge we need to check if there is an ocean tile in the adjoining tiles not in the corresponding line with the tile we're working with
            //if there is not an ocean tile then set the class to the correct straight ocean sprite

            if (gameTile.LandType == LandType.Ocean)
            {
                //figure out what's around us so we can act accordingly and not look wierd to everyone. Keep it together Kevin!
                var directNeighbors = TheGameBoard.GetGameTileDirectNeighbors(gameTile);

                //if all of the direct neighbors are ocean then this is ocean too
                if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-top";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-bottom";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-left";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] != null || directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-right";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-single";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-bay-top";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-bay-right";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-bay-bottom";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-bay-left";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-channel-horizontal";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-channel-vertical";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-elbow-top-right";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] == null || directNeighbors[1].LandType == LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] != null && directNeighbors[3].LandType != LandType.Ocean))
                {
                    return "Ocean-elbow-bottom-right";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-elbow-bottom-left";
                }
                else if ((directNeighbors[0] != null && directNeighbors[0].LandType != LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] == null || directNeighbors[2].LandType == LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-elbow-bottom-left";
                }
                else if ((directNeighbors[0] == null || directNeighbors[0].LandType == LandType.Ocean) && (directNeighbors[1] != null && directNeighbors[1].LandType != LandType.Ocean) && (directNeighbors[2] != null && directNeighbors[2].LandType != LandType.Ocean) && (directNeighbors[3] == null || directNeighbors[3].LandType == LandType.Ocean))
                {
                    return "Ocean-elbow-top-left";
                }
            }

            //just return the basic name other wise for now
            return gameTile.LandType.ToString("G");
        }

        //old color list for marking tile border color
        public Dictionary<int, string> Colors = new Dictionary<int, string> { { 1, "Orange" }, { 2, "Cyan" }, { 3, "Red" }, { 4, "Chartreuse" }, { 5, "DeepPink" } };

        public async ValueTask DisposeAsync()
        {
            await GameHubConnection.DisposeAsync();
        }
    }
}
