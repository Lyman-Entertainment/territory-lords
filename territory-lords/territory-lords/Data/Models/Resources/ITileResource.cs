namespace territory_lords.Data.Models.Resources
{
    public interface ITileResource
    {
        public string Name { get;}
        public byte Food { get;}
        public byte Production { get;}
        public byte Trade { get;}
    }
}
