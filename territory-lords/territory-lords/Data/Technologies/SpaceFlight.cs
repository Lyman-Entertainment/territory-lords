namespace territory_lords.Data.Technologies
{
    public class SpaceFlight : BaseTechnology
    {
        public SpaceFlight() : base()
        {
            Name = "Space Flight";
            Price = 10;
            RequiredTechs = new() {};//computers, Rocketry
            TechnologyType = TechnologyTypes.SpaceFlight;
        }
    }
}
