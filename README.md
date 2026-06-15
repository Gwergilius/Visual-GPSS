# Visual-GPSS

Visual-GPSS is a modern revival project for the classic GPSS (General Purpose Simulation System) language that was widely used between the 1960s and 1980s for discrete-event simulation.

The project has three major goals:

1. Collect and preserve GPSS language documentation.
2. Build a GPSS interpreter/runtime in .NET/C#.
3. Build a visual editor (Visual-GPSS) for Desktop/Web/Mobile platforms, where simulation models can be composed visually.

## Vision

The long-term vision is to make GPSS usable again for modern simulation workflows while preserving compatibility with historical GPSS concepts and syntax.

## Planned Workstreams

## 1) Documentation Archive

- Collect official and community GPSS references.
- Organize documents by GPSS dialect/version.
- Build a searchable reference index.
- Add practical examples and migration notes.

## 2) GPSS Runtime (Interpreter)

- Implement a parser and semantic model for GPSS source files.
- Implement an execution engine for core GPSS blocks and events.
- Provide deterministic simulation runs and result reporting.
- Offer a CLI-first developer workflow.

## 3) Visual-GPSS Editor

- Create a node/block-based model editor.
- Support graph composition, validation, and simulation launch.
- Target Desktop, Web, and Mobile.
- Enable round-trip conversion between visual model and GPSS text.

## Technology Direction

- Language and runtime: C# on .NET (latest stable major).
- Solution format preference: `.slnx` over `.sln`.
- Unit testing: xUnit.
- Integration and E2E testing: Reqnroll.

## Repository Status

This repository is currently in the bootstrap phase.

Initial focus:

1. Set up repository standards and project conventions.
2. Define the GPSS language scope for the first executable milestone.
3. Start implementation of parser + minimal runtime.

## Contributing

Contributions are welcome once the initial architecture and coding standards are published.

For now, issues and discussions are the best way to collaborate on:

- GPSS reference sources.
- Historic compatibility requirements.
- Runtime and editor architecture ideas.

## License

This project is licensed under the terms of the license file in this repository.