# AeroCAD Sample Plugin

This project demonstrates an external AutoCAD-like `POINT` entity and command.

- `PointEntity` is a custom external entity with one `Node` grip/snap point.
- `PointRenderStrategy` draws the point using the current `PDMODE` and `PDSIZE` values.
- `PointBoundsStrategy` integrates it with picking/spatial queries.
- `PointGripPreviewStrategy` adds drag preview while moving the point grip.
- `PointModule` registers `POINT`, `PO`, `PUNKT`, `PDMODE`, and `PDSIZE`.

Commands:

- `POINT` / `PO` / `PUNKT`: prompts `Specify a point:` and creates a point object.
- `PDMODE`: sets point display mode. Supported base modes: `0` dot, `1` hidden, `2` plus, `3` asterisk, `4` circle. Flags `32` and `64` add surrounding circle/square shapes.
- `PDSIZE`: sets point display size. `0` uses 5 percent of drawing height, positive values are absolute sizes, negative values are treated as a viewport-height percentage.

The View project builds this plugin automatically and copies `AeroCAD.SamplePlugin.dll` to the WPF app's `Extensions` folder next to `AeroCAD.View.exe`.
