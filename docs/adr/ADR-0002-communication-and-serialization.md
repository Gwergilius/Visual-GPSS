[adr-0001]: ADR-0001-component-architecture.md
[system-text-json]: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview
[azure-service-bus]: https://learn.microsoft.com/en-us/azure/service-bus-messaging/
[rabbitmq]: https://www.rabbitmq.com/
[azure-event-hubs]: https://learn.microsoft.com/en-us/azure/event-hubs/

# ADR-0002: Component Communication and Serialization Strategy

| Field   | Value             |
|---------|-------------------|
| Date    | 2026-06-15        |
| Status  | Accepted          |
| Authors | Gergely Z. Tóth   |

---

## Context

[ADR-0001][adr-0001] establishes a loosely coupled component architecture where components communicate through contracts (`Gpss.Contracts`). It defers the question of *how* data crosses component boundaries — in-process object passing, serialized DTOs, or async message channels.

As the project targets Desktop, Web, and Mobile hosts, and may eventually extract the runtime into an out-of-process service, we need a communication strategy that:

- Works efficiently for the in-process phase-1 scenario.
- Does not require an architectural rewrite when components are moved out-of-process.
- Supports async, message-oriented channels (e.g., Azure Service Bus, RabbitMQ) for future distributed scenarios.
- Keeps the internal parser-to-runtime pipeline simple and performant.

---

## Decision

We adopt a **two-layer boundary strategy**: in-process C# object passing within the core pipeline, and serialized JSON DTOs at all host-facing and externally-visible boundaries.

### Boundary Map

| Boundary | Direction | Communication Form |
|---|---|---|
| Human → System | input | GPSS source text (`.gps` file or string) |
| Host → Runtime | request | JSON DTO (`SimulationRequest`) |
| Runtime → Host | response | JSON DTO (`SimulationResult`) |
| Parser → Model | internal | In-process C# objects (AST nodes) |
| Model → Runtime | internal | In-process C# objects (validated model) |
| Async channel | event | JSON message (Service Bus, RabbitMQ, Event Hubs) |

### Key Principles

1. **GPSS source text is the canonical model format.** The `.gps` text file is the authoritative, human-readable, and version-controllable representation of a simulation model. There is no separate JSON-AST format — if a component needs a model, it receives either the source text or a `SimulationRequest` DTO that wraps it.

2. **JSON at host boundaries, C# objects internally.** The `Parser → Model → Runtime` pipeline passes C# AST node objects in-process. Only the boundary between a host (`Cli`, `Editor`) and the `Runtime` uses serialized JSON DTOs. This keeps the internal pipeline simple and avoids round-trip fidelity issues with complex, polymorphic AST trees.

3. **DTOs are defined in `Gpss.Contracts`.** All serializable boundary types (`SimulationRequest`, `SimulationResult`, event records, etc.) live in the contracts project. No component exposes its internal domain types over a boundary.

4. **JSON is the wire format.** We use `System.Text.Json` as the serialization library. YAML is acceptable for configuration files (e.g., simulation run profiles), but JSON is the canonical inter-component format. XML is not used.

5. **Async-ready boundary design.** The host-to-runtime boundary is designed as a request/result pair from day one, even when invoked synchronously in-process. This means extracting the runtime to an async message channel (Azure Service Bus, RabbitMQ, Azure Event Hubs) requires no contract changes — only a new transport adapter.

### DTO Sketch (Phase 1)

```csharp
// Gpss.Contracts

record SimulationRequest(
    string SourceText,
    SimulationOptions Options
);

record SimulationOptions(
    int? RandomSeed,
    long? TerminationCount
);

record SimulationResult(
    bool Success,
    IReadOnlyList<SimulationEvent> Events,
    SimulationStatistics Statistics,
    IReadOnlyList<DiagnosticMessage> Diagnostics
);
```

These types are intentionally minimal. Fields are added incrementally as runtime capabilities grow.

### What Is Deliberately Excluded

- **Binary serialization** (MessagePack, Protobuf): not needed in phase 1; can be added as an optimizing transport layer later without changing the DTO contract.
- **JSON-serialized AST**: the internal AST is a C# object graph and is never serialized. The source text is the external AST representation.
- **Shared domain objects across boundaries**: no component passes its internal entity types to another component directly.

---

## Consequences

### Positive

- The internal pipeline has no serialization overhead.
- Host components can be implemented in any language that speaks JSON (e.g., a TypeScript-based web editor).
- Migrating to an out-of-process or message-driven topology requires only a transport adapter, not a contract change.
- `System.Text.Json` source generation keeps serialization AOT-friendly for MAUI/mobile targets.

### Negative / Risks

- JSON DTO versioning must be managed explicitly as the `SimulationRequest` and `SimulationResult` schemas evolve.
- Deep simulation result structures (large event logs) may produce large JSON payloads; streaming or pagination may be needed later.

### Neutral

- GPSS source text as the canonical model format means the round-trip (text → parse → simulate → result) is always explicit. There is no hidden in-memory model state shared between sessions.
