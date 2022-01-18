using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using Sanara.Exception;
using Sanara.Module.Command;
using System.IO.Compression;

namespace Sanara.Module.Button
{
    public class Doujinshi
    {
        public static async Task DownloadDoujinshiAsync(ICommandContext ctx, string id)
        {
            string path = id + "_" + DateTime.Now.ToString("HHmmssff") + StaticObjects.Random.Next(0, int.MaxValue);
            Directory.CreateDirectory("Saves/Download/" + path); // Folder that contains the ZIP
            Directory.CreateDirectory("Saves/Download/" + path + "/" + id); // Folder that will inside the ZIP
            GalleryElement elem;
            try
            {
                elem = await SearchClient.SearchByIdAsync(int.Parse(id));
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("There is no doujinshi with this id.");
            }

            int i = 1;
            foreach (var page in elem.pages)
            {
                string extension = "." + page.format.ToString().ToLower();
                // Write each page in the folder
                File.WriteAllBytes($"Saves/Download/{path}/{id}/{i:D3}{extension}",
                    await StaticObjects.HttpClient.GetByteArrayAsync("https://i.nhentai.net/galleries/" + elem.mediaId + "/" + i + extension));
                i++;
            }
            string finalPath = "Saves/Download/" + path + "/" + id + ".zip";
            ZipFile.CreateFromDirectory("Saves/Download/" + path + "/" + id, "Saves/Download/" + path + "/" + id + ".zip");

            // Delete all files
            for (i = Directory.GetFiles("Saves/Download/" + path + "/" + id).Length - 1; i >= 0; i--)
                File.Delete(Directory.GetFiles("Saves/Download/" + path + "/" + id)[i]);

            // Delete folder
            Directory.Delete("Saves/Download/" + path + "/" + id);

            FileInfo fi = new(finalPath);
            if (fi.Length < 8000000) // 8MB
            {
                await ctx.ReplyAsync(new FileStream(finalPath, FileMode.Open), fi.Name);
            }
            else
            {
                Directory.CreateDirectory(StaticObjects.UploadWebsiteLocation + path);
                File.Copy(finalPath, StaticObjects.UploadWebsiteLocation + path + "/" + id + ".zip");
                await ctx.ReplyAsync(StaticObjects.UploadWebsiteUrl + path + "/" + id + ".zip" + Environment.NewLine + "You file will be deleted after 10 minutes.");
                _ = Task.Run(async () =>
                {
                    await Task.Delay(600000); // 10 minutes
                    File.Delete(StaticObjects.UploadWebsiteLocation + path + "/" + id + ".zip");
                    Directory.Delete(StaticObjects.UploadWebsiteLocation + path);
                });
            }
            File.Delete(finalPath);
            Directory.CreateDirectory("Saves/Download/" + path + "/" + id);
        }
    }
}
