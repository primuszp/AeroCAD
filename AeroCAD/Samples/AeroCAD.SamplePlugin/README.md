# AeroCAD Sample Plugin

This project demonstrates an external `ROADPLAN` entity and command.

- `RoadPlanEntity` stores alignment vertices and per-vertex curve parameters on top of `CustomEntityBase`.
- `RoadPlanRenderStrategy` renders straight segments with circular fillets at curved vertices through `EntityRenderStrategy<RoadPlanEntity>`.
- `RoadPlanBoundsStrategy` integrates the entity with picking/spatial queries through `EntityBoundsStrategy<RoadPlanEntity>`.
- `RoadPlanGripPreviewStrategy` uses `GripPreviewStrategy<RoadPlanEntity>` so the sample stays type-safe without repeated casts.
- `RoadPlanModule` registers `ROADPLAN` / `RP`.

Current scope:

- The model stores radius plus in/out transition lengths for future clothoid support.
- Rendering currently draws the circular arc portion between adjacent tangents.
- The command creates an alignment point-by-point from typed coordinates or viewport clicks.
- `Undo`/`U` removes the last point, `Radius`/`R` sets the previous vertex curve radius after at least three points, and `Close`/`C` closes the alignment.
- Enter creates the entity after at least two points and cancels before the first point.

The View project builds this plugin automatically and copies `AeroCAD.SamplePlugin.dll` to the WPF app's `Extensions` folder next to `AeroCAD.View.exe`.
