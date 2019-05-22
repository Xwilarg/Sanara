/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Xunit;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace Sanara_UnitTests
{
    public sealed class SkipIfNoToken : FactAttribute
    {
        private static bool inamiLoad = false, ayamiLoad = false;
        private static DiscordSocketClient inamiClient;
        public static ITextChannel chan;
        public static string ayamiMention;
        public static ulong ayamiId;

        public SkipIfNoToken()
        {
            Timeout = 30000;

            string ayamiToken, inamiToken, guildTesting, channelTesting;
            if (File.Exists("Keys/ayamiToken.txt"))
                ayamiToken = File.ReadAllText("Keys/ayamiToken.txt");
            else
                ayamiToken = Environment.GetEnvironmentVariable("AYAMI_TOKEN");
            if (File.Exists("Keys/inamiToken.txt"))
                inamiToken = File.ReadAllText("Keys/inamiToken.txt");
            else
                inamiToken = Environment.GetEnvironmentVariable("INAMI_TOKEN");
            if (File.Exists("Keys/guildTesting.txt"))
                guildTesting = File.ReadAllText("Keys/guildTesting.txt");
            else
                guildTesting = Environment.GetEnvironmentVariable("GUILD_TESTING");
            if (File.Exists("Keys/channelTesting.txt"))
                channelTesting = File.ReadAllText("Keys/channelTesting.txt");
            else
                channelTesting = Environment.GetEnvironmentVariable("CHANNEL_TESTING");
            if (ayamiToken == null || inamiToken == null || guildTesting == null || channelTesting == null)
            {
                Skip = "No token found in files or environment variables";
                return;
            }
            inamiClient = new DiscordSocketClient();
            inamiClient.LoginAsync(TokenType.Bot, inamiToken).GetAwaiter().GetResult();
            inamiClient.Connected += ConnectedInami;
            inamiClient.StartAsync().GetAwaiter().GetResult();
            while (!inamiLoad) // Waiting for Inami to connect...
            { }
            SanaraV2.Program ayamiProgram = new SanaraV2.Program();
            ayamiProgram.client.Connected += ConnectedAyami;
            ayamiProgram.MainAsync(ayamiToken, inamiClient.CurrentUser.Id).GetAwaiter().GetResult();
            while (!ayamiLoad) // Waiting for Ayami to connect...
            { }
            chan = inamiClient.GetGuild(ulong.Parse(guildTesting)).GetTextChannel(ulong.Parse(channelTesting));
            ayamiId = ayamiProgram.client.CurrentUser.Id;
            ayamiMention = "<@" + ayamiId + ">";
        }

        private Task ConnectedAyami()
        {
            ayamiLoad = true;
            return Task.CompletedTask;
        }

        private Task ConnectedInami()
        {
            inamiLoad = true;
            return Task.CompletedTask;
        }

        public static void SubscribeMsg(Func<SocketMessage, Task> fct)
        {
            inamiClient.MessageReceived += fct;
        }
    }
}
