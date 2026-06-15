# GPSS Simulation Examples Index

[tutorial-ch1]: https://athena.ecs.csus.edu/~mitchell/csc148/gpssW/Tutorial%20Manual/t1.htm
[tutorial-ch2]: https://athena.ecs.csus.edu/~mitchell/csc148/gpssW/Tutorial%20Manual/t5.htm
[minuteman-tutorial]: http://www.minutemansoftware.com/tutorial/tutorial_manual.htm
[gpsspy-examples]: https://martendo.github.io/gpss.py/examples
[try-mts-barber]: https://try-mts.com/gpss-introduction-and-barber-shop-simulation/
[gpss-examples-repo]: https://github.com/mihaighidoveanu/gpss-examples
[gpssh-sim-repo]: https://github.com/ucoruh/gpssh-system-simulation/

<!-- Image references -->

Date: 2026-06-15

## Scope

This index catalogs runnable GPSS simulation examples suitable for conformance testing, feature coverage validation,
and runtime benchmarking of the Visual-GPSS interpreter.

## Examples

### 1. GPSS World Tutorial Manual — Chapter 1 (Introductory Example)

[Tutorial Chapter 1][tutorial-ch1] — also mirrored at [Minuteman Software][minuteman-tutorial]
- Author: James O. Henriksen / Minuteman Software
- Year: 2001 (4th edition)
- Access date: 2026-06-15
- Dialect relevance: GPSS World
- Reliability: High — official GPSS World tutorial
- Complexity: Simple
- Blocks exercised: GENERATE, QUEUE, SEIZE, DEPART, ADVANCE, RELEASE, TERMINATE, START
- Domain: Single-server queue (barber shop)
- Notes: The canonical introductory model. Expected output statistics are documented in the manual,
  making it ideal as the first conformance test target.

### 2. GPSS World Tutorial Manual — Chapter 2 (Application Examples)

[Tutorial Chapter 2][tutorial-ch2]
- Author: James O. Henriksen / Minuteman Software
- Year: 2001 (4th edition)
- Access date: 2026-06-15
- Dialect relevance: GPSS World
- Reliability: High — official GPSS World tutorial
- Complexity: Simple to Advanced (25 models across a wide spectrum)
- Blocks exercised: Full block set including TRANSFER, GATE, TEST, ASSIGN, SAVEVALUE, STORAGE, ENTER, LEAVE, PRIORITY, SPLIT, ASSEMBLE

| File | Domain | Complexity | Key blocks |
|------|--------|------------|------------|
| TURNSTIL.GPS | Football stadium turnstile | Simple | GENERATE, TERMINATE, timer |
| TELEPHON.GPS | Simple telephone system | Simple | GATE, TRANSFER, TERMINATE |
| PERIODIC.GPS | Periodic inventory review | Medium | SAVEVALUE, ASSIGN, TEST |
| TVREPAIR.GPS | TV repair shop | Medium | QUEUE, SEIZE, ADVANCE, PRIORITY |
| QCONTROL.GPS | Quality control | Medium | TRANSFER, TEST, ASSIGN |
| ORDERPNT.GPS | Order-point inventory | Medium | SAVEVALUE, TEST, GENERATE |
| MANUFACT.GPS | Electronics manufacturing | Advanced | STORAGE, ENTER, LEAVE, TRANSFER |
| TEXTILE.GPS | Textile factory | Advanced | Multiple facilities, TRANSFER |
| OILDEPOT.GPS | Oil depot | Advanced | STORAGE, ENTER, LEAVE |
| ASSEMBLY.GPS | Pump assembly | Advanced | SPLIT, ASSEMBLE |
| ROBOTFMS.GPS | Robot flexible manufacturing | Advanced | Complex routing, TRANSFER |
| BICYCLE.GPS | Bicycle factory | Advanced | Multi-step assembly |
| STOCKCTL.GPS | Warehouse inventory | Advanced | SAVEVALUE, multi-entity |
| LOCKSIMN.GPS | Canal lock | Advanced | Complex sequencing |
| FOUNDRY.GPS | Foundry operations | Advanced | STORAGE, SPLIT |
| TAPEPREP.GPS | NC tape preparation | Medium | SEIZE, ADVANCE, RELEASE |
| TRAFFIC.GPS | T-junction traffic flow | Medium | TRANSFER, conditional routing |
| POWDER.GPS | Brand loyalty patterns | Medium | TRANSFER, probability |
| QTHEORY.GPS | Solvable queuing network | Medium | Multi-server, TRANSFER |
| SUPERMRK.GPS | Supermarket checkout | Medium | Multiple QUEUE, SEIZE |
| SHIPPORT.GPS | Port operations | Advanced | STORAGE, ENTER, LEAVE |
| EXCHANGE.GPS | PBX telephone exchange | Advanced | GATE, TRANSFER, STORAGE |
| FMSMODEL.GPS | Flexible manufacturing system | Advanced | Full FMS model |
| ETHERNET.GPS | 10 Mbps Ethernet network | Advanced | GATE, TEST, collision logic |
| PREDATOR.GPS | Predator-prey population dynamics | Advanced | SAVEVALUE, matrix operations |

