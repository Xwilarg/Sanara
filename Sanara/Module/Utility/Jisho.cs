namespace Sanara.Module.Utility;

public class Jisho
{
    public JishoData[] Data { set; get; }
}

public class JishoData
{
    public string Slug { set; get; }
    public string[] Jlpt { set; get; }
    public JishoJapanese[] Japanese { set; get; }
    public JishoSense[] Senses { set; get; }
}

public class JishoSense
{
    public string[] EnglishDefinitions { set; get; }
    public string[] Tags { set; get; }
    public string[] Info { set; get; }
}

public class JishoJapanese
{
    public string Word { set; get; }
    public string Reading { set; get; }
}

