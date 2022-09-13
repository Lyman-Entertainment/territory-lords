namespace territory_lords.Data.Models.Resources
{
    public class Oil : ITileResource
    {
        public string Name { get => this.GetType().Name; }
        public byte Food { get => 0; }
        public byte Production { get => 4; }
        public byte Trade { get => 0; }
    }
}
