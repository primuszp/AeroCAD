# AeroCAD

Command-driven 2D CAD editor built with WPF on .NET 8.

## Features

- 2D entities: `Line`, `Polyline`, `Circle`, `Arc`
- command line and mouse-driven input
- object snap, grip edit, ortho, adaptive grid
- undo/redo
- modify commands: `MOVE`, `COPY`, `OFFSET`, `TRIM`, `EXTEND`

## Structure

- `AeroCAD/AeroCAD.sln`
- `AeroCAD/AeroCAD.Core`
- `AeroCAD/AeroCAD.Presentation`
- `docs`

## Build

```powershell
dotnet build .\AeroCAD\AeroCAD.sln -p:UseSharedCompilation=false
```

## Rights

Copyright (c) Primusz Peter. All rights reserved.

No license is granted for use, modification, distribution, or commercial use without prior written permission from the author.
