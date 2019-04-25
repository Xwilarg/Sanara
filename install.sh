#!/bin/sh
set -e
curl -LO https://download.rethinkdb.com/windows/rethinkdb-2.3.6.zip
yes | unzip rethinkdb-2.3.6.zip
rm rethinkdb-2.3.6.zip
if [ ! -d SanaraV2\\bin\\Debug\\Saves ]
then
	mkdir -p SanaraV2\\bin\\Debug\\Saves
fi
if [ ! -d Sanara-UnitTests\\bin\\Debug\\Saves ]
then
	mkdir -p Sanara-UnitTests\\bin\\Debug\\Saves
fi
curl -LO https://files.zirk.eu/Sanara/AnimeTags.dat
cp AnimeTags.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv AnimeTags.dat SanaraV2\\bin\\Debug\\Saves
curl -LO https://files.zirk.eu/Sanara/BooruTriviaTags.dat
cp BooruTriviaTags.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv BooruTriviaTags.dat SanaraV2\\bin\\Debug\\Saves
curl -LO https://files.zirk.eu/Sanara/shiritoriWords.dat
cp shiritoriWords.dat Sanara-UnitTests\\bin\\Debug\\Saves
mv shiritoriWords.dat SanaraV2\\bin\\Debug\\Saves
echo -e "\n\e[92mInstallation succeed, don't forget to compile Sanara using Visual Studio\nDefault output directory is at SanaraV2/bin/Debug."
echo -e "\nBefore launching Sanara, you must start ReThinkdb.\e[0m"