using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models;

namespace territory_lords.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendTileUpdate(string gameBoardId,string serializedGameTile)
        {
            await Clients.Others.SendAsync("TileUpdate",gameBoardId, serializedGameTile);
        }
    }
}
