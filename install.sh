#!/bin/sh
set -e
curl -LO https://download.rethinkdb.com/windows/rethinkdb-2.3.6.zip
yes | unzip rethinkdb-2.3.6.zip
rm rethinkdb-2.3.6.zip
mkdir -p SanaraV2\\bin\\Debug\\Saves
mkdir -p Sanara-UnitTests\\bin\\Debug\\Saves
mkdir -p Sanara-UnitTests\\bin\\Debug\\Keys
curl -LO https://files.zirk.eu/Sanara/AnimeTags.dat
cp AnimeTags.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv AnimeTags.dat SanaraV2\\bin\\Debug\\Saves
curl -LO https://files.zirk.eu/Sanara/BooruTriviaTags.dat
cp BooruTriviaTags.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv BooruTriviaTags.dat SanaraV2\\bin\\Debug\\Saves
curl -LO https://files.zirk.eu/Sanara/shiritoriWords.dat
cp shiritoriWords.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv shiritoriWords.dat SanaraV2\\bin\\Debug\\Saves
curl -LO https://discord.foxbot.me/binaries/win32/opus.dll
cp opus.dll Sanara-UnitTests\\bin\\Debug
mv opus.dll SanaraV2\\bin\\Debug
curl -LO https://discord.foxbot.me/binaries/win32/libsodium.dll
cp libsodium.dll Sanara-UnitTests\\bin\\Debug
mv libsodium.dll SanaraV2\\bin\\Debug
curl -LO https://dsharpplus.emzi0767.com/natives/ffmpeg_win32_x64.zip
yes | unzip ffmpeg_win32_x64.zip
rm ffmpeg_win32_x64.zip
cp ffmpeg.exe Sanara-UnitTests\\bin\\Debug
mv ffmpeg SanaraV2\\bin\\Debug
curl -LO https://yt-dl.org/downloads/2019.04.30/youtube-dl.exe
cp youtube-dl.exe Sanara-UnitTests\\bin\\Debug
mv youtube-dl.exe SanaraV2\\bin\\Debug
echo -e "\n\e[92mInstallation succeed, don't forget to compile Sanara using Visual Studio\nDefault output directory is at SanaraV2/bin/Debug."
echo -e "\nBefore launching Sanara, you must start ReThinkdb.\e[0m"