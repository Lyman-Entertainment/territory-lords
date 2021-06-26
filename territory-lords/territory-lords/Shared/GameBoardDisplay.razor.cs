using Microsoft.AspNetCore.Components;
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
    public partial class GameBoardDisplay : IAsyncDisposable
    {
        [Inject] NavigationManager NavigationManager { get; set; }
        [Inject] GameBoardCache BoardCache { get; set; }

        [Parameter]
        public GameBoard gameBoard { get; set; }

        private HubConnection gameHubConnection;
        private territory_lords.Data.Models.Units.IUnit PlayerActiveUnit = null;

        protected override async Task OnInitializedAsync()
        {
            gameHubConnection = new HubConnectionBuilder()
                .WithUrl(
                    NavigationManager.ToAbsoluteUri("/gamehub"),
                    config => config.UseDefaultCredentials = true)
                .WithAutomaticReconnect()
                .Build();

            gameHubConnection.On<string, string>("TileUpdate", (gameBoardId, serializedGameTile) =>
            {
                if (gameBoardId == gameBoard.GameBoardId)
                {
                    GameTile gameTile = JsonConvert.DeserializeObject<GameTile>(serializedGameTile,new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    
                    //we don't want to display the other players active units so if there is a unit in here set it's active state to false
                    if(gameTile.Unit != null)
                    {
                        gameTile.Unit.Active = false;
                    }
                    gameBoard.Board[gameTile.RowIndex, gameTile.ColumnIndex] = gameTile;
                    StateHasChanged();
                }

            });

            try
            {

                await gameHubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                var t = ex.ToString();
                
            }finally
            {
                var g = gameHubConnection;
            }
        }

        private void SendTileUpdate(GameTile gameTile)
        {
            gameHubConnection.SendAsync("SendTileUpdate", gameBoard.GameBoardId, gameTile.ToJson());
        }

        public bool IsConnected =>
            gameHubConnection.State == HubConnectionState.Connected;

        //At some point I think this should be handled by a game manager or something
        private void HandleGameBoardSquareClick(GameTile gameTile)
        {
            if (gameTile.LandType == LandType.Ocean)
            {
                //don't do anything now. You can't do anything with ocean tiles yet
            }
            else
            {
                var rnd = new Random();
                gameTile.Color = Colors[rnd.Next(1, Colors.Count + 1)];

                //there's got to be a better way to do this than all these if statements
                //if there is a unit on this tile select it as active
                if(gameTile.Unit != null)
                {
                    var localUnit = gameTile.Unit;
                    localUnit.ColumnIndex = gameTile.ColumnIndex;
                    localUnit.RowIndex = gameTile.RowIndex;
                    //check for if players unit so we don't do stuff to other players units

                    if (PlayerActiveUnit != null && (localUnit.ColumnIndex != PlayerActiveUnit?.ColumnIndex || localUnit.RowIndex != PlayerActiveUnit?.RowIndex))
                    {
                        //the user clicked on a different unit so unset the old active
                        PlayerActiveUnit.Active = false;
                    }

                    //set the class to be active
                    localUnit.Active = !localUnit.Active;
                    PlayerActiveUnit = localUnit.Active ? localUnit : null;
                }
                else //this is an empty tile that they might be moving a unit to
                {
                    
                    if (PlayerActiveUnit != null)
                    {
                        //clear the unit at the old coordinates
                        GameTile oldTile = gameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
                        oldTile.Unit = null;
                        //send an update to everyone
                        SendTileUpdate(oldTile);
                        //gameBoard.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex).Unit = null;

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

            BoardCache.UpdateGameCache(gameBoard);

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
                var directNeighbors = gameBoard.GetGameTileDirectNeighbors(gameTile);

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


        public Dictionary<int, string> Colors = new Dictionary<int, string> { { 1, "Orange" }, { 2, "Cyan" }, { 3, "Red" }, { 4, "Chartreuse" }, { 5, "DeepPink" } };

        public async ValueTask DisposeAsync()
        {
            await gameHubConnection.DisposeAsync();
        }
    }
}
