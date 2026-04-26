# AeroCAD Sample Plugin

This project demonstrates an external entity and command.

- `XMarkerEntity` is a custom entity.
- `XMarkerRenderStrategy` and `XMarkerBoundsStrategy` integrate it with rendering and picking.
- `XMarkerGripPreviewStrategy` adds the orange dashed helper preview while moving the marker grip.
- `XMarkerModule` registers the entity plugin and the `XMARK` / `XM` command through the builder APIs.

Build the project and copy `AeroCAD.SamplePlugin.dll` to the WPF app's `Extensions` folder next to `AeroCAD.View.exe`, then restart the app.
