# Project CLAUDE Instructions: Visual-GPSS

These instructions are specific to the Visual-GPSS repository and complement personal CLAUDE instructions.

## Project Mission

Recreate and modernize the GPSS ecosystem by delivering:

1. A curated GPSS documentation archive.
2. A GPSS interpreter/runtime in .NET/C#.
3. A visual editor for Desktop/Web/Mobile platforms.

## Language Rules

- User chat discussions: Hungarian.
- Project documentation: English.
- Source code, identifiers, and code comments: English.

## Runtime and Language Baseline

- Target the latest development baseline.
- Current required baseline: .NET 10.0 and C# 14.
- Prefer `.slnx` files instead of `.sln` for solutions.

## Testing Policy

- Unit testing framework: xUnit.
- Integration testing framework: Reqnroll.
- E2E testing framework: Reqnroll.

## Architectural Priorities

- Preserve core GPSS semantics where feasible.
- Keep parser, semantic model, and runtime engine separated.
- Ensure deterministic simulation results for repeatable runs.
- Design for future interoperability between text GPSS and visual models.

## Quality Expectations

- Add tests for every new runtime behavior.
- Document unsupported GPSS features explicitly.
- Prefer small, reviewable increments over large refactors.