- Notes: The first five entries include full code discussion in the tutorial text.
  The remaining twenty provide code listings without step-by-step walkthrough.

### 3. gpss.py Community Examples

[gpss.py Examples][gpsspy-examples]
- Author: martendo (GitHub)
- Year: ~2021–present
- Access date: 2026-06-15
- Dialect relevance: IBM GPSS (close to original semantics)
- Reliability: Medium — community implementation, not an official source
- Complexity: Simple to Medium (4 models)
- Blocks exercised: GENERATE, QUEUE, SEIZE, DEPART, ADVANCE, RELEASE, STORAGE, ENTER, LEAVE, TRANSFER, TERMINATE

| Model | Domain | Complexity | Key blocks |
|-------|--------|------------|------------|
| Barber Shop | Single-server queue | Simple | GENERATE, QUEUE, SEIZE, DEPART, ADVANCE, RELEASE |
| Tool Crib | Two-category priority queue | Simple | as above + PRIORITY |
| Widget Assembly Line | Multi-worker shared resource | Medium | STORAGE, ENTER, LEAVE, TRANSFER (feedback) |
| Inspection Station | Dual queue with feedback | Medium | STORAGE, dual QUEUE, TRANSFER (feedback) |

- Notes: Full code and expected simulation output are available on the examples page.
  Useful as a cross-reference against GPSS World output, but dialect differences may cause minor discrepancies.

### 4. Try-MTS Annotated Barber Shop

[Try-MTS Barber Shop][try-mts-barber]
- Author: Try-MTS editorial
- Year: Unknown
- Access date: 2026-06-15
- Dialect relevance: General GPSS (close to GPSS/360 syntax)
- Reliability: Medium — educational/informal walkthrough
- Complexity: Simple
- Blocks exercised: SIMULATE, GENERATE, QUEUE, SEIZE, DEPART, ADVANCE, RELEASE, TERMINATE, START, END
- Domain: Single-server queue (barber shop)
- Notes: Includes a line-by-line explanation of each statement. Useful for parser conformance checks
  and verifying diagnostic messages.

### 5. mihaighidoveanu/gpss-examples (Educational Lab Repository)

[GitHub repository][gpss-examples-repo]
- Author: mihaighidoveanu (GitHub)
- Year: ~2018
- Access date: 2026-06-15
- Dialect relevance: GPSS World (university lab course material)
- Reliability: Low — student coursework, no independent review
- Complexity: Simple to Medium (10 lab exercises)
- Files: lab2.gps – lab10.gps, locDeJoaca.gps
- Domain: Various academic exercises (queue, inventory, factory scenarios)
- Notes: Accompanying PDF guide is in Romanian. Useful as a secondary set of parser stress-test inputs.
  Expected output must be generated with a verified runtime before using as regression baselines.

### 6. ucoruh/gpssh-system-simulation (GPSS/H Examples)

[GitHub repository][gpssh-sim-repo]
- Author: ucoruh (GitHub)
- Year: Unknown
- Access date: 2026-06-15
- Dialect relevance: GPSS/H
- Reliability: Low — community repository, limited documentation
- Complexity: Medium
- Domain: Various systems (GPSS/H dialect)
- Notes: GPSS/H syntax differs from GPSS World in several block forms and operand conventions.
  Do not use as a GPSS World conformance baseline. Isolate in a separate dialect-tagged test suite.

## Testing Priority Order

| Priority | Example | Rationale |
|----------|---------|-----------|
| 1 | Tutorial Ch. 1 — Barber Shop | Minimal block set; expected output documented in the manual |
| 2 | TURNSTIL.GPS, TELEPHON.GPS | Simplest single-mechanism models from the official set |
| 3 | gpss.py Barber Shop / Tool Crib | Cross-check against an independent GPSS implementation |
| 4 | TVREPAIR.GPS, SUPERMRK.GPS | Medium complexity; realistic multi-server queue behavior |
| 5 | QCONTROL.GPS, PERIODIC.GPS | Exercises TRANSFER, TEST, SAVEVALUE |
| 6 | MANUFACT.GPS, ASSEMBLY.GPS | Advanced block coverage (STORAGE, SPLIT, ASSEMBLE) |
| 7 | ETHERNET.GPS, PREDATOR.GPS | Edge cases: non-queuing applications; SAVEVALUE/matrix blocks |

## Usage Notes

- Treat GPSS World Tutorial examples as the primary conformance baseline: they are official, well-documented,
  and the manual provides enough context to derive expected outputs.
- Use gpss.py examples for secondary cross-implementation comparison only; they may carry subtle dialect differences.
- Student lab repositories (item 5) have no published expected outputs — generate reference outputs with a
  verified GPSS World runtime before using them as regression tests.
- GPSS/H examples (item 6) must be kept in a separate test suite tagged with the `gpss-h` dialect marker
  to avoid polluting GPSS World conformance results.
