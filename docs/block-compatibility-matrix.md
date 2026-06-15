# GPSS Block Compatibility Matrix

[reference-manual]: https://athena.ecs.csus.edu/~mitchell/csc148/gpssW/Reference%20Manual/reference_manual.htm
[gpss360-manual]: https://archive.computerhistory.org/resources/access/text/2021/06/102665359-05-01-acc.pdf
[gpssh-system-guide]: https://wolverinesoftware.com/hsysguid.PDF

<!-- Image references -->

Date: 2026-06-15

## Purpose

This matrix tracks which GPSS block statements, definition statements, and control commands are
present in each major GPSS dialect, and which are targeted for the Visual-GPSS v1 interpreter.

Use this document to:

- Scope the Visual-GPSS v1 implementation effort.
- Identify conformance test targets per dialect.
- Track unsupported features with explicit rationale.

## Legend

| Symbol | Meaning |
|:------:|---------|
| ✓ | Present and fully supported |
| ~ | Present with variant syntax or partial semantics |
| ✗ | Not present in this dialect / not in scope |
| ? | Not yet verified against primary sources |

For the **Visual-GPSS v1** column the symbols are scoped to the initial release milestone.

## Sources

- **GPSS/360** column: [GPSS/360 User's Manual][gpss360-manual] (IBM, ~1967)
- **GPSS World** column: [GPSS World Reference Manual][reference-manual] (Minuteman Software, 2001)
- **GPSS/H** column: [GPSS/H System Guide][gpssh-system-guide] (Wolverine Software)
- **Visual-GPSS v1** column: defined by this project; see `CLAUDE.md` Architectural Priorities

---

## 1. Block Statements

Block statements are executable simulation steps that transactions pass through at run time.

### 1.1 Transaction Movement

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| GENERATE | ✓ | ✓ | ✓ | ✓ | Core arrival generator; A–H operands |
| ADVANCE | ✓ | ✓ | ✓ | ✓ | Time delay; A (mean), B (spread/fn) |
| TERMINATE | ✓ | ✓ | ✓ | ✓ | Destroys transaction; drives START termination counter |
| TRANSFER | ✓ | ✓ | ✓ | ✓ | Unconditional and statistical modes universal; conditional modes (BOTH/ALL/SBR/etc.) vary by dialect |

### 1.2 Queue Statistics

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| QUEUE | ✓ | ✓ | ✓ | ✓ | Enter named queue (statistics collection only, no blocking) |
| DEPART | ✓ | ✓ | ✓ | ✓ | Leave named queue |

### 1.3 Facility (Single-Server Resource)

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| SEIZE | ✓ | ✓ | ✓ | ✓ | Capture a named facility |
| RELEASE | ✓ | ✓ | ✓ | ✓ | Free a named facility |
| PREEMPT | ✓ | ✓ | ✓ | ~ | Preempt a facility; PR and RE operand variants deferred to v2 |
| RETURN | ✓ | ✓ | ✓ | ~ | Return preempted facility; full semantics paired with PREEMPT v2 work |

### 1.4 Storage (Multi-Server Resource)

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| ENTER | ✓ | ✓ | ✓ | ✓ | Reserve capacity from a named storage |
| LEAVE | ✓ | ✓ | ✓ | ✓ | Release capacity to a named storage |

### 1.5 Logic and Conditional Flow

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| GATE | ✓ | ✓ | ✓ | ✓ | Conditional pass/branch on facility, storage, or logic-switch state |
| TEST | ✓ | ✓ | ✓ | ✓ | Compare two SNAs; optional branch on failure |
| LOGIC | ? | ✓ | ✓ | ~ | Set (S), Reset (R), or Invert (I) a logic switch; S and R modes in v1; I deferred |

### 1.6 Transaction Attributes and Global Values

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| ASSIGN | ✓ | ✓ | ✓ | ✓ | Set or modify a transaction parameter |
| SAVEVALUE | ✓ | ✓ | ✓ | ✓ | Set or modify a global savevalue cell |
| MSAVEVALUE | ? | ✓ | ✓ | ✗ | Matrix savevalue operation; requires MATRIX entity; deferred to v2 |
| PRIORITY | ✓ | ✓ | ✓ | ✓ | Change current transaction's scheduling priority |
| MARK | ✓ | ✓ | ✓ | ~ | Record current clock time in a parameter; transit-time SNA (MP) support needed |

### 1.7 Assembly and Synchronization

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| SPLIT | ✓ | ✓ | ✓ | ~ | Create copies of a transaction; basic copy in v1; assembly-set semantics (ASSEMBLE/GATHER/MATCH) deferred |
| ASSEMBLE | ✓ | ✓ | ✓ | ✗ | Merge N copies into one; deferred to v2 |
| GATHER | ✓ | ✓ | ✓ | ✗ | Gather all members of an assembly set; deferred to v2 |
| MATCH | ✓ | ✓ | ✓ | ✗ | Synchronize paired assembly-set members; deferred to v2 |

### 1.8 Statistical Recording

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| TABULATE | ✓ | ✓ | ✓ | ✓ | Record a value in a TABLE frequency distribution |
| COUNT | ? | ✓ | ✓ | ✗ | Count entities satisfying a condition; deferred to v2 |
| SELECT | ? | ✓ | ✓ | ✗ | Select an entity from a group by condition; deferred to v2 |

### 1.9 User Chains

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| LINK | ? | ✓ | ✓ | ✗ | Move transaction to a named user chain; deferred to v2 |
| UNLINK | ? | ✓ | ✓ | ✗ | Release transactions from a user chain; deferred to v2 |
| SCAN | ✗ | ✓ | ✓ | ✗ | Scan a user chain and modify attributes; GPSS World/H extension |
| ALTER | ✗ | ✓ | ✓ | ✗ | Alter attributes of transactions on a user chain; GPSS World/H extension |

### 1.10 Output

| Block | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-------|:--------:|:----------:|:------:|:--------------:|-------|
| WRITE | ✗ | ✓ | ? | ✗ | Write to an output file; GPSS World extension |
| PRINT | ✗ | ✓ | ? | ✗ | Print expression result to output; GPSS World extension |

---

## 2. Definition Statements

Definition statements declare simulation entities. They are processed before the simulation run starts
and are not executable blocks.

| Statement | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|-----------|:--------:|:----------:|:------:|:--------------:|-------|
| STORAGE | ✓ | ✓ | ✓ | ✓ | Declare storage capacity (single integer or SNA) |
| FUNCTION | ✓ | ✓ | ✓ | ✓ | Declare lookup function; C/D/E/L/M types in all dialects; X type in GPSS World |
| TABLE | ✓ | ✓ | ✓ | ✓ | Declare frequency-distribution table |
| VARIABLE | ✓ | ✓ | ✓ | ✓ | Declare integer arithmetic variable expression |
| FVARIABLE | ? | ✓ | ✓ | ~ | Floating-point variable; integer arithmetic covers most v1 use cases |
| BVARIABLE | ✗ | ✓ | ✓ | ✗ | Boolean variable; deferred to v2 |
| MATRIX | ? | ✓ | ✓ | ✗ | Matrix entity (for MSAVEVALUE); deferred with MSAVEVALUE |
| INITIAL | ✓ | ✓ | ✓ | ✓ | Initialize savevalue and matrix cells before START |
| EQU | ✓ | ✓ | ✓ | ✓ | Define symbolic label or constant |
| SIMULATE | ✓ | ✓ | ✓ | ✓ | Enable simulation execution (required before START) |
| RMULT | ✓ | ✓ | ✓ | ~ | Set random number generator seeds; exact multiplier/stream semantics may differ |

---

## 3. Control Commands

Control commands govern simulation execution and are issued to the runtime engine, not embedded in the
block model.

| Command | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|---------|:--------:|:----------:|:------:|:--------------:|-------|
| START | ✓ | ✓ | ✓ | ✓ | Start simulation; sets termination counter and NP option |
| RESET | ✓ | ✓ | ✓ | ✓ | Reset statistics without clearing model state; time continues |
| CLEAR | ✓ | ✓ | ✓ | ~ | Clear all model state; some G and C operand variants deferred |
| END | ✓ | ✓ | ✓ | ✓ | End simulation session |
| REPORT | ? | ✓ | ✓ | ~ | Generate standard output report; subset of report types in v1 |
| PUTPIC | ✗ | ✓ | ✗ | ✗ | Formatted output picture statement; GPSS World extension |
| GETLIST | ✗ | ✓ | ✗ | ✗ | Read values from an input list; GPSS World extension |
| BLET | ✗ | ✓ | ✗ | ✗ | General expression assignment at command level; GPSS World extension |

---

## 4. Standard Numerical Attributes (SNAs)

SNAs are read-only runtime values accessible as operands in block statements. A full SNA reference
sheet is planned (see Section 5 open items); the key SNA families are listed here.

| SNA Family | GPSS/360 | GPSS World | GPSS/H | Visual-GPSS v1 | Notes |
|------------|:--------:|:----------:|:------:|:--------------:|-------|
| Facility: `Fn`, `Fn.*` | ✓ | ✓ | ✓ | ✓ | Utilization, capture count, average hold time, etc. |
| Storage: `Sn`, `Sn.*` | ✓ | ✓ | ✓ | ✓ | Capacity, current contents, utilization, etc. |
| Queue: `Qn`, `Qn.*` | ✓ | ✓ | ✓ | ✓ | Average wait, maximum contents, zero-entry fraction, etc. |
| Table: `Tn.*` | ✓ | ✓ | ✓ | ✓ | Mean, standard deviation, entry count |
| Savevalue: `Xn`, `X$name` | ✓ | ✓ | ✓ | ✓ | Integer savevalue cell |
| Matrix savevalue: `MXn(r,c)` | ? | ✓ | ✓ | ✗ | Deferred with MSAVEVALUE and MATRIX |
| Transaction parameter: `Pn`, `P$name` | ✓ | ✓ | ✓ | ✓ | Current transaction's parameter values |
| Transit time: `MPn` | ✓ | ✓ | ✓ | ~ | Time since MARK; requires MARK v1 support |
| Clock: `C1`, `AC1` | ✓ | ✓ | ✓ | ✓ | Absolute clock (`C1`) and post-RESET clock (`AC1`) |
| Random number: `RNn` | ✓ | ✓ | ✓ | ✓ | Random number stream n (1–8 typical) |
| Function: `FNn`, `FN$name` | ✓ | ✓ | ✓ | ✓ | Evaluated function value |
| Variable: `Vn`, `V$name` | ✓ | ✓ | ✓ | ✓ | Evaluated integer variable |
| Float variable: `BVn` | ✗ | ✓ | ✓ | ✗ | Evaluated boolean variable; deferred |
| Logic switch: `LSn`, `LS$name` | ? | ✓ | ✓ | ~ | Boolean state of a logic switch; requires LOGIC block |
| Block entry count: `Nn` | ✓ | ✓ | ✓ | ✓ | Total transactions entered a block |
| Block current contents: `Wn` | ✓ | ✓ | ✓ | ✓ | Transactions currently in a block |
| Assembly set: `MBn` | ? | ✓ | ✓ | ✗ | Assembly-set member count; deferred with assembly blocks |
| Termination counter: `TG1` | ? | ✓ | ✓ | ~ | Remaining count until simulation end |

---

## 5. Open Items

The following items need primary-source verification before this matrix can be considered stable:

1. **GPSS/360 vs. GPSS V boundary**: Confirm which blocks (LOGIC, LINK, UNLINK, COUNT, SELECT)
   were introduced in GPSS V (an intermediate revision) rather than the original GPSS/360.
   The GPSS/360 manual is the authoritative source; cross-check against the Wikipedia GPSS article.

2. **WRITE / PRINT in GPSS/H**: Verify whether these output blocks are available in GPSS/H or
   are exclusive to GPSS World.

3. **MSAVEVALUE / MATRIX in GPSS/360**: Confirm presence or absence in the original IBM manual.

4. **SCAN / ALTER in GPSS/H**: Confirm whether GPSS/H supports these blocks or they are GPSS World-only.

5. **REPORT command in GPSS/360**: Verify availability and syntax.

6. **Full SNA reference sheet**: Expand the SNA table (Section 4) into a dedicated companion
   document once the block matrix is stable.

7. **FVARIABLE in GPSS/360**: Confirm when floating-point variable support was introduced.

---

## 6. Visual-GPSS v1 Scope Summary

| Category | In Scope for v1 | Deferred (v2+) |
|----------|----------------|----------------|
| Transaction movement | GENERATE, ADVANCE, TERMINATE, TRANSFER | — |
| Queue statistics | QUEUE, DEPART | — |
| Facility | SEIZE, RELEASE | PREEMPT/RETURN full semantics |
| Storage | ENTER, LEAVE | — |
| Logic / conditional | GATE, TEST, LOGIC (S/R modes) | LOGIC I mode |
| Attributes / values | ASSIGN, SAVEVALUE, PRIORITY | MSAVEVALUE |
| Assembly | SPLIT (basic copy) | ASSEMBLE, GATHER, MATCH |
| Statistics | TABULATE | COUNT, SELECT |
| User chains | — | LINK, UNLINK, SCAN, ALTER |
| Output blocks | — | WRITE, PRINT |
| Definitions | STORAGE, FUNCTION, TABLE, VARIABLE, INITIAL, EQU, SIMULATE, RMULT | FVARIABLE (full float), BVARIABLE, MATRIX |
| Control | START, RESET, CLEAR (basic), END | REPORT (full), PUTPIC, GETLIST, BLET |
| SNAs | Facility, Storage, Queue, Table, Savevalue, Parameter, Clock, RN, Function, Variable, N, W | Matrix savevalue, BVARIABLE, assembly-set, TG1 (partial) |
