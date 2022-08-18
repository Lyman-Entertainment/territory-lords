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
using territory_lords.Data.Models.Units;

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
        private IUnit? PlayerActiveUnit = null;
        private Guid? CurrentPlayerGuid = default;
 
        protected override async Task OnInitializedAsync()
        {

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
                //TODO need to find a way that other games messages won't even come here. Still should probably make sure the gameID matches, but if there are 100 games going that's going to be a lot of messages that don't pertain to this game
                if (gameBoardId == TheGameBoard.GameBoardId)
                {
                    GameTile gameTile = JsonConvert.DeserializeObject<GameTile>(serializedGameTile,new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    
                    
                    if (gameTile?.Unit != null)
                    {
                        //we don't want to display the other players active units so if there is a unit in here set it's active state to false
                        gameTile.Unit.Active = false;

                        //we need to set the Active Unit to nothing if the other player moved their unit onto our active unit
                        if (gameTile.Unit.ColumnIndex == PlayerActiveUnit?.ColumnIndex
                        && gameTile.Unit.RowIndex == PlayerActiveUnit?.RowIndex)
                        {
                            PlayerActiveUnit = null;
                        }
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

            //this needs to happen at the end because we want to send a tile update with a unit
            var authUser = (await AuthStateProvider.GetAuthenticationStateAsync()).User;
            var possibleGuidString = authUser.FindFirst(c => c.Type.Contains("objectidentifier"))?.Value;
            if (possibleGuidString != null)
            {
                //add them to the board as well as track them here
                CurrentPlayerGuid = new Guid(possibleGuidString);
                Player? addedPlayer = TheGameBoard.AddPlayerToGame(CurrentPlayerGuid.Value, authUser.Identity.Name);
                if (addedPlayer != null)
                {
                    
                    //TODO: the other players need to know this user joined

                    //TODO: create a unit and put it on the board and send an update
                    //this isn't the way to do it but is the way I'm doing it for now to test it
                    var newUnit = new Malitia(addedPlayer);
                    var aTile = TheGameBoard.GetGameTileAtIndex(3, 3);
                    newUnit.RowIndex = 3;
                    newUnit.ColumnIndex = 3;
                    aTile.Unit = newUnit;
                    BoardCache.UpdateGameCache(TheGameBoard);
                    SendTileUpdate(aTile);
                    
                }
            }

        }

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
        /// <param name="selectedGameTile"></param>
        private void HandleGameBoardSquareClick(GameTile selectedGameTile)
        {
            //there's got to be a better way to do this than all these if statements

            if (selectedGameTile.LandType == LandType.Ocean)
            {
                //don't do anything now. You can't do anything with ocean tiles yet
            }
            else
            {
               
                //if there is a unit on this tile we need to figure out if it's the player's unit to select it as active or an enemy unit the player is attacking
                if(selectedGameTile.Unit != null)
                {
                    
                    var selectedUnit = selectedGameTile.Unit;
                    //doesn't seem like we should need to do this but whatever for now it's fine
                    selectedUnit.ColumnIndex = selectedGameTile.ColumnIndex;
                    selectedUnit.RowIndex = selectedGameTile.RowIndex;

                    //this is their own unit
                    if (selectedUnit.OwningPlayer?.Id == CurrentPlayerGuid)
                    {
                        //there are 3 scenarios here.
                        //1. they are selecting a unit to be active. Meaning there is no active unit
                        //2. they are deselecting the active unit. Meaning they are clicking on the active unit
                        //3. they have an active unit and are wanting the two units to switch places

                        if (PlayerActiveUnit == null)
                        {
                            //they are selecting a unit
                            selectedUnit.Active = true;
                            PlayerActiveUnit = selectedUnit;
                        }
                        else if(PlayerActiveUnit.Equals(selectedUnit))
                        {
                            //they are de-selecting a unit
                            selectedUnit.Active = false;
                            PlayerActiveUnit = null;
                        }
                        else
                        {
                            //they are swapping the active unit and selected units positions
                            //put the clicked on unit into the square where the active unit currently is
                            GameTile? oldTile = TheGameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
                            selectedUnit.ColumnIndex = PlayerActiveUnit.ColumnIndex;
                            selectedUnit.RowIndex = PlayerActiveUnit.RowIndex; 
                            selectedUnit.Active = false;//turn off active

                            oldTile.Unit = selectedUnit;
                            SendTileUpdate(oldTile);


                            //now put the active unit into the clicked on square
                            selectedGameTile.Unit = PlayerActiveUnit;
                            selectedGameTile.Unit.ColumnIndex = selectedGameTile.ColumnIndex;
                            selectedGameTile.Unit.RowIndex = selectedGameTile.RowIndex;
                            selectedGameTile.Unit.Active = false;

                            //then set the active unit to be nothing because we just moved a unit
                            PlayerActiveUnit = null;


                        }
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
                            }

                            //update the unit to be in the new place
                            selectedGameTile.Unit = PlayerActiveUnit;
                            selectedGameTile.Unit.ColumnIndex = selectedGameTile.ColumnIndex;
                            selectedGameTile.Unit.RowIndex = selectedGameTile.RowIndex;
                            selectedGameTile.Unit.Active = false;

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
                        }

                        //update the unit to be in the new place
                        selectedGameTile.Unit = PlayerActiveUnit;
                        selectedGameTile.Unit.ColumnIndex = selectedGameTile.ColumnIndex;
                        selectedGameTile.Unit.RowIndex = selectedGameTile.RowIndex;
                        selectedGameTile.Unit.Active = false;

                        //then set the active unit to be nothing because we just moved a unit
                        PlayerActiveUnit = null;
                    }
                }

                
            }

            //update the game board cache
            BoardCache.UpdateGameCache(TheGameBoard);

            //What are ya silly? I'm still gonna send it
            SendTileUpdate(selectedGameTile);
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

        public async ValueTask DisposeAsync()
        {
            await GameHubConnection.DisposeAsync();
        }
    }
}
