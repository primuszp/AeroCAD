using System;
using Primusz.AeroCAD.Core.Editor;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class EditorCommandCatalogTests
    {
        [Fact]
        public void Register_ThrowsOnDuplicateAlias()
        {
            var catalog = new EditorCommandCatalog();

            catalog.Register(new EditorCommandDefinition("FIRST", aliases: new[] { "F" }));

            Assert.Throws<InvalidOperationException>(() =>
                catalog.Register(new EditorCommandDefinition("SECOND", aliases: new[] { "F" })));
        }

        [Fact]
        public void Register_ReplacesExistingCommandWhenExplicitlyRequested()
        {
            var catalog = new EditorCommandCatalog();

            catalog.Register(new EditorCommandDefinition("LINE", description: "Old line."));
            catalog.Register(new EditorCommandDefinition("LINE", description: "New line.", replaceExistingCommand: true));

            Assert.True(catalog.TryResolve("LINE", out var definition));
            Assert.Equal("New line.", definition.Description);
        }
    }
}
