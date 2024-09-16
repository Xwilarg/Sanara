using Sanara.Exception;
using Sanara.Module.Command;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Sanara.Module.Button
{
    public class Cosplay
    {
        public static async Task DownloadCosplayAsync(IContext ctx, string idFirst, string idSecond, string idName)
        {
            string html;
            int nbPages;
            int limitPages;
            html = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/g/" + idFirst + "/" + idSecond);
            var m = Regex.Match(html, "Showing [0-9]+ - ([0-9]+) of ([0-9]+) images");
            if (!m.Success)
                throw new CommandFailed("There is no cosplay with this id.");
            limitPages = int.Parse(m.Groups[1].Value);
            nbPages = int.Parse(m.Groups[2].Value);

            var id = Guid.NewGuid();
            string path = id + "_" + DateTime.Now.ToString("HHmmssff");

            Directory.CreateDirectory("Saves/Download/" + path);
            Directory.CreateDirectory("Saves/Download/" + path + "/" + id);
            int nextPage = 1;
            for (int i = 1; i <= nbPages; i++)
            {
                var imageMatch = Regex.Match(html, "<a href=\"https:\\/\\/e-hentai.org\\/s\\/([a-z0-9]+)\\/" + idFirst + "-" + i + "\">");
                string html2 = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/s/" + imageMatch.Groups[1].Value + "/" + idFirst + "-" + i);
                m = Regex.Match(html2, "<img id=\"img\" src=\"([^\"]+)\"");
                string url = m.Groups[1].Value;
                string extension = "." + url.Split('.').Last();
                File.WriteAllBytes($"Saves/Download/{path}/{id}/{i:D3}{extension}",
                await StaticObjects.HttpClient.GetByteArrayAsync(url));
                if (i == limitPages)
                {
                    html = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/g/" + idFirst + "/" + idSecond + "/?p=" + nextPage);
                    m = Regex.Match(html, "Showing [0-9]+ - ([0-9]+) of [0-9]+ images");
                    limitPages = int.Parse(m.Groups[1].Value);
                    nextPage++;
                }
            }

            string finalPath = "Saves/Download/" + path + "/" + id + ".zip";
            ZipFile.CreateFromDirectory("Saves/Download/" + path + "/" + id, finalPath);
            for (int i = Directory.GetFiles("Saves/Download/" + path + "/" + id).Length - 1; i >= 0; i--)
                File.Delete(Directory.GetFiles("Saves/Download/" + path + "/" + id)[i]);
            Directory.Delete("Saves/Download/" + path + "/" + id);

            FileInfo fi = new(finalPath);
            if (idName == "cosplay")
            {
                await StaticObjects.Db.AddDownloadCosplayAsync((int)(fi.Length / 1000));
            }
            else if (idName == "doujinshi")
            {
                await StaticObjects.Db.AddDownloadDoujinshiAsync((int)(fi.Length / 1000));
            }
            else
            {
                throw new ArgumentException("Invalid downlaod type", nameof(idName));
            }
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
