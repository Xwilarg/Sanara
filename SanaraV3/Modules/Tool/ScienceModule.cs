using Discord.Commands;
using System.Data;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadScienceHelp()
        {
            _help.Add(new Help("Calc", new[] { new Argument(ArgumentType.MANDATORY, "operation") }, "Evaluate a basic math operation and return the result.", false));
        }
    }
}

namespace SanaraV3.Modules.Tool
{
    public class ScienceModule : ModuleBase
    {
        [Command("Calc")]
        public async Task CalcAsync([Remainder]string operation)
        {
            DataTable table = new DataTable();
            try
            {
                await ReplyAsync(table.Compute(operation, "").ToString());
            }
            catch (EvaluateException)
            {
                await ReplyAsync("I can't calculate the expression you gave");
            }
            catch (SyntaxErrorException)
            {
                await ReplyAsync("I can't calculate the expression you gave");
            }
        }
    }
}
