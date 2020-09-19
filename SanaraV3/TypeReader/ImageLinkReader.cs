using Discord.Commands;
using DiscordUtils;
using SanaraV3.CustomClass;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SanaraV3.TypeReader
{
    public sealed class ImageLinkReader : Discord.Commands.TypeReader
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (!await Utils.IsLinkValid(input) || !Utils.IsImage(Path.GetExtension(input)))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "The given argument isn't a valid link to an image.");
            return TypeReaderResult.FromSuccess(new ImageLink() { Link = input });
        }
    }
}
