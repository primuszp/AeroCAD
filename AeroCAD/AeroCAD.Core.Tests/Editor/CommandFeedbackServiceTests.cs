using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class CommandFeedbackServiceTests
    {
        [Fact]
        public void BeginCommand_LogsCommandNameOnce()
        {
            var service = new CommandFeedbackService();
            var messages = new List<string>();
            service.MessageLogged += (_, e) => messages.Add(e.Message);

            service.BeginCommand("polygon", "Specify number of sides [3-1024] <4>:");

            Assert.Equal("Specify number of sides [3-1024] <4>:", service.Prompt);
            Assert.Equal(new[] { "POLYGON" }, messages);
        }

        [Fact]
        public void Prompt_KeepsOriginalDisplayTextAfterPromptUpdate()
        {
            var service = new CommandFeedbackService();

            service.BeginCommand("offset", "Specify offset distance:");
            service.SetPrompt("Specify object to offset:");

            Assert.Equal("Specify object to offset:", service.Prompt);
        }
    }
}
