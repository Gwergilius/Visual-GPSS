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

## Repository Standards

- Every repository must have a `.gitattributes` file that normalizes all text files to LF (`* text=auto eol=lf`). Windows scripts (`.bat`, `.cmd`, `.ps1`) are the only exception and use CRLF. Binary files must be marked `binary`.

## Runtime and Language Baseline

- Target the latest development baseline.
- Current required baseline: .NET 10.0 and C# 14.
- Prefer `.slnx` files instead of `.sln` for solutions.

## Testing Policy

- Unit testing framework: xUnit.
- Integration testing framework: Reqnroll.
- E2E testing framework: Reqnroll.
- Use Shouldly for all assertions (`value.ShouldBe(expected)`, `value.ShouldBeNull()`, etc.) — never use `xUnit.Assert` directly.
- Prefer `[Theory, InlineData(...)]` over `[Fact]` for tests that instantiate objects with values; add multiple `InlineData` rows to cover representative inputs and boundaries. Use `[Fact]` only when there is genuinely no parameterizable data.

## Architectural Priorities

- Preserve core GPSS semantics where feasible.
- Keep parser, semantic model, and runtime engine separated.
- Ensure deterministic simulation results for repeatable runs.
- Design for future interoperability between text GPSS and visual models.

## Quality Expectations

- Add tests for every new runtime behavior.
- Document unsupported GPSS features explicitly.
- Prefer small, reviewable increments over large refactors.
- Every `public` and `protected` type (class, record, interface, enum, struct) and every `public` or `protected` member (method, property, constructor, field) must have XML documentation (`<summary>` at minimum). Use `<param>`, `<returns>`, `<remarks>`, and `<see cref>` where they add clarity. One-line summaries are fine; avoid padding.
