[<img src="https://discordbots.org/api/widget/owner/329664361016721408.svg"/>](https://discordbots.org/bot/329664361016721408)<br/>
[![Build status](https://ci.appveyor.com/api/projects/status/o67101qtad8drfit/branch/master?svg=true)](https://ci.appveyor.com/project/Xwilarg/sanara/branch/master)<br/>
[![CodeFactor](https://www.codefactor.io/repository/github/xwilarg/sanara/badge)](https://www.codefactor.io/repository/github/xwilarg/sanara)<br/>
# Sanara

Sanara is a Discord bot made in C#.

Her goal is to provide various functionalities, you can check them [here](https://zirk.eu/sanara.html#commands).

I spent a lot of times working on her and I hope that her source code will be able to help some people as the source code of some others bots did for me.

# Useful links

[Commands](https://zirk.eu/sanara.html#commands)<br/>
[Official Discord server](https://discordapp.com/invite/H6wMRYV)<br/>
[Report an issue/Make a suggestion](https://github.com/Xwilarg/Sanara/issues)<br/>
[Invitation link](https://discordapp.com/oauth2/authorize?client_id=329664361016721408&permissions=3196928&scope=bot)<br/>
[Help for the translation](https://github.com/Xwilarg/Sanara-translations/)<br/>

# External libraries used

[BooruSharp](https://github.com/Xwilarg/BooruSharp)<br/>
[Discord.Net](https://github.com/RogueException/Discord.Net)<br/>
[Google API DotNet](https://github.com/google/google-api-dotnet-client)<br/>
[Opus, Sodium and FFmpeg](https://dsharpplus.emzi0767.com/natives/index.html)<br/>
[Raven-csharp](https://github.com/getsentry/raven-csharp)<br/>
[RethinkDb](https://github.com/rethinkdb/rethinkdb)<br/>
[VNDBSharp](https://github.com/Nikey646/VndbSharp)<br/>
[youtube-dl](https://rg3.github.io/youtube-dl/)<br/>

# Please make note that the bot also collect and save the following datas:

About users: id of the highscore (when playing games)<br/>
About guilds: Guild preference (box prefix and language)<br/>
You can check the different stats about the bot here: https://zirk.eu/sanara.html#stats<br/>
You can also do the 'GDPR' command at any time to check the informations the bot have on you (and on the guild if you are the owner).

# How to use Sanara by cloning the repository

- First clone the repository like that: git clone --recurse-submodules https://github.com/Xwilarg/Sanara.git , that will allow you to also clone the translation submodule.
- Then open SanaraV2.sln with Visual Studio
- You'll need to create a 'Keys' folder near your executable (default location: bin/Debug/)
- You then need to create at least a 'token.dat' file inside the Keys folder. It contains the token of your bot.
- The last step is to go inside 'Sentences.cs' and change the ownerId value by your Discord id.
- Then download [RethinkDb](https://rethinkdb.com/docs/install/) and launch it
- You can also create the following files in the 'Keys' folder:
  - Sanara-7430da57d6af.json (contain your json file to access to the Google Translate API)
  - URLShortenerAPIKey.dat (contain your key for the goo.gl API), youtubeAPIKey.dat (contain your key for the YouTube API)
  - ranven.dat (contain your url to use Sentry)
  - visionAPI.json (contain your json for the Google Vision API)
- To use radio you will also need to add opus.dll, libsodium.dll, ffmpeg.exe and youtube-dl.exe near Sanara executable, you can download them [here](https://dsharpplus.emzi0767.com/natives/index.html) and [here](https://rg3.github.io/youtube-dl/).
- To use games, you will also need to add files to the 'Saves' folder (downloadable [here](https://files.zirk.eu/?dir=Sanara)):
  - AnimeTags.dat for the anime guess game
  - BooruTriviaTags.dat for the booru guess game
  - shiritoriWords.dat for the shiritori game
