[adr-template]: https://adr.github.io/
[c4-model]: https://c4model.com/
[net-aspire]: https://learn.microsoft.com/en-us/dotnet/aspire/

[component-map]: ../images/component-architecture.mmd.svg

# ADR-0001: Loosely Coupled Component Architecture

| Field   | Value             |
|---------|-------------------|
| Date    | 2026-06-15        |
| Status  | Accepted          |
| Authors | Gergely Z. Tóth   |

---

## Context

Visual-GPSS has three distinct workstreams: a documentation archive, a GPSS runtime (parser + execution engine), and a visual editor targeting Desktop, Web, and Mobile.

The runtime and the editor have very different lifecycle cadences, deployment targets, and technical concerns. Binding them into a single monolithic application would:

- Make it hard to test the runtime in isolation.
- Couple the editor release cycle to runtime stability.
- Prevent reuse of the runtime from CLI tools, test harnesses, or future integrations.
- Complicate porting the editor to Web/Mobile if the runtime is entangled in desktop-specific code.

A clean separation of concerns is therefore necessary from day one.

---

## Decision

We adopt a **loosely coupled component architecture** where each major subsystem is its own .NET project (assembly), communicating exclusively through well-defined contracts (C# interfaces in a shared `Contracts` project).

### Component Map

![Component Architecture][component-map]

### Projects (Planned)

| Project | Role |
|---|---|
| `Gpss.Contracts` | Shared interfaces, DTOs, and enums. No implementation. |
| `Gpss.Parser` | Lexer + parser: GPSS source text → Semantic Model (AST). |
| `Gpss.Model` | Semantic model: AST node types, validation, transformation. |
| `Gpss.Runtime` | Simulation engine: executes a validated model deterministically. |
| `Gpss.Cli` | CLI host: developer-facing entry point for parse/run/report workflows. |
| `Gpss.Editor` | Visual editor UI: block-based model composer, round-trip to text GPSS. |
| `Gpss.*.Tests` | Per-component xUnit test projects. |

### Communication Rules

1. **Contracts first.** No component may reference another component's concrete types directly — only interfaces from `Gpss.Contracts`.
2. **No circular dependencies.** Dependency direction: `Cli` / `Editor` → `Runtime` → `Model` → `Parser` → `Contracts`.
3. **DTOs over domain objects at boundaries.** Results and events crossing component boundaries are plain data (records/structs), not live simulation objects.
4. **Determinism is the runtime's responsibility.** The engine must produce identical results for the same input regardless of the host (CLI, Editor, test).

### Deployment Model

In the initial phase, all components are compiled into the same process (in-process hosting). The architecture does not mandate network separation. Future phases may extract the runtime into an out-of-process service (e.g., via .NET Aspire or gRPC) if the Editor's Web/Mobile targets require it — the contract boundary makes this migration straightforward.

---

## Consequences

### Positive

- Each component can be developed, tested, and released independently.
- The CLI host provides a fast inner loop for runtime development without the editor.
- The visual editor can be ported to new platforms without touching the runtime.
- Contract interfaces make it trivial to inject fakes/mocks in unit tests.

### Negative / Risks

- More project files and solution structure overhead than a monolith.
- Contracts project introduces a coordination point: interface changes require updating both sides.
- Early over-separation may slow down initial bootstrapping; mitigated by keeping contracts minimal at first.

### Neutral

- In-process hosting in phase 1 means no network latency; the distributed option is additive, not a rewrite.
- The [C4 model][c4-model] will be used in future ADRs to document each component in more detail.
