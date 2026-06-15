using Gpss.Runtime.Parser;
using Gpss.Runtime.Lexer;

namespace Gpss.Runtime.Simulation;

/// <summary>
/// The GPSS Simulation Engine.
/// Executes a parsed GPSS program by maintaining a future-events chain
/// and processing transactions through blocks.
/// </summary>
public sealed class SimulationModel
{
    // ── Entities ─────────────────────────────────────────────────────────────
    public Dictionary<string, Facility>    Facilities    { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Storage>     Storages      { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, QueueEntity> Queues        { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, TableEntity> Tables        { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, LogicSwitch> Switches      { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Savevalue>   Savevalues    { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, double>      Variables     { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int>         Labels        { get; } = new(StringComparer.OrdinalIgnoreCase);

    // ── Clock & Statistics ────────────────────────────────────────────────────
    public double Clock         { get; private set; }
    public long   TerminateCount { get; private set; }

    private long _terminateGoal;
    private long _totalTransactions;

    private readonly GpssRng _rng;

    // ── Blocks ────────────────────────────────────────────────────────────────
    private List<Statement> _blocks = new();

    // ── Event queues ──────────────────────────────────────────────────────────
    // Future Events Chain: ordered by MoveTime, then by priority
    private readonly SortedSet<Transaction> _fec = new(TransactionComparer.Instance);
    // Current Events Chain: active at Clock
    private readonly Queue<Transaction> _cec = new();

    public SimulationModel(int seed = 0)
    {
        _rng = new GpssRng(seed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Setup
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Loads a parsed list of statements into the model.</summary>
    public void Load(IReadOnlyList<Statement> statements)
    {
        // First pass: extract definitions and labels
        int blockIndex = 0;
        foreach (var stmt in statements)
        {
            switch (stmt.OperationType)
            {
                case TokenType.Storage:
                    DefineStorage(stmt);
                    break;

                case TokenType.Table:
                case TokenType.Qtable:
                    DefineTable(stmt);
                    break;

                case TokenType.Variable:
                case TokenType.Fvariable:
                    DefineVariable(stmt);
                    break;

                case TokenType.Start:
                    _terminateGoal = (long)EvalOperand(stmt.Get(0), null);
                    break;

                default:
                    // It's a block; record label
                    if (stmt.Label != null)
                        Labels[stmt.Label] = blockIndex;
                    _blocks.Add(stmt);
                    blockIndex++;
                    break;
            }
        }

        // Schedule GENERATE blocks
        for (int i = 0; i < _blocks.Count; i++)
        {
            var b = _blocks[i];
            if (b.OperationType == TokenType.Generate)
                ScheduleGenerate(b, i);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Run
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the simulation until the TERMINATE count reaches the START goal.
    /// Returns the final report.
    /// </summary>
    public SimulationReport Run()
    {
        if (_terminateGoal <= 0)
            throw new InvalidOperationException("No START statement with a positive count was found.");

        while (TerminateCount < _terminateGoal)
        {
            // Move clock to next event
            if (_cec.Count == 0)
            {
                if (_fec.Count == 0) break;
                var next = _fec.Min!;
                Clock = next.MoveTime;
                // Move all events at this time to CEC
                while (_fec.Count > 0 && _fec.Min!.MoveTime <= Clock)
                {
                    var t = _fec.Min;
                    _fec.Remove(t);
                    _cec.Enqueue(t);
                }
            }

            var xact = _cec.Dequeue();
            ProcessTransaction(xact);
        }

        return BuildReport();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Transaction processing
    // ─────────────────────────────────────────────────────────────────────────

    private void ProcessTransaction(Transaction xact)
    {
        while (xact.CurrentBlock < _blocks.Count)
        {
            var block = _blocks[xact.CurrentBlock];
            bool advanced = ExecuteBlock(block, xact);
            if (!advanced) break; // blocked — transaction stays put
        }
    }

    /// <summary>
    /// Executes one block for <paramref name="xact"/>.
    /// Returns <c>true</c> if the transaction advanced (moved to next block or scheduled).
    /// Returns <c>false</c> if the transaction is blocked.
    /// </summary>
    private bool ExecuteBlock(Statement block, Transaction xact)
    {
        switch (block.OperationType)
        {
            case TokenType.Generate:
                return ExecGenerate_patched(block, xact);

            case TokenType.Terminate:
                return ExecTerminate(block, xact);

            case TokenType.Advance:
                return ExecAdvance(block, xact);

            case TokenType.Seize:
                return ExecSeize(block, xact);

            case TokenType.Release:
                return ExecRelease(block, xact);

            case TokenType.Enter:
                return ExecEnter(block, xact);

            case TokenType.Leave:
                return ExecLeave(block, xact);

            case TokenType.Queue:
                return ExecQueue(block, xact);

            case TokenType.Depart:
                return ExecDepart(block, xact);

            case TokenType.Transfer:
                return ExecTransfer(block, xact);

            case TokenType.Test:
                return ExecTest(block, xact);

            case TokenType.Assign:
                return ExecAssign(block, xact);

            case TokenType.Tabulate:
                return ExecTabulate(block, xact);

            case TokenType.Priority:
                return ExecPriority(block, xact);

            case TokenType.Mark:
                return ExecMark(block, xact);

            case TokenType.Logic:
                return ExecLogic(block, xact);

            case TokenType.Savevalue:
                return ExecSavevalue(block, xact);

            default:
                // Unknown/unsupported block — skip
                xact.CurrentBlock++;
                return true;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Block implementations
    // ─────────────────────────────────────────────────────────────────────────

    private bool ExecTerminate(Statement block, Transaction xact)
    {
        double count = block.Operands.Count > 0 ? EvalOperand(block.Get(0), xact) : 1;
        TerminateCount += (long)count;
        // Transaction is destroyed — do not reschedule
        xact.CurrentBlock = int.MaxValue;
        return true;
    }

    private bool ExecAdvance(Statement block, Transaction xact)
    {
        double mean    = EvalOperand(block.Get(0), xact);
        double spread  = block.Operands.Count > 1 ? EvalOperand(block.Get(1), xact) : 0;
        double delay   = _rng.UniformSpread(mean, spread);
        if (delay < 0) delay = 0;

        xact.MoveTime    = Clock + delay;
        xact.CurrentBlock++;
        ScheduleInFec(xact);
        return false; // stop processing; resume when MoveTime is reached
    }

    private bool ExecSeize(Statement block, Transaction xact)
    {
        string name     = ResolveEntityName(block.Get(0), xact);
        var facility    = GetOrCreateFacility(name);
        if (facility.TrySeize(xact, Clock))
        {
            xact.CurrentBlock++;
            return true;
        }
        return false; // blocked
    }

    private bool ExecRelease(Statement block, Transaction xact)
    {
        string name     = ResolveEntityName(block.Get(0), xact);
        var facility    = GetOrCreateFacility(name);
        var nextXact    = facility.Release(xact, Clock);
        if (nextXact != null)
        {
            nextXact.CurrentBlock++;
            _cec.Enqueue(nextXact);
        }
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecEnter(Statement block, Transaction xact)
    {
        string name  = ResolveEntityName(block.Get(0), xact);
        int units    = block.Operands.Count > 1 ? (int)EvalOperand(block.Get(1), xact) : 1;
        var storage  = GetOrCreateStorage(name);
        if (storage.TryEnter(xact, Clock, units))
        {
            xact.CurrentBlock++;
            return true;
        }
        return false; // blocked
    }

    private bool ExecLeave(Statement block, Transaction xact)
    {
        string name = ResolveEntityName(block.Get(0), xact);
        int units   = block.Operands.Count > 1 ? (int)EvalOperand(block.Get(1), xact) : 1;
        var storage = GetOrCreateStorage(name);
        foreach (var released in storage.Leave(Clock, units))
        {
            released.CurrentBlock++;
            _cec.Enqueue(released);
        }
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecQueue(Statement block, Transaction xact)
    {
        string name = ResolveEntityName(block.Get(0), xact);
        var queue   = GetOrCreateQueue(name);
        queue.Enter(xact, Clock);
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecDepart(Statement block, Transaction xact)
    {
        string name = ResolveEntityName(block.Get(0), xact);
        var queue   = GetOrCreateQueue(name);
        queue.Depart(xact, Clock);
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecTransfer(Statement block, Transaction xact)
    {
        // TRANSFER  fraction/mode, dest1, dest2
        var modeOp = block.Get(0);
        var dest1  = block.Get(1);
        var dest2  = block.Get(2);

        if (modeOp is Operand.Empty)
        {
            // Unconditional transfer to dest1
            return TransferTo(block.Get(1), xact);
        }

        string modeStr = (modeOp is Operand.SymbolRef sr) ? sr.Name.ToUpperInvariant() : "";

        switch (modeStr)
        {
            case "BOTH":
            {
                // Try dest1 first, then dest2
                if (!TryTransferTo(dest1, xact))
                    return TryTransferTo(dest2, xact);
                return true;
            }
            case "ALL":
            {
                // Sequential scan from dest1 to dest2 for an available block
                return TryTransferTo(dest1, xact) || TryTransferTo(dest2, xact);
            }
            default:
            {
                // Probabilistic transfer: fraction (0–1) → dest2, else dest1
                double prob = EvalOperand(modeOp, xact);
                string target = _rng.Uniform() < prob
                    ? ResolveEntityName(dest2, xact)
                    : ResolveEntityName(dest1, xact);
                return JumpToLabel(target, xact);
            }
        }
    }

    private bool TransferTo(Operand dest, Transaction xact) =>
        JumpToLabel(ResolveEntityName(dest, xact), xact);

    private bool TryTransferTo(Operand dest, Transaction xact)
    {
        if (dest is Operand.Empty) return false;
        return JumpToLabel(ResolveEntityName(dest, xact), xact);
    }

    private bool JumpToLabel(string label, Transaction xact)
    {
        if (string.IsNullOrEmpty(label)) return false;
        if (!Labels.TryGetValue(label, out int idx)) return false;
        xact.CurrentBlock = idx;
        return true;
    }

    private bool ExecTest(Statement block, Transaction xact)
    {
        // TEST op A,B,dest
        // op: E G GE L LE NE
        var opOp   = block.Get(0);
        var aOp    = block.Get(1);
        var bOp    = block.Get(2);
        var destOp = block.Get(3);

        string op = (opOp is Operand.SymbolRef sr) ? sr.Name.ToUpperInvariant() : "E";
        double a  = EvalOperand(aOp, xact);
        double b  = EvalOperand(bOp, xact);

        bool result = op switch
        {
            "E"  => a == b,
            "NE" => a != b,
            "G"  => a >  b,
            "GE" => a >= b,
            "L"  => a <  b,
            "LE" => a <= b,
            _    => a == b,
        };

        if (result)
        {
            xact.CurrentBlock++;
            return true;
        }

        if (destOp is not Operand.Empty)
            return JumpToLabel(ResolveEntityName(destOp, xact), xact);

        return false; // blocked if no destination
    }

    private bool ExecAssign(Statement block, Transaction xact)
    {
        // ASSIGN param,value[,function]
        int paramNum = (int)EvalOperand(block.Get(0), xact);
        double value = EvalOperand(block.Get(1), xact);
        xact.Parameters[paramNum] = value;
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecTabulate(Statement block, Transaction xact)
    {
        string tableName = ResolveEntityName(block.Get(0), xact);
        if (Tables.TryGetValue(tableName, out var table))
        {
            double val = block.Operands.Count > 1
                ? EvalOperand(block.Get(1), xact)
                : Clock - xact.MarkTime;
            table.Record(val);
        }
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecPriority(Statement block, Transaction xact)
    {
        xact.Priority = (int)EvalOperand(block.Get(0), xact);
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecMark(Statement block, Transaction xact)
    {
        xact.MarkTime = Clock;
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecLogic(Statement block, Transaction xact)
    {
        // LOGIC S|R|I switchname
        var modeOp = block.Get(0);
        var nameOp = block.Get(1);
        string mode = (modeOp is Operand.SymbolRef sr) ? sr.Name.ToUpperInvariant() : "S";
        string name = ResolveEntityName(nameOp, xact);
        var sw = GetOrCreateSwitch(name);
        switch (mode)
        {
            case "S": sw.Set();    break;
            case "R": sw.Reset();  break;
            case "I": sw.Invert(); break;
        }
        xact.CurrentBlock++;
        return true;
    }

    private bool ExecSavevalue(Statement block, Transaction xact)
    {
        string name = ResolveEntityName(block.Get(0), xact);
        double value = EvalOperand(block.Get(1), xact);
        var sv = Savevalues.GetValueOrDefault(name) ?? new Savevalue(name);
        sv.Value = value;
        Savevalues[name] = sv;
        xact.CurrentBlock++;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GENERATE scheduling
    // ─────────────────────────────────────────────────────────────────────────

    private void ScheduleGenerate(Statement block, int blockIndex)
    {
        double mean   = EvalOperand(block.Get(0), null);
        double spread = block.Operands.Count > 1 ? EvalOperand(block.Get(1), null) : 0;
        double offset = block.Operands.Count > 2 ? EvalOperand(block.Get(2), null) : _rng.UniformSpread(mean, spread);
        int priority  = block.Operands.Count > 4 ? (int)EvalOperand(block.Get(4), null) : 0;

        var xact = new Transaction
        {
            MoveTime     = Clock + offset,
            CurrentBlock = blockIndex,
            Priority     = priority,
        };
        _totalTransactions++;
        ScheduleInFec(xact);

        // Attach the generator context so it reschedules when the xact is consumed
        // We handle this by re-scheduling inside ExecGenerate (called when xact hits the block)
        // Store mean/spread in attributes for re-use
        xact.Attributes["__gen_mean"]   = mean;
        xact.Attributes["__gen_spread"] = spread;
        xact.Attributes["__gen_block"]  = blockIndex;
    }

    private void RescheduleGenerate(Transaction consumedXact)
    {
        if (!consumedXact.Attributes.TryGetValue("__gen_mean",   out double mean))   return;
        if (!consumedXact.Attributes.TryGetValue("__gen_spread", out double spread)) return;
        if (!consumedXact.Attributes.TryGetValue("__gen_block",  out double bidx))   return;

        int blockIndex = (int)bidx;

        // Create the next transaction from this generator
        double inter = _rng.UniformSpread(mean, spread);
        var next = new Transaction
        {
            MoveTime     = Clock + inter,
            CurrentBlock = blockIndex,
            Priority     = consumedXact.Priority,
        };
        next.Attributes["__gen_mean"]   = mean;
        next.Attributes["__gen_spread"] = spread;
        next.Attributes["__gen_block"]  = blockIndex;
        _totalTransactions++;
        ScheduleInFec(next);
    }

    // Override the Execute logic for GENERATE to also reschedule
    // We patch ExecGenerate here:
    private bool ExecGenerate_patched(Statement block, Transaction xact)
    {
        RescheduleGenerate(xact);
        xact.CurrentBlock++;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Definitions
    // ─────────────────────────────────────────────────────────────────────────

    private void DefineStorage(Statement stmt)
    {
        string name     = stmt.Label ?? ResolveEntityName(stmt.Get(0), null);
        int capacity    = (int)EvalOperand(
            stmt.Label != null ? stmt.Get(0) : stmt.Get(1), null);
        Storages[name]  = new Storage(name, capacity);
    }

    private void DefineTable(Statement stmt)
    {
        string name = stmt.Label ?? stmt.Operation;
        double lower  = EvalOperand(stmt.Get(1), null);
        double width  = EvalOperand(stmt.Get(2), null);
        int    count  = (int)EvalOperand(stmt.Get(3), null);
        Tables[name]  = new TableEntity(name, lower, width, count);
    }

    private void DefineVariable(Statement stmt)
    {
        string name = stmt.Label ?? stmt.Operation;
        Variables[name] = 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void ScheduleInFec(Transaction xact)
    {
        _fec.Add(xact);
    }

    private string ResolveEntityName(Operand op, Transaction? xact)
    {
        return op switch
        {
            Operand.SymbolRef sr => sr.Name,
            Operand.IntLiteral il => il.Value.ToString(),
            _ => EvalOperand(op, xact).ToString(),
        };
    }

    private double EvalOperand(Operand op, Transaction? xact)
    {
        return op switch
        {
            Operand.IntLiteral il   => il.Value,
            Operand.FloatLiteral fl => fl.Value,
            Operand.Empty           => 0,
            Operand.SymbolRef sr    => ResolveSymbol(sr.Name, xact),
            Operand.Sna sna         => EvalSna(sna, xact),
            Operand.BinaryExpr be   => EvalBinary(be, xact),
            Operand.FnRef fn        => EvalFn(fn, xact),
            _                       => 0,
        };
    }

    private double ResolveSymbol(string name, Transaction? xact)
    {
        // Parameter reference: Pn
        if (name.StartsWith("P", StringComparison.OrdinalIgnoreCase) &&
            name.Length > 1 && int.TryParse(name[1..], out int pNum))
        {
            return xact?.Parameters.GetValueOrDefault(pNum) ?? 0;
        }

        if (Variables.TryGetValue(name, out double val)) return val;
        if (Savevalues.TryGetValue(name, out var sv))    return sv.Value;
        if (Labels.TryGetValue(name, out int idx))        return idx;

        // Try parse as number
        if (double.TryParse(name, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double d)) return d;
        return 0;
    }

    private double EvalSna(Operand.Sna sna, Transaction? xact)
    {
        string entityName = ResolveEntityName(sna.Entity, xact);
        return sna.Prefix.ToUpperInvariant() switch
        {
            // Queue SNAs
            "Q"  => Queues.GetValueOrDefault(entityName)?.CurrentCount ?? 0,
            "QA" => Queues.GetValueOrDefault(entityName)?.AverageTime  ?? 0,
            "QM" => Queues.GetValueOrDefault(entityName)?.MaxCount     ?? 0,
            "QT" => Queues.GetValueOrDefault(entityName)?.TotalEntries ?? 0,
            "QZ" => Queues.GetValueOrDefault(entityName)?.ZeroEntries  ?? 0,
            // Facility SNAs
            "F"  => Facilities.GetValueOrDefault(entityName)?.IsSeized == true ? 1 : 0,
            "FR" => Facilities.GetValueOrDefault(entityName)?.Utilization(Clock) ?? 0,
            "FC" => Facilities.GetValueOrDefault(entityName)?.TotalSeizes ?? 0,
            // Storage SNAs
            "S"  => Storages.GetValueOrDefault(entityName)?.InUse    ?? 0,
            "SA" => Storages.GetValueOrDefault(entityName)?.AverageContent(Clock) ?? 0,
            "SC" => Storages.GetValueOrDefault(entityName)?.TotalEntries ?? 0,
            "SR" => Storages.GetValueOrDefault(entityName)?.Utilization(Clock) ?? 0,
            "SE" => Storages.GetValueOrDefault(entityName)?.Available ?? 0,
            // Clock
            "C1" => Clock,
            "AC1" => Clock,
            // Transaction
            "XN1" => xact?.Id ?? 0,
            "P"  => xact?.Parameters.GetValueOrDefault((int)EvalOperand(sna.Entity, xact)) ?? 0,
            // Savevalue
            "X"  => Savevalues.GetValueOrDefault(entityName)?.Value ?? 0,
            _ => 0,
        };
    }

    private double EvalBinary(Operand.BinaryExpr be, Transaction? xact)
    {
        double left  = EvalOperand(be.Left,  xact);
        double right = EvalOperand(be.Right, xact);
        return be.Op switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => right != 0 ? left / right : 0,
            _   => 0,
        };
    }

    private double EvalFn(Operand.FnRef fn, Transaction? xact)
    {
        double arg = EvalOperand(fn.Argument, xact);
        // Built-in functions
        return fn.Name.ToUpperInvariant() switch
        {
            "EXP"  => Math.Exp(arg),
            "LOG"  => arg > 0 ? Math.Log(arg) : 0,
            "SQRT" => arg >= 0 ? Math.Sqrt(arg) : 0,
            "INT"  => Math.Truncate(arg),
            "ABS"  => Math.Abs(arg),
            _      => 0,
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Entity factories
    // ─────────────────────────────────────────────────────────────────────────

    private Facility GetOrCreateFacility(string name)
    {
        if (!Facilities.TryGetValue(name, out var f))
            Facilities[name] = f = new Facility(name);
        return f;
    }

    private Storage GetOrCreateStorage(string name)
    {
        if (!Storages.TryGetValue(name, out var s))
            Storages[name] = s = new Storage(name, int.MaxValue);
        return s;
    }

    private QueueEntity GetOrCreateQueue(string name)
    {
        if (!Queues.TryGetValue(name, out var q))
            Queues[name] = q = new QueueEntity(name);
        return q;
    }

    private LogicSwitch GetOrCreateSwitch(string name)
    {
        if (!Switches.TryGetValue(name, out var sw))
            Switches[name] = sw = new LogicSwitch(name);
        return sw;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Report
    // ─────────────────────────────────────────────────────────────────────────

    private SimulationReport BuildReport() => new SimulationReport
    {
        EndTime           = Clock,
        TerminateCount    = TerminateCount,
        TotalTransactions = _totalTransactions,
        Facilities        = Facilities,
        Storages          = Storages,
        Queues            = Queues,
        Tables            = Tables,
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Comparer
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class TransactionComparer : IComparer<Transaction>
    {
        public static readonly TransactionComparer Instance = new();
        public int Compare(Transaction? x, Transaction? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return  1;
            int cmp = x.MoveTime.CompareTo(y.MoveTime);
            if (cmp != 0) return cmp;
            cmp = y.Priority.CompareTo(x.Priority); // higher priority first
            if (cmp != 0) return cmp;
            return x.Id.CompareTo(y.Id);
        }
    }
}
