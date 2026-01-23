namespace Sanara.Module.Utility.Danbooru;

public class GelbooruJson
{
    public GelbooruPost[]? post { set; get; }
}

public class GelbooruPost
{
    public int id { set; get; }
    public string rating { set; get; }
    public string file_url { set; get; }
    public string preview_url { set; get; }
    public string tags { set; get; }
    public int width { set; get; }
    public int height { set; get; }
    public string source { set; get; }
}