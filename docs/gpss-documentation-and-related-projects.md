# GPSS Documentation and Related Projects Survey

Date: 2026-06-15
Scope: publicly accessible GPSS language documentation (English/Hungarian if available), plus similar GPSS or visual GPSS-related projects.

## 1. GPSS Language Documentation Found

## 1.1 Primary language and reference documentation

1. GPSS World Reference Manual (comprehensive online manual)
- URL: https://athena.ecs.csus.edu/~mitchell/csc148/gpssW/Reference%20Manual/reference_manual.htm
- Type: Full reference manual (chapters on statements, entities, commands, block statements, grammar, reports, statistics).
- Notes: One of the most complete freely accessible GPSS-family references online.

2. GPSS/360 User's Manual (historical IBM manual)
- URL: https://archive.computerhistory.org/resources/access/text/2021/06/102665359-05-01-acc.pdf
- Type: Historical primary documentation.
- Notes: Important for preserving original semantics and terminology.

3. GPSS/H System Guide (Wolverine Software)
- URL: https://wolverinesoftware.com/hsysguid.PDF
- Type: Product/system guide for GPSS/H.
- Notes: Practical guide for GPSS/H usage and setup.

4. GPSS/H product and student edition information (Wolverine Software)
- URL: https://wolverinesoftware.com/GPSSHProducts.html
- Type: Product overview and constraints (Professional/Personal/Student editions).
- Notes: Useful for understanding living GPSS/H ecosystem and licensing/size limits.

## 1.2 Supporting overview and historical context

5. GPSS (Wikipedia)
- URL: https://en.wikipedia.org/wiki/GPSS
- Type: Overview article (history, concepts, examples, references, external links).
- Notes: Good starting point; should be treated as index material, not a normative specification.

6. GPSS/H bibliographic catalog (HathiTrust catalog record)
- URL: https://catalog.hathitrust.org/Record/002952023
- Type: Bibliographic source for GPSS/H references.
- Notes: Useful for locating print/manual editions.

## 1.3 Academic references relevant for language/tool reconstruction

7. JGPSS paper (Winter Simulation Conference 2009)
- URL: http://www.informs-sim.org/wsc09papers/022.pdf
- Type: Research paper on a Java GPSS framework for education.
- Notes: Helpful for architecture and pedagogical tooling insights.

8. GPSS Interactive Learning Environment paper (open source web environment)
- URL: https://sedici.unlp.edu.ar/bitstream/handle/10915/25669/GPSSInteractiveLearningEnvironment+word+format.pdf?sequence=1
- Type: Academic paper/tooling concept.
- Notes: Relevant to visual or guided model-building UX.

## 1.4 Hungarian-language coverage

- No clearly complete, high-quality GPSS language specification in Hungarian was identified in this first pass.
- Recommendation: plan for English-first source collection, then curate Hungarian terminology/glossary in this repository.

## 2. Similar GPSS / Visual-GPSS-Adjacent Projects

## 2.1 GPSS implementations and runtimes

1. gpss.py (Python GPSS implementation)
- URL (project site): https://martendo.github.io/gpss.py/
- URL (source): https://github.com/martendo/gpss.py
- Summary: Python implementation of IBM GPSS with usage/syntax/examples docs.
- Key lessons:
  - Keep syntax docs and runnable examples together.
  - Provide browser/demo entry points to reduce onboarding friction.

2. OpenGPSS (Python-based open-source reinterpretation)
- URL: https://github.com/NotSoOld/OpenGPSS
- Summary: Open source GPSS-inspired language/interpreter with manuals in multiple languages.
- Key lessons:
  - Clear migration story is needed when diverging from classic GPSS syntax.
  - Bundled manuals and bilingual docs improve adoption.

3. gpss-py (prototype GPSS interpreter/runtime skeleton)
- URL: https://github.com/pmantellini/gpss-py
- Summary: Work-in-progress Python interpreter with minimal scheduler/chains/blocks and tests.
- Key lessons:
  - Start from minimal core semantics and explicit roadmap.
  - Publish limitations clearly to align contributor expectations.

4. JGPSS (Java General Purpose Simulation System)
- URL: https://jgpss.liam.upc.edu/en
- Summary: Java framework and educational implementation aligned with GPSS ideas.
- Key lessons:
  - Educational framing helps community formation.
  - Separation of framework vs complete engine supports teaching and extensibility.

