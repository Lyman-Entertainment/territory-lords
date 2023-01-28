using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models;

namespace territory_lords.Hubs
{
    [AllowAnonymous]
    public class GameHub : Hub
    {

        public GameHub()
        {
            //what should go in a ctor of a hub?
        }
        public async Task SendGameBoardTileUpdate(string gameBoardId,string serializedGameTile)
        {
            await Clients.Others.SendAsync("GameBoardTileUpdate",gameBoardId, serializedGameTile);
        }

        public async Task SendUnitUpdate(string gameBoardId, string serializedGameTile)
        {
            await Clients.Others.SendAsync("UnitUpdate", gameBoardId, serializedGameTile);
        }
    }
}
