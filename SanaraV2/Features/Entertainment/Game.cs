using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SanaraV2.Features.Entertainment.Response;

namespace SanaraV2.Features.Entertainment
{
    public static class Game
    {
        public static async Task<FeatureRequest<Response.Game, Error.Game>> Play(string[] args, bool isChanNsfw, ulong chanId, List<Modules.Entertainment.GameModule.Game> games)
        {
            if (games.Any(x => x.m_chan.Id == chanId))
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.AlreadyRunning));
            if (args.Length == 0)
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongName));
            string gameName = args[0].ToLower();
            bool isNormal = true;
            if (args.Length > 1)
            {
                string difficulty = args[1].ToLower();
                if (difficulty == "easy")
                    isNormal = false;
                else if (difficulty != "normal")
                    return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongDifficulty));
            }
            GameName gn;
            if (gameName == "shiritori")
                gn = GameName.Shiritori;
            else if (gameName == "anime")
                gn = GameName.Anime;
            else if (gameName == "booru" || gameName == "gelbooru")
                gn = GameName.Booru;
            else if (gameName == "kancolle" || gameName == "kantaicollection" || gameName == "kc")
                gn = GameName.Kancolle;
            else if (gameName == "fireemblem" || gameName == "fe")
                gn = GameName.FireEmblem;
            else
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.WrongName));
            if (gn == GameName.Booru && !isChanNsfw)
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.NotNsfw));
            // TODO: replace files by db
            if ((gn == GameName.Shiritori && !File.Exists("Saves/shiritoriWords.dat"))
            || (gn == GameName.Anime && !File.Exists("Saves/AnimeTags.dat"))
            || (gn == GameName.Booru && !File.Exists("Saves/BooruTriviaTags.dat")))
                return (new FeatureRequest<Response.Game, Error.Game>(null, Error.Game.NoDictionnary));
            return (new FeatureRequest<Response.Game, Error.Game>(new Response.Game()
            {
                gameName = gn,
                isNormal = isNormal
            }, Error.Game.None));
        }
    }
}
