namespace Sanara.Help.Data;

public class Submodule
{
    public string Name { set; get; }
    public string Description { set; get; }
    public IEnumerable<Command> Commands { set; get; }
}
