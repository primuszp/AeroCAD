# AeroCAD Extensions

This project is designed to be extended through three public registries:

- `ICadModuleCatalog`
- `IEntityPluginCatalog`
- `IInteractiveCommandRegistry`

Use `ModelSpace.RegisterModule(...)` to add a third-party module. The host will discover:

- entity plugins
- interactive commands
- editor commands

## Minimal third-party module

```csharp
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Plugins;
using Primusz.AeroCAD.Core.Rendering;
using Primusz.AeroCAD.Core.Spatial;

public sealed class MyCompanyModule : CadModuleBase
{
    public override string Name => "MyCompany.MyModule";

    public override IEnumerable<IEntityPlugin> Plugins
    {
        get { yield return new MyCustomEntityPlugin(); }
    }

    public override IEnumerable<InteractiveCommandRegistration> InteractiveCommands
    {
        get
        {
            yield return new InteractiveCommandRegistration(
                "MYCMD",
                layerProvider => new MyCustomCommandController(layerProvider),
                aliases: new[] { "MC" },
                description: "Run my custom command.",
                menuGroup: "Modify",
                menuLabel: "_MyCmd");
        }
    }
}

public sealed class MyCustomEntityPlugin : EntityPluginBase
{
    protected override string PluginName => "MyCompany.MyEntity";
    protected override EntityPluginCapability Capabilities =>
        EntityPluginCapability.Render |
        EntityPluginCapability.Bounds |
        EntityPluginCapability.InteractiveCommand;

    protected override IEntityRenderStrategy RenderStrategy => new MyCustomRenderStrategy();
    protected override IEntityBoundsStrategy BoundsStrategy => new MyCustomBoundsStrategy();
}
```

## What to implement

- `IEntityPlugin`
  - provide render, bounds, preview, offset, trim/extend strategies as needed
- `InteractiveCommandRegistration`
  - provide a controller factory and command metadata
- `EditorCommandDefinition`
  - use this for non-interactive commands

## Querying at runtime

The host exposes catalog services so extensions can be discovered and filtered:

- `ICadModuleCatalog.Find(name)`
- `IEntityPluginCatalog.Find(name)`
- `IEntityPluginCatalog.GetByCapability(capability)`
- `IInteractiveCommandRegistry.Find(commandName)`

## Capability guidance

Use `EntityPluginCapability` to declare what your plugin supports.
Prefer explicit capabilities over implicit behavior.

Recommended minimum for a geometry entity:

- `Render`
- `Bounds`

Optional, when implemented:

- `GripPreview`
- `SelectionMovePreview`
- `TransientPreview`
- `Offset`
- `TrimExtend`
- `InteractiveCommand`

