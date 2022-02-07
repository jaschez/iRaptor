/*
 * Data structure that contains data about certain
 * constrains relateed to the graph generation
 */
[System.Serializable]
public struct GraphInput
{
    public int Seed;
    public int Size;
    public int Leaves;
    public int Loops;
    public int MinimumLoopLength;
    public int MaximumLoopLength;

    public GraphInput(int Seed, int Size, int Leaves, int Loops, int MinimumLoopLength, int MaximumLoopLength)
    {
        if (Seed < 2)
        {
            this.Seed = 2;
        }
        else
        {
            this.Seed = Seed;
        }
        
        this.Size = Size;
        this.Loops = Loops;
        this.Leaves = Leaves;
        this.MinimumLoopLength = MinimumLoopLength;
        this.MaximumLoopLength = MaximumLoopLength;
    }
}
