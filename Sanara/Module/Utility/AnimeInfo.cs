namespace Sanara.Module.Utility;

public record AnimeInfo
{
    public AnimeData data;
}

public record AnimeData
{
    public AnimeContainer Page;
}

public record AnimeContainer
{
    public AnimeResult[] media;
    public AiringSchedule[] airingSchedules;
}

public record AiringSchedule
{
    public int id;
    public int episode;
    public AnimeResult media;
    public int airingAt;
}

public record AnimeResult
{
    public int id;
    public AnimeTitle title;
    public bool isAdult;
    public string description;
    public AnimeCover coverImage;
    public int? averageScore;
    public int? episodes;
    public int? duration;
    public FuzzyDate startDate;
    public FuzzyDate endDate;
    public string source;
    public string format;
    public string type;
    public AnimeTag[] tags;
    public string[] genres;
}

public record AnimeTitle
{
    public string romaji;
    public string native;
    public string english;
}

public record AnimeCover
{
    public string large;
}

public record FuzzyDate
{
    public int? year;
    public int? month;
    public int? day;
}

public record AnimeTag
{
    public string name;
    public int rank;
}
