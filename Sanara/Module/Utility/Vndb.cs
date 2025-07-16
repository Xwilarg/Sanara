namespace Sanara.Module.Utility;

public class VndbReq<T>
{
    public T[] Results { set; get; }
}

public class VnResult
{
    public VnInfo Vn { set; get; }
    public string Quote { set; get; }
}

public class VnInfo
{
    public string Id { set; get; }
    public string Title { set; get; }
    public VnImage Image { set; get; }
    public string[] Languages { set; get; }
    public int? Length { set; get; }
    public string[] Platforms { set; get; }
    public float? Rating { set; get; }
    public string Released { set; get; }
    public string Description { set; get; }
}

public class VnImage
{
    public string Url { set; get; }
    public float Sexual { set; get; }
    public float Violence { set; get; }
}