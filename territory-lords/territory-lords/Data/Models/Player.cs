using System;

namespace territory_lords.Data.Models
{
    /// <summary>
    /// A player playing a game
    /// </summary>
    public class Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Color { get; set; } = default!;
        public string BorderColor { get; set; } = default!;
    }
}
