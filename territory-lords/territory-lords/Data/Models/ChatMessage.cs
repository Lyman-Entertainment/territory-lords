using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace territory_lords.Data.Models
{
    public record ChatMessage
    {
        public string UserName { get; set; }
        public string Message { get; set; }
        public bool Mine { get; set; } = default;
    }
}
