using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using territory_lords.Data.Models;

namespace territory_lords.Data.Cache
{
    //Currently this is the ONLY persistence layer. When the app is restarted everything is wiped away.
    //This is fine for now. Eventually we'll need to take this game cache and update a database or storage some place.
    //Doesn't need to be a database. We could probably just store .json files and overwrite them when we update and then load them as needed. 
    //We might need a database to store other parts though, like linking of somekind or some other type of data

    //this also needs to be thread safe or work with threads
    public class GameBoardCache : IGameBoardCache
    {
        
        private readonly Dictionary<string, GameBoard> _gameCache;
        public GameBoardCache()
        {
            _gameCache = new Dictionary<string, GameBoard>();
        }

        public void UpdateGameCache(GameBoard gameBoard)
        {
            //doing a remove then an add keeps us from having to check for the existance of a key.
            //if we simply did _gameCache[gameBoard.GameBoardId] = gameBaord.ToJson(); we would get an exception if the key didn't exist.
            //this could maybe be made better later
            //_gameCache.Remove(gameBoard.GameBoardId);
            //_gameCache.Add(gameBoard.GameBoardId, gameBoard);
            _gameCache[gameBoard.GameBoardId] = gameBoard;  
        }

        /// <summary>
        /// Get a gameboard matching this gameboardid
        /// </summary>
        /// <param name="gameBoardId"></param>
        /// <returns>A GameBoard or null</returns>
        public GameBoard? GetGameBoard(string gameBoardId)
        {
            return _gameCache.GetValueOrDefault(gameBoardId);

            //return string.IsNullOrEmpty(jsonBoard) ? null : JsonConvert.DeserializeObject<GameBoard>(jsonBoard,new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        /// <summary>
        /// Get all keys to the games in the cache. Later this should only return the games the player has access to
        /// </summary>
        /// <returns>IEnumerable of gameboardid strings</returns>
        public IEnumerable<string> GetIdsToPlayerGames()
        {
            return _gameCache.Keys.ToArray();
        }
    }
}
