using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Hubs
{
    public class GameHub : Hub
    {
        public async Task SendTileUpdate(string gameBoardId,int row, int col, string color)
        {
            await Clients.Others.SendAsync("TileUpdate",gameBoardId, row, col, color);
        }
    }
}
