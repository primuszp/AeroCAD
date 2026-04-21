# AeroCAD Architecture Roadmap

## Goal

Make the CAD engine easier to extend by third parties without turning the codebase into a generic plugin framework.

The target shape is:

- a small, stable kernel for geometry and entity state
- a runtime layer for commands, selection, snapping, and plugin discovery
- a thin view layer that only renders and forwards input
- explicit extension points for modules, entity plugins, and commands

## Current strengths

- Entity behavior is already split with strategy classes.
- Trim/extend logic is test-covered and isolated from the UI.
- Module and registry concepts already exist.
- Command flow is mostly consistent and reusable.

## Constraints

- Do not introduce broad abstractions unless a concrete extension need exists.
- Keep existing commands and entity types working.
- Prefer additive changes over rewrites.
- Every new extension point must be covered by tests.

## Architecture target

### 1. Kernel

Contains:

- entity data and geometry
- snap descriptors
- trim/extend/offset logic
- bounds and geometry helpers

Must not contain:

- view code
- command line orchestration
- UI-specific state

### 2. Runtime

Contains:

- command lifecycle
- selection and hover processing
- snapping and grip handling
- plugin/module registration
- undo/redo orchestration

### 3. View

Contains:

- viewport drawing
- command line UI
- mouse and keyboard forwarding
- status text binding

The view layer must stay thin.

## Extension points to keep public

- `ICadModule`
- `IEntityPlugin`
- `IInteractiveCommandRegistry`
- `ICadModuleCatalog`
- `IEntityPluginCatalog`
- `EntityPluginCapability`
- command registration metadata

## Extension points to keep internal

- `MainViewModel`
- concrete command controller internals
- geometry helpers
- trim/extend strategies
- command repeat coordinator internals

## Refactor plan

### Phase 1: Stabilize the runtime boundary

Focus:

- command repeat behavior
- command lifecycle
- hover/snap status calculation

Deliverables:

- a small service for command lifecycle decisions
- a small service for hover and snap feedback
- tests for repeat, enter, space, and idle hover behavior

Acceptance criteria:

- the view model only forwards input and updates bindings
- repeat behavior is covered by tests
- hover feedback uses one consistent path

### Phase 2: Normalize interactive commands

Focus:

- line, polyline, circle, arc, rectangle controller cleanup
- shared keyword handling
- shared finish/cancel cleanup

Deliverables:

- a smaller common controller base
- less duplicated keyword handling
- consistent command naming

Acceptance criteria:

- adding a new interactive command follows one clear pattern
- existing commands still behave the same
- no new UI coupling leaks into the core

### Phase 3: Finalize plugin discovery and capability model

Focus:

- module discovery
- entity plugin discovery
- command registration discovery
- capability-based queries

Deliverables:

- one consistent registration path for third-party modules
- capability metadata for entity plugins
- tests for catalog and registry queries

Acceptance criteria:

- a third-party assembly can register a module without touching core code
- the runtime can query capabilities without hardcoding entity types

### Phase 4: Introduce plugin-aware document persistence

Focus:

- document schema
- entity serialization
- versioned load/save flow

Deliverables:

- a document format contract
- serializer registration per entity/plugin
- fallback behavior for unsupported entities

Acceptance criteria:

- built-in entities can be saved and loaded
- plugin entities can participate through serializer registration

### Phase 5: Add style and property extensibility

Focus:

- layer style
- entity extended properties
- override precedence

Deliverables:

- typed extended property access
- layer style container
- clear rendering precedence rules

Acceptance criteria:

- plugin-defined metadata can be stored on entities
- styling rules are explicit and testable

## Recommended implementation order

1. Command lifecycle service
2. Hover/snap feedback service
3. Interactive controller cleanup
4. Plugin discovery finalization
5. Persistence contract
6. Style and property extensibility

## Testing strategy

Every phase must add or update tests in one of these categories:

- `Functional`
- `Regression`
- `Catalog`
- `Runtime`

Rules:

- fix the regression with a test first when possible
- keep geometry tests close to trim/extend helpers
- keep command lifecycle tests isolated from the view

## Non-goals

- Do not switch to ECS.
- Do not introduce a full visitor framework.
- Do not over-abstract the current strategy classes.
- Do not replace the current view architecture with a new UI stack.

## Working rule

Any future change should answer these questions first:

1. Can a third-party module add this without modifying core code?
2. Is the logic in the correct layer?
3. Is there a test that will catch the regression?
4. Is the API stable enough to be used by someone else?

