using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator
{
    int[] pruferCode;

    System.Random random;

    public GraphGenerator(int[] pruferCode) {
        this.pruferCode = pruferCode;
    }

    public UnrootedNode[] GenerateGraph()
    {
        //UnrootedNode[] unrootedTree = PruferGeneration(pruferCode);
        //RootedNode rootedTree = RootTree(unrootedTree);

        return PruferGeneration(pruferCode);
    }

    public int[] GenerateSequence(int size, int leaves, int seed)
    {
        random = new System.Random(seed);

        int[] seq = new int[size-2];

        List<int> seqElements = new List<int>();
        List<int> notDuplicatedFlag = new List<int>();
        List<int> uniqueSequence;

        if (leaves < 2)
        {
            leaves = 2;
        }

        for (int i = 0; i < size;i++)
        {
            seqElements.Add(i + 1);
        }

        for(int i = 0; i < leaves; i++)
        {
            int index = random.Next(0, seqElements.Count);
            
            seqElements.RemoveAt(index);
        }

        uniqueSequence = new List<int>(seqElements);

        //Duplicate elements in the sequence that has not been duplicated yet, as many times as leaves it should have
        for (int i = 0; i < leaves - 2; i++)
        {
            if (notDuplicatedFlag.Count == 0)
            {
                notDuplicatedFlag = new List<int>(uniqueSequence);
            }

            int index = random.Next(0, notDuplicatedFlag.Count);
            int duplicatedElement = notDuplicatedFlag[index];

            notDuplicatedFlag.Remove(duplicatedElement);
            seqElements.Add(duplicatedElement);
        }

        for (int i = 0; i < seq.Length; i++)
        {
            int index = random.Next(0, seqElements.Count);
            int element = seqElements[index];

            seqElements.RemoveAt(index);

            seq[i] = element;
        }

        return seq;
    }

    UnrootedNode[] PruferGeneration(int[] pruferCode)
    {
        List<UnrootedNode> nodes = new List<UnrootedNode>();

        List<int> L = new List<int>();
        List<int> S = new List<int>(pruferCode);

        int l;
        int s;

        for (int i = 0; i < pruferCode.Length + 2; i++)
        {
            L.Add(i + 1);
        }

        for (int i = 0; i < S.Count; i++)
        {
            l = pruferCode.Length + 2;
            s = S[i];

            //We take the smallest value in L that is not in S
            foreach(int auxl in L)
            {
                if (auxl < l)
                {
                    if (!S.Contains(auxl)) {
                        l = auxl;
                    }
                }
            }

            L.Remove(l);
            S.Remove(s);

            i--;

            //If the graph already contains a node with the label s or l, the nodes are linked directly,
            //if not, they are created first
            LinkNodes(nodes, l, s);

        }

        if (L.Count == 2)
        {
            LinkNodes(nodes, L[0], L[1]);
        }

        return nodes.ToArray();
    }

    RootedNode RootTree(UnrootedNode[] unrooted, int rootLabel)
    {
        RootedNode root = new RootedNode(null);



        return root;
    }

    int SearchRootElement(int[] sequence)
    {
        int root = 0;

        return root;
    }

    UnrootedNode SearchNode(List<UnrootedNode> nodes, int label)
    {
        foreach (UnrootedNode node in nodes)
        {
            if (node.Label == label)
            {
                return node;
            }
        }

        return null;
    }

    void LinkNodes(List<UnrootedNode> nodes, int l, int s)
    {
        UnrootedNode lNode = SearchNode(nodes, l);
        UnrootedNode sNode = SearchNode(nodes, s);

        if (lNode == null)
        {
            lNode = new UnrootedNode(l);
            nodes.Add(lNode);
        }

        if (sNode == null)
        {
            sNode = new UnrootedNode(s);
            nodes.Add(sNode);
        }

        lNode.AddNeighbour(sNode);
        sNode.AddNeighbour(lNode);
    }
}

public class UnrootedNode
{
    public List<UnrootedNode> Neighbours { get; private set; }
    public int Label { get; private set; }
    public UnrootedNode(int Label)
    {
        Neighbours = new List<UnrootedNode>();
        this.Label = Label;
    }

    public void AddNeighbour(UnrootedNode neighbour)
    {
        if (!Neighbours.Contains(neighbour)) {
            Neighbours.Add(neighbour);
        }
    }
}

public class RootedNode
{
    public RootedNode Parent { get; private set; }
    public List<RootedNode> Childs { get; private set; }

    public RootedNode(RootedNode Parent)
    {
        this.Parent = Parent;
        Childs = new List<RootedNode>();
    }

    public void AddChild(RootedNode child)
    {
        Childs.Add(child);
    }
}