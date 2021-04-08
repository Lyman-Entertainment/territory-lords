using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Cache;

namespace territory_lords.Shared
{
    public class GameManager
    {

        private IGameBoardCache _gameBoardCache;
        public GameManager(IGameBoardCache gameBoardCache)
        {

        }

    }
}
