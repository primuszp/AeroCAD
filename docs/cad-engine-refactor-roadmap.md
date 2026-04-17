# CAD Engine Refactor Roadmap

## Target Architecture

The long-term goal is a three-layer architecture:

1. `Domain`
   Pure CAD document model, entities, editing rules, snap descriptors, grip descriptors, transactions.
2. `Editor`
   Tool runtime, interaction state machine, command orchestration, selection workflow, undo/redo coordination.
3. `Presentation`
   WPF viewport, renderers, transient overlays, cursor visuals, input event bridging.

## Current Refactor Status

Implemented in this slice:

- Introduced a central editor state service.
- Moved cursor policy and interaction mode decisions behind `EditorStateService`.
- Centralized active layer tracking behind editor state.
- Reduced direct viewport cursor mutation from tools.
- Introduced a central `CadDocumentService` for layer/entity ownership.
- Routed layer creation through the document instead of direct viewport mutation.
- Routed add-entity commands through document ownership instead of direct layer mutation.
- Moved grip preview generation out of entities into dedicated preview strategies.
- Routed transient grip preview creation through an editing service instead of entity-owned preview code.
- Introduced strategy-based entity rendering.
- Moved concrete WPF line/polyline drawing out of entity classes into a render service.
- Turned `Layer` into the render host for entity visuals instead of letting entities draw themselves directly.
- Introduced a document-driven spatial query service for broadphase snap and hit-test candidate lookup.
- Added entity geometry change tracking so spatial data stays in sync during grip edits and command restores.
- Replaced provider-driven osnap enumeration with descriptor-based snap definitions and a central snap mode policy.

## Remaining Phases

### Phase 1: Document Model

- Introduce `CadDocument`.
- Move layer/entity ownership from viewport visuals to document state.
- Give entities stable IDs.

### Phase 2: Rendering Separation

- Remove WPF inheritance from domain entities.
- Introduce render adapters per entity type.
- Move grip preview geometry creation out of domain entities into editor/presentation policies.

### Phase 3: Editor State Machine

- Replace tool suspension with explicit editor states.
- Route input through a command/state machine instead of iterating all tools.
- Model selection, command input, grip editing, and pan as state transitions.

### Phase 4: Transactional Editing

- Replace visual/entity snapshot mutation with document transactions.
- Define commands over document IDs and value objects.
- Keep undo/redo independent from WPF visuals.

### Phase 5: Grip and Snap Descriptors

- Introduce descriptor-based grips and snap points.
- Support endpoint, midpoint, center, intersection, tangent, and custom snap modes through policies.
- Keep entity edit semantics outside the viewport layer.

### Phase 6: Performance and Scalability

- Add spatial indexing for hit testing and snapping.
- Stop scanning all entities on every mouse move.
- Separate model invalidation from full viewport invalidation.

## Implementation Rule

Any new feature should align with the target split:

- domain rule in `Domain`
- interaction flow in `Editor`
- drawing/input bridging in `Presentation`

Do not add new feature logic directly into WPF controls unless it is purely visual.
