using territory_lords.Data.Technologies;

namespace territory_lords.Data.Models.Wonders
{
    public class ApolloProgram : BaseWonderBuilding, IWonderBuilding
    {

        public ApolloProgram()
        {
            WonderType = WonderTypes.ApolloProgram;
            RequiredTech = new SpaceFlight();
            ObsoleteTech = null;
        }
    }
}
