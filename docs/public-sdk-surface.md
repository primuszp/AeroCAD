# AeroCAD Public SDK Surface

This document defines the Core types that external plugin authors can depend on.
The SDK is currently hosted inside `AeroCAD.Core`; it is not a separate assembly yet.

## Stable Plugin Entry Points

- `Primusz.AeroCAD.Core.Plugins.ICadModule`
- `Primusz.AeroCAD.Core.Plugins.CadModuleBase`
- `Primusz.AeroCAD.Core.Plugins.IEntityPlugin`
- `Primusz.AeroCAD.Core.Plugins.EntityPluginBuilder`
- `Primusz.AeroCAD.Core.Plugins.EntityPluginDescriptor`
- `Primusz.AeroCAD.Core.Plugins.EntityPluginCapability`
- `Primusz.AeroCAD.Core.Plugins.InteractiveCommandRegistrationBuilder`
- `Primusz.AeroCAD.Core.Plugins.InteractiveCommandRegistration`
- `Primusz.AeroCAD.Core.Plugins.InteractiveCommandContext`
- `Primusz.AeroCAD.Core.Plugins.PluginManifest`

## Stable Entity Model

- `Primusz.AeroCAD.Core.Drawing.Entities.Entity`
- `Primusz.AeroCAD.Core.Drawing.Entities.CustomEntityBase`
- `Primusz.AeroCAD.Core.Drawing.Entities.EntityColor`
- `Primusz.AeroCAD.Core.Drawing.Entities.EntityColorKind`
- `Primusz.AeroCAD.Core.Drawing.Entities.EntityCommandHighlightKind`
- `Primusz.AeroCAD.Core.Drawing.Handles.GripDescriptor`
- `Primusz.AeroCAD.Core.Drawing.Handles.GripKind`
- `Primusz.AeroCAD.Core.Snapping.ISnapDescriptor`
- `Primusz.AeroCAD.Core.Snapping.SnapPointDescriptor`
- `Primusz.AeroCAD.Core.Snapping.ComputedSnapDescriptor`
- `Primusz.AeroCAD.Core.Snapping.SnapType`

Prefer `CustomEntityBase` for new external entities. It standardizes:

- `Clone`: copies geometry, style, and identity.
- `Duplicate`: copies geometry and style, but creates a new identity.
- `RestoreState`: restores geometry and base visual properties, then invalidates once.
- `InvalidateEntityGeometry`: a protected helper for custom geometry changes.

## Stable Strategy Base Classes

- `Primusz.AeroCAD.Core.Rendering.EntityRenderStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Spatial.EntityBoundsStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Editing.GripPreviews.GripPreviewStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Editing.MovePreviews.SelectionMovePreviewStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Editing.TransientPreviews.TransientEntityPreviewStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Editing.Offsets.EntityOffsetStrategy<TEntity>`
- `Primusz.AeroCAD.Core.Editing.TrimExtend.EntityTrimExtendStrategy<TEntity>`

These classes adapt typed plugin code to the engine interfaces and remove repeated
`CanHandle`/cast boilerplate from external strategies.

## Stable Command Types

- `Primusz.AeroCAD.Core.Editor.CommandStep`
- `Primusz.AeroCAD.Core.Editor.CommandPrompt`
- `Primusz.AeroCAD.Core.Editor.CommandInputToken`
- `Primusz.AeroCAD.Core.Editor.CommandKeywordOption`
- `Primusz.AeroCAD.Core.Editor.EditorCommandPolicy`
- `Primusz.AeroCAD.Core.Tools.InteractiveCommandResult`
- `Primusz.AeroCAD.Core.Tools.PointSequenceCommandControllerBase`

Plugin commands should prefer `InteractiveCommandRegistrationBuilder` and
`InteractiveCommandContext` over deriving directly from the internal controller stack.
For command flows that collect multiple points before creating an entity, prefer
`PointSequenceCommandControllerBase` over deriving directly from `CommandControllerBase`.

## Internal Engine Details

External plugins should not depend directly on these unless a future SDK document
explicitly promotes them:

- `CommandControllerBase`
- `ToolService`
- `ModelSpaceRuntimeBootstrapper`
- registry implementations such as `InteractiveCommandRegistry`
- WPF view/viewmodel classes in `AeroCAD.View`
- concrete runtime bootstrapping and discovery implementation details

## Compatibility Rule

Types listed in this document are treated as the public Core SDK. Breaking changes
to this surface should be intentional, tested against `AeroCAD.SamplePlugin`, and
reflected here.
