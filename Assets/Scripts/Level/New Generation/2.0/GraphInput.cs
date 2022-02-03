/*
 * Data structure that contains data about certain
 * constrains relateed to the graph generation
 */
[System.Serializable]
public struct GraphInput
{
    private int _leaves;

    public int Seed { get; }
    public int Size { get; }
    public int Leaves
    {
        get { return _leaves; }
        set { if (value < 2) _leaves = 2; else _leaves = value; }
    }

    public int LoopNumber { get; }
    public int MinimumLoopLength { get; }

    public GraphInput(int Seed, int Size, int Leaves, int LoopNumber, int MinimumLoopLength)
    {
        _leaves = 0;

        this.Seed = Seed;
        this.Size = Size;
        this.LoopNumber = LoopNumber;
        this.MinimumLoopLength = MinimumLoopLength;
        this.Leaves = Leaves;
    }
}
