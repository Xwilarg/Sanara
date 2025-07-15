namespace Sanara.Module.Utility;

public class VndbReq
{
    public VnResult[] Results { set; get; }
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
}

public class VnImage
{
    public string Url { set; get; }
}