## 2.2 Tooling and ecosystem support

5. GPSS VS Code extension
- URL: https://github.com/eugkhp/gpss-vscode-extension
- Summary: Language support extension (syntax/snippets/run integration assumptions).
- Key lessons:
  - Early editor tooling (syntax highlight/snippets) can accelerate user adoption.
  - Avoid hard dependency assumptions in UX (for example local executable placement).

## 2.3 Visual-modeling-adjacent references

6. GPSS Interactive Learning Environment paper
- URL: https://sedici.unlp.edu.ar/bitstream/handle/10915/25669/GPSSInteractiveLearningEnvironment+word+format.pdf?sequence=1
- Summary: Web environment where learners assemble models through guided UI rather than writing raw GPSS first.
- Key lessons:
  - A visual-first path is viable for education and onboarding.
  - Round-trip capability (visual model <-> textual GPSS) should be designed from day one.

## 3. GPSS Simulation Examples

See [docs/sources/examples.md] for the full index with metadata.

The survey identified six sources of runnable GPSS simulation models suitable for conformance testing and feature coverage validation:

1. **GPSS World Tutorial Manual — Chapter 1**: canonical barber-shop introductory model; expected output is documented in the manual.
2. **GPSS World Tutorial Manual — Chapter 2**: 25 application examples covering the full range of GPSS blocks (inventory, manufacturing, traffic, networking, population dynamics, and more).
3. **gpss.py community examples**: 4 models (Barber Shop, Tool Crib, Widget Assembly Line, Inspection Station) with full code and expected output — useful for cross-implementation comparison.
4. **Try-MTS annotated barber shop**: line-by-line explanation of a single model; useful for parser conformance checks.
5. **mihaighidoveanu/gpss-examples** (GitHub): 10 educational lab exercises in GPSS World dialect; expected outputs must be derived from a reference runtime.
6. **ucoruh/gpssh-system-simulation** (GitHub): GPSS/H dialect examples; should be isolated in a separate dialect-tagged test suite.

### Recommended testing order

Start with the Tutorial Chapter 1 barber shop, progress through the five simple Chapter 2 models (TURNSTIL, TELEPHON, TVREPAIR, SUPERMRK, QCONTROL), then advance to manufacturing and storage-heavy models. Keep GPSS/H examples separate from GPSS World conformance runs.

## 4. Practical Recommendations for Visual-GPSS

1. Build a canonical documentation base first.
- Treat GPSS World Reference Manual + GPSS/360 manual as baseline corpus.
- Add a normalization layer: terminology map across GPSS variants (GPSS/360, GPSS World, GPSS/H).

2. Define a compatibility profile.
- Declare an explicit target dialect for v1 (for example GPSS World core blocks).
- Track unsupported blocks/features with rationale and planned milestones.

3. Keep runtime semantics deterministic and testable.
- Adopt event-chain semantics and reproducible random streams from the start.
- Build executable conformance examples from historical manuals.

4. Plan editor and runtime co-evolution.
- Start with textual model import/export and validation diagnostics.
- Add visual composition incrementally with strict semantic parity tests.

5. Prepare multilingual documentation strategy.
- Author normative docs in English.
- Add Hungarian glossary and tutorial overlays as a separate layer.

## 5. Suggested Next Documentation Steps

1. ~~Create docs index pages in this repository~~ ✓ Done — see [docs/sources/primary.md], [docs/sources/secondary.md], [docs/sources/projects.md].

2. ~~Collect runnable simulation examples for test coverage~~ ✓ Done — see [docs/sources/examples.md].

3. ~~Mirror essential references metadata (not copyrighted full text)~~ ✓ Done — see [docs/sources/primary.md], [docs/sources/secondary.md], [docs/sources/projects.md]:
- title, author, year, URL, access date, dialect relevance, reliability score.

4. ~~Build a GPSS block compatibility matrix~~ ✓ Done — see [docs/block-compatibility-matrix.md]:
- Rows: GPSS blocks/commands.
- Columns: GPSS/360, GPSS World, GPSS/H, Visual-GPSS target support.

5. Add curated starter reading path:
- Beginner path (overview + simple examples).
- Language-core path (syntax/entities/blocks).
- Runtime-engine path (scheduler/chains/statistics/experiments).
