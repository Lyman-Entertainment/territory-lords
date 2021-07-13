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
    public class ChatHub : Hub
    {
        public async Task SendMessage(ChatMessage chatMessage)
        {
            //just send it to the people who aren't us. We already know about it
            await Clients.Others.SendAsync("ReceiveMessage", chatMessage);
        }
    }
}
