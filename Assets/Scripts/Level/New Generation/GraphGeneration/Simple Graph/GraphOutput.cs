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

    public List<List<RootedNode>> Loops { get; }
    
    public RootedNode[] Nodes { get; }
    public RootedNode[] Leaves { get; }
    public RootedNode DeepestLeaf { get; }

    public GraphOutput(
        int Seed,
        int Size,
        int[] PruferCode,
        RootedNode DeepestLeaf,
        RootedNode[] Nodes,
        RootedNode[] Leaves,
        List<List<RootedNode>> Loops)
    {
        this.Seed = Seed;
        this.Size = Size;
        this.Leaves = Leaves;
        this.PruferCode = PruferCode;
        this.Nodes = Nodes;
        this.Loops = Loops;
        this.DeepestLeaf = DeepestLeaf;
    }
}
