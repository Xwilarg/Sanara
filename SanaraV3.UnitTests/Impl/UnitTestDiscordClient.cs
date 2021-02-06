using Discord;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Impl
{
    public sealed class UnitTestDiscordClient : IDiscordClient
    {
        public ConnectionState ConnectionState => ConnectionState.Connected;

        public ISelfUser CurrentUser => null;

        public TokenType TokenType => TokenType.Bot;

        public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream jpegIcon = null, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public Task<IApplication> GetApplicationInfoAsync(RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<BotGateway> GetBotGatewayAsync(RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IGuild> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> GetRecommendedShardCountAsync(RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task StartAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
