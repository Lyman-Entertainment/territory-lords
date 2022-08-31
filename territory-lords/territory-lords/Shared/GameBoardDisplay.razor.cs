﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Cache;
using territory_lords.Data.Models;
using territory_lords.Data.Models.Improvements;
using territory_lords.Data.Models.Units;
using territory_lords.Data.Statics.Extensions;

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

                    ////TODO: create a city for the player
                    //var city = new City(addedPlayer);
                    //var cityTile = TheGameBoard.GetGameTileAtIndex(2, 3);
                    ////city.RowIndex = 3;
                    ////city.ColumnIndex = 3;
                    //cityTile.Improvement = city;
                    //BoardCache.UpdateGameCache(TheGameBoard);
                    //SendTileUpdate(cityTile);

                    //TODO: create a unit and put it on the board and send an update
                    //this isn't the way to do it but is the way I'm doing it for now to test it
                    var settler = new Settler(addedPlayer);
                    var settlerTile = TheGameBoard.InsertUnitToMap(settler);
                    var malitia = new Malitia(addedPlayer);
                    var malitiaTile = TheGameBoard.InsertUnitToMap(malitia);
                    BoardCache.UpdateGameCache(TheGameBoard);
                    SendTileUpdate(settlerTile);
                    SendTileUpdate(malitiaTile);
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
                            GameTile? oldTile = TheGameBoard.Board.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
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
                            GameTile? oldTile = TheGameBoard.Board.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
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
                        GameTile? oldTile = TheGameBoard.Board.GetGameTileAtIndex(PlayerActiveUnit.RowIndex, PlayerActiveUnit.ColumnIndex);
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
            //figure out what's around us so we can act accordingly and not look wierd to everyone. Keep it together Kevin!
            var directNeighbors = TheGameBoard.Board.GetGameTileDirectNeighbors(gameTile.RowIndex,gameTile.ColumnIndex);
            var cssClass = gameTile.LandType.ToString("g");
            switch (gameTile.LandType)
            {
                //all of these functions need to be refacored. This doesn't seem like a great way to do this
                case LandType.Ocean:
                    cssClass = GetCorrectOceanCssClass(directNeighbors);break;
                case LandType.River:
                    cssClass = GetCorrectRiverCssClass(directNeighbors); break;
                case LandType.Mountains:
                    cssClass = GetCorrectMountainCssClass(directNeighbors); break;
                case LandType.Hills:
                    cssClass = GetCorrectHillsCssClass(directNeighbors);break;
                case LandType.Jungle:
                    cssClass = GetCorrectJungleCssClass(directNeighbors);break;

            }


            return cssClass;
        }

        private string GetCorrectJungleCssClass(GameTile[] neighbors)
        {
            var jungle = LandType.Jungle;
            //if no neighbors are hills than this is just a single hill all by it's lonesome self
            if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-cross";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-straight-vertical";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-straight-horizontal";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-bend-north-east";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-bend-south-east";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-bend-south-west";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-bend-north-west";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-tee-north";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-tee-east";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-tee-south";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-tee-west";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType == jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-start-north";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType == jungle))
            {
                return "Jungle-start-east";
            }
            else if ((neighbors[0].LandType == jungle) && (neighbors[1].LandType != jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-start-south";
            }
            else if ((neighbors[0].LandType != jungle) && (neighbors[1].LandType == jungle) && (neighbors[2].LandType != jungle) && (neighbors[3].LandType != jungle))
            {
                return "Jungle-start-west";
            }

            return "Jungle";
        }

        private string GetCorrectHillsCssClass(GameTile[] neighbors)
        {
            var hills = LandType.Hills;
            //if no neighbors are hills than this is just a single hill all by it's lonesome self
            if ((neighbors[0].LandType != hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType != hills))
            {
                return "Hills";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-cross";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-straight-vertical";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-straight-horizontal";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-bend-north-east";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-bend-south-east";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-bend-south-west";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-bend-north-west";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-tee-north";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-tee-east";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-tee-south";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-tee-west";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType == hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-start-north";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType == hills))
            {
                return "Hills-start-east";
            }
            else if ((neighbors[0].LandType == hills) && (neighbors[1].LandType != hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-start-south";
            }
            else if ((neighbors[0].LandType != hills) && (neighbors[1].LandType == hills) && (neighbors[2].LandType != hills) && (neighbors[3].LandType != hills))
            {
                return "Hills-start-west";
            }

            return "Hills";
        }

        private string GetCorrectMountainCssClass(GameTile[] neighbors)
        {
            var mountains = LandType.Mountains;
            //if no neighbors are mountains than this is just a single mountain all by it's lonesome self
            if ((neighbors[0]?.LandType != mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-cross";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-straight-vertical";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-straight-horizontal";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-bend-north-east";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-bend-south-east";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-bend-south-west";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-bend-north-west";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-tee-north";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-tee-east";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-tee-south";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-tee-west";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType == mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-start-north";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType == mountains))
            {
                return "Mountains-start-east";
            }
            else if ((neighbors[0].LandType == mountains) && (neighbors[1].LandType != mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-start-south";
            }
            else if ((neighbors[0].LandType != mountains) && (neighbors[1].LandType == mountains) && (neighbors[2].LandType != mountains) && (neighbors[3].LandType != mountains))
            {
                return "Mountains-start-west";
            }

            return "Mountains";
        }

        private string GetCorrectRiverCssClass(GameTile[] neighbors)
        {
            if (IsWaterTile(neighbors[0]) && IsWaterTile(neighbors[1]) && IsWaterTile(neighbors[2]) && IsWaterTile(neighbors[3]))
            {
                return "River-cross";
            }
            //we should probably check for connecting to Ocean first
            else if ((IsWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-straight-vertical";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-straight-horizontal";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-bend-south-east";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-bend-south-west";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-bend-north-west";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-bend-north-east";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-tee-north";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-tee-east";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-tee-south";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-tee-west";
            }
            else if ((IsWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-start-south";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-start-west";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsWaterTile(neighbors[2])) && (IsNotWaterTile(neighbors[3])))
            {
                return "River-start-north";
            }
            else if ((IsNotWaterTile(neighbors[0])) && (IsNotWaterTile(neighbors[1])) && (IsNotWaterTile(neighbors[2])) && (IsWaterTile(neighbors[3])))
            {
                return "River-start-east";
            }

            //return the blank green square
            return "River";
        }


        /// <summary>
        /// Gets the CSS Stylesheet class for the tile based off of it's direct neighbors for ocean tiles
        /// </summary>
        /// <param name="neighbors"></param>
        /// <returns></returns>
        private string GetCorrectOceanCssClass(GameTile?[] neighbors)
        {
            //if all of the direct neighbors are ocean then this is ocean too
            if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-top";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-bottom";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-left";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] != null || neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-right";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-single";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-bay-top";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-bay-right";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-bay-bottom";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-bay-left";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-channel-horizontal";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-channel-vertical";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-elbow-top-right";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] == null || neighbors[1]?.LandType == LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] != null && neighbors[3]?.LandType != LandType.Ocean))
            {
                return "Ocean-elbow-bottom-right";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-elbow-bottom-left";
            }
            else if ((neighbors[0] != null && neighbors[0]?.LandType != LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] == null || neighbors[2]?.LandType == LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-elbow-bottom-left";
            }
            else if ((neighbors[0] == null || neighbors[0]?.LandType == LandType.Ocean) && (neighbors[1] != null && neighbors[1]?.LandType != LandType.Ocean) && (neighbors[2] != null && neighbors[2]?.LandType != LandType.Ocean) && (neighbors[3] == null || neighbors[3]?.LandType == LandType.Ocean))
            {
                return "Ocean-elbow-top-left";
            }

            //I guess if we didn't find anything then just return the default ocean class
            return "Ocean";
        }


        /// <summary>
        /// Return wiether or not this tile has water on it
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns></returns>
        private bool IsWaterTile(GameTile? gameTile)
        {
            if (gameTile == null)
                return false;
            return (gameTile.LandType == LandType.Ocean || gameTile.LandType == LandType.River);
        }

        /// <summary>
        /// Return wiether or not this tile does not have water on it
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns></returns>
        private bool IsNotWaterTile(GameTile gameTile)
        {
            return !IsWaterTile(gameTile);
        }

        public async ValueTask DisposeAsync()
        {
            await GameHubConnection.DisposeAsync();
        }
    }
}
