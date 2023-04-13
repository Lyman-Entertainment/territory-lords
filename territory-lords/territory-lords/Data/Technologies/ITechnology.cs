using System.Collections.Generic;

namespace territory_lords.Data.Technologies
{
    public interface ITechnology
    {
        public string Name { get;}
        public byte Price { get;}
        public List<ITechnology> RequiredTechs { get;}
        public TechnologyTypes TechnologyType { get;}
    }
}
