using System;
using System.Collections.Generic;
using territory_lords.Data.Models;
using territory_lords.Data.Models.Tiles;
using territory_lords.Data.Models.Units;
using territory_lords.Data.Statics;

namespace territory_lords.Shared
{
    public class UnitOrderManager
    {
        public List<UnitOrder> GetUnitsMenuOptions(GameBoardTile tile, IUnit unit)
        {
            List<UnitOrder> unitsOptions = new();

            //TODO: this will need logic to determine what orders are actually available based on the tile and the unit
            if (unit.GetType() == typeof(Settler))
            { 
                unitsOptions.Add(new UnitOrder("Build City", MudBlazor.Icons.Material.Filled.Home, OrderType.BuildCity));
                unitsOptions.Add(new UnitOrder("Build Road", MudBlazor.Icons.Material.Filled.AddRoad, OrderType.Road));
                unitsOptions.Add(new UnitOrder("Build Farm", MudBlazor.Icons.Material.Filled.Water, OrderType.Irrigate));
                unitsOptions.Add(new UnitOrder("Build Mine", MudBlazor.Icons.Material.Filled.Nature, OrderType.Mine));
                unitsOptions.Add(new UnitOrder("Build Fortress", MudBlazor.Icons.Material.Filled.Castle, OrderType.Fortress));
                unitsOptions.Add(new UnitOrder("Build Factory", MudBlazor.Icons.Material.Filled.Co2, OrderType.Factory));
                unitsOptions.Add(new UnitOrder("Build Lumbermill", MudBlazor.Icons.Material.Filled.Forest, OrderType.Lumbermill));

            }
            else
            {
                unitsOptions.Add(new UnitOrder("Sentry", MudBlazor.Icons.Material.Filled.ShieldMoon, OrderType.Sentry));
                unitsOptions.Add(new UnitOrder("Fortify", MudBlazor.Icons.Material.Filled.Shield, OrderType.Fortify));
                unitsOptions.Add(new UnitOrder("Pillage", MudBlazor.Icons.Material.Filled.RemoveCircle, OrderType.Pillage));
            }

            unitsOptions.Add(new UnitOrder("Disband", MudBlazor.Icons.Material.Filled.Delete, OrderType.Disband));

            return unitsOptions;
        }

    }
}
