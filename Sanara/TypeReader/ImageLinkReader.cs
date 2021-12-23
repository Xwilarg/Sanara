using Discord.Commands;
using Sanara.CustomClass;

namespace Sanara.TypeReader
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
