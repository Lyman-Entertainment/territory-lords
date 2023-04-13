namespace territory_lords.Data.Technologies
{
    public class Pottery : BaseTechnology, ITechnology
    {
        public Pottery() : base()
        {
            Name = "Pottery";
            Price = 10;
            RequiredTechs = new();
            TechnologyType = TechnologyTypes.Pottery;
        }
    }
}
