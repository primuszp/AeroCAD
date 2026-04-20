using System.Collections.Generic;
using Primusz.AeroCAD.Core.Plugins;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class InteractiveCommandRegistryTests
    {
        [Fact]
        public void Registrations_ContainModuleAndPluginInteractiveCommands()
        {
            var registry = new InteractiveCommandRegistry(
                new IEntityPlugin[] { new LineEntityPlugin() },
                new ICadModule[] { new BuiltInModifyModule() });

            Assert.Contains(registry.Registrations, registration => registration.CommandName == "LINE");
            Assert.Contains(registry.Registrations, registration => registration.CommandName == "MOVE");
            Assert.Contains(registry.Registrations, registration => registration.CommandName == "COPY");
        }

        [Fact]
        public void Find_ReturnsRegistrationByCommandName()
        {
            var registry = new InteractiveCommandRegistry(
                new IEntityPlugin[] { new LineEntityPlugin() },
                new ICadModule[] { new BuiltInModifyModule() });

            var registration = registry.Find("MOVE");

            Assert.NotNull(registration);
            Assert.Equal("MOVE", registration.CommandName);
        }
    }
}
