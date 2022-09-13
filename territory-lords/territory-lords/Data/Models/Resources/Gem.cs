namespace territory_lords.Data.Models.Resources
{
    public class Gem : ITileResource
    {
        public string Name { get => this.GetType().Name; }
        public byte Food { get => 0; }
        public byte Production { get => 0; }
        public byte Trade { get => 4; }
    }
}
