namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS logical switch (Logic Switch) — can be set, reset, or inverted.
/// LOGIC block references it; GATE block tests it.
/// </summary>
public sealed class LogicSwitch
{
    public string Name  { get; }
    public bool   IsSet { get; private set; }

    public LogicSwitch(string name) => Name = name;

    public void Set()    => IsSet = true;
    public void Reset()  => IsSet = false;
    public void Invert() => IsSet = !IsSet;
}

/// <summary>
/// A GPSS Savevalue — a global numeric variable (SAVEVALUE block).
/// </summary>
public sealed class Savevalue
{
    public string Name  { get; }
    public double Value { get; set; }

    public Savevalue(string name) => Name = name;
}
