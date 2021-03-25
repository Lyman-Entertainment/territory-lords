using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models;

namespace territory_lords.Data.Cache
{
    public class GameBoardCache : IGameBoardCache
    {
        private Dictionary<string, string> _gameCache;
        public GameBoardCache()
        {
            _gameCache = new Dictionary<string, string>();
        }

        public void UpdateGameCache(GameBoard gameBoard)
        {
            _gameCache.Remove(gameBoard.GameBoardId);
            _gameCache.Add(gameBoard.GameBoardId, gameBoard.ToJson());
        }

        public GameBoard GetGameBoard(string gameBoardId)
        {
            string jsonBoard = _gameCache.GetValueOrDefault(gameBoardId);

            return string.IsNullOrEmpty(jsonBoard) ? null : JsonConvert.DeserializeObject<GameBoard>(jsonBoard);
        }
    }
}
