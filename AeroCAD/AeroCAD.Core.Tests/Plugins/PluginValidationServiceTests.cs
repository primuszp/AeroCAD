using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Plugins
{
    public class PluginValidationServiceTests
    {
        [Fact]
        public void Validate_ReportsDuplicateCommandAliases()
        {
            var first = CreatePlugin("Plugin.One", "TEST", "T");
            var second = CreatePlugin("Plugin.Two", "OTHER", "T");
            var result = new PluginValidationService().Validate(new[] { first, second }, new ICadModule[0]);

            Assert.True(result.HasErrors);
            Assert.Contains(result.Issues, issue => issue.Message.Contains("Command alias 'T'"));
        }

        [Fact]
        public void Validate_AllowsExplicitCommandReplacement()
        {
            var first = CreatePlugin("Plugin.One", "TEST", "T");
            var module = new ReplacementModule();
            var result = new PluginValidationService().Validate(new[] { first }, new ICadModule[] { module });

            Assert.False(result.HasErrors);
        }

        [Fact]
        public void Validate_ReportsIncompatibleManifestVersion()
        {
            var module = new FutureModule();
            var result = new PluginValidationService(new System.Version(1, 0)).Validate(new IEntityPlugin[0], new[] { module });

            Assert.True(result.HasErrors);
            Assert.Contains(result.Issues, issue => issue.Message.Contains("requires AeroCAD Core"));
        }

        private static IEntityPlugin CreatePlugin(string pluginName, string commandName, params string[] aliases)
        {
            return EntityPluginBuilder
                .Create(pluginName)
                .WithRenderStrategy(new NoOpRenderStrategy())
                .WithBoundsStrategy(new NoOpBoundsStrategy())
                .WithInteractiveCommand(InteractiveCommandRegistrationBuilder.Create(commandName).WithAliases(aliases).Build())
                .BuildPlugin();
        }

        private sealed class FutureModule : CadModuleBase
        {
            public override string Name => "Future.Module";
            public override PluginManifest Manifest => new PluginManifest(Name, version: "1.0.0", minimumCoreVersion: new System.Version(99, 0));
            public override IEnumerable<IEntityPlugin> Plugins { get { yield break; } }
        }

        private sealed class ReplacementModule : CadModuleBase
        {
            public override string Name => "Replacement.Module";
            public override IEnumerable<IEntityPlugin> Plugins { get { yield break; } }
            public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
            {
                get
                {
                    yield return new InteractiveCommandRegistration(
                        "TEST",
                        () => null,
                        aliases: new[] { "T" },
                        replaceExistingCommand: true);
                }
            }
        }

        private sealed class NoOpRenderStrategy : IEntityRenderStrategy
        {
            public bool CanHandle(Entity entity) => false;
            public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context) { }
        }

        private sealed class NoOpBoundsStrategy : IEntityBoundsStrategy
        {
            public bool CanHandle(Entity entity) => false;
            public Rect GetBounds(Entity entity) => Rect.Empty;
        }
    }
}
