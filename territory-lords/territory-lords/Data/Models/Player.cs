using System;
using System.Drawing;

namespace territory_lords.Data.Models
{
    /// <summary>
    /// A player playing a game
    /// </summary>
    public record Player
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        //public string Color { get; set; } = default!;
        //public string BorderColor { get; set; } = default!;
        public PlayerColors Colors { get; set; } = default!;
    }

    /// <summary>
    /// Colors for the player
    /// </summary>
    public record PlayerColors
    {
        public string TileColor { get; set; } = default!;
        public string BorderColor { get; set; } = default!;
    }


}
