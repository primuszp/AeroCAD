# AeroCAD

Command-driven 2D CAD editor built with WPF on .NET 8.

## Features

- 2D entities: `Line`, `Polyline`, `Circle`, `Arc`
- command line and mouse-driven input
- object snap, grip edit, ortho, adaptive grid
- undo/redo
- modify commands: `MOVE`, `COPY`, `OFFSET`, `TRIM`, `EXTEND`
- extension loading from `Extensions/*.dll` next to the WPF executable

## Structure

- `AeroCAD/AeroCAD.sln`
- `AeroCAD/AeroCAD.Core`
- `AeroCAD/AeroCAD.Presentation`
- `docs`

## Build

```powershell
dotnet build .\AeroCAD\AeroCAD.sln -p:UseSharedCompilation=false
```

## External entity plugins

External assemblies can expose `ICadModule` or `IEntityPlugin` implementations with public parameterless constructors. Put the compiled DLL in the app's `Extensions` folder before startup. Entity behavior is discovered from the plugin descriptor: render and bounds are required; grip preview, move preview, transient preview, offset, trim/extend, tools, and command-line registrations are optional capabilities.

Use `EntityPluginBuilder` and `InteractiveCommandRegistrationBuilder` for new external entities and commands without subclassing the internal command controller stack directly:

```csharp
yield return EntityPluginBuilder
    .Create("Vendor.MyEntity")
    .WithRenderStrategy(new MyEntityRenderStrategy())
    .WithBoundsStrategy(new MyEntityBoundsStrategy())
    .WithInteractiveCommand(
        InteractiveCommandRegistrationBuilder
            .Create("MYENTITY")
            .WithAliases("MY")
            .WithInitialStep(new CommandStep("Point", "Specify point:"))
            .OnPoint((context, point) =>
            {
                context.AddEntity(new MyEntity(point));
                return context.End("MYENTITY created.");
            })
            .Build())
    .BuildPlugin();
```

See `AeroCAD/Samples/AeroCAD.SamplePlugin` for a buildable external plugin that registers the `XMARK` command and a custom `XMarkerEntity`.

## Rights

Copyright (c) Primusz Peter. All rights reserved.

No license is granted for use, modification, distribution, or commercial use without prior written permission from the author.
