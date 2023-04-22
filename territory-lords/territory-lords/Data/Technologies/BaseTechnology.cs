using System.Collections.Generic;

namespace territory_lords.Data.Technologies
{
    public abstract class BaseTechnology : ITechnology
    {
        public string Name { get; protected set; }
        public byte Price { get; protected set; }
        public List<ITechnology> RequiredTechs { get; protected set; } = default!;
        public TechnologyTypes TechnologyType { get; protected set; }
    }
}
