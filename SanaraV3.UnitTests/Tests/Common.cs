using Discord.Commands;
using SanaraV3.UnitTests.Impl;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SanaraV3.UnitTests.Tests
{
    public static class Common
    {
        /// <summary>
        /// Create and assign a context to a module
        /// Since the SetContext method is private and from an interface we need to load the assemble and une reflection to get the method
        /// </summary>
        public static void AddContext(ModuleBase module, Func<UnitTestUserMessage, Task> callback)
        {
            var assembly = Assembly.LoadFrom("Discord.Net.Commands.dll");
            var method = assembly.GetType("Discord.Commands.IModuleBase").GetMethod("SetContext", BindingFlags.Instance | BindingFlags.Public);
            var context = new CommandContext(new UnitTestDiscordClient(), new UnitTestUserMessage(callback));
            method.Invoke(module, new[] { context });
        }
    }
}
