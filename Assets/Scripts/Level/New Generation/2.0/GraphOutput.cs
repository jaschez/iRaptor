using System.Collections.Generic;

/*
 * Data structure that contains data about a certain
 * generated graph
 */
public struct GraphOutput
{
    public int[] PruferCode { get; }
    public int Seed { get; }
    public int Size { get; }
    public int Leaves { get; }

    public List<List<RootedNode>> Loops { get; }

    public RootedNode[] Map { get; }
    public RootedNode DeepestLeaf { get; }

    public GraphOutput(
        int Seed,
        int Size,
        int Leaves,
        int[] PruferCode,
        RootedNode DeepestLeaf,
        RootedNode[] Map,
        List<List<RootedNode>> Loops)
    {
        this.Seed = Seed;
        this.Size = Size;
        this.Leaves = Leaves;
        this.PruferCode = PruferCode;
        this.Map = Map;
        this.Loops = Loops;
        this.DeepestLeaf = DeepestLeaf;
    }
}
