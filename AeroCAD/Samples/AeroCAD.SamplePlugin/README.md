# AeroCAD Sample Plugin

This project demonstrates an external `ROADPLAN` entity and command.

- `RoadPlanEntity` stores alignment vertices and per-vertex curve parameters.
- `RoadPlanRenderStrategy` renders straight segments with circular fillets at curved vertices.
- `RoadPlanBoundsStrategy` integrates the entity with picking/spatial queries.
- `RoadPlanModule` registers `ROADPLAN` / `RP`.

Current scope:

- The model stores radius plus in/out transition lengths for future clothoid support.
- Rendering currently draws the circular arc portion between adjacent tangents.
- The command creates a demo alignment from an insertion point, using the sample coordinates from the road-plan scenario.

The View project builds this plugin automatically and copies `AeroCAD.SamplePlugin.dll` to the WPF app's `Extensions` folder next to `AeroCAD.View.exe`.
