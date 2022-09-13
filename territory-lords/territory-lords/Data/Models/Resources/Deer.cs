namespace territory_lords.Data.Models.Resources
{
    public class Deer : ITileResource
    {
        public string Name { get => this.GetType().Name; }
        public byte Food { get => 3; }
        public byte Production { get => 0; }
        public byte Trade { get => 0; }
    }
}
