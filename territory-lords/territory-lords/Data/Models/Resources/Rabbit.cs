namespace territory_lords.Data.Models.Resources
{
    public class Rabbit : ITileResource
    {
        public string Name { get => this.GetType().Name; }
        public byte Food { get => 2; }
        public byte Production { get => 0; }
        public byte Trade { get => 0; }
    }
}
