using territory_lords.Data.Models;

namespace territory_lords.Shared
{
    public record UnitOrder
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public OrderType OrderType { get; set; }

        public UnitOrder(string name, string icon, OrderType orderType)
        {
            Name = name;
            Icon = icon;
            OrderType = orderType;
        }
    }
}
