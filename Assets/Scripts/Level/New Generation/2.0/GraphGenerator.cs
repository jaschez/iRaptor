using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator
{
    System.Random random;

    public int[] PruferCode { get; private set; }

    public int Seed { get; private set; }
    public int Size { get; private set; }
    public int Leaves { get; private set; }

    int loopNumber;
    int minLoopLength;
    int maxLoopLength;

    public List<List<int>> Loops { get; private set; }

    public GraphGenerator(int Seed, int Size, int Leaves, int loops, int minLoopLength, int maxLoopLength) {
        this.Seed = Seed;
        this.Size = Size;
        this.Leaves = Leaves;
        this.minLoopLength = minLoopLength;
        this.maxLoopLength = maxLoopLength;
        loopNumber = loops;
    }

    public RootedNode[] GenerateGraph()
    {
        RootedNode[] rootedTree, mapGraph;

        UnrootedNode[] unrootedTree;
        UnrootedNode rootElement;

        int rootLabel;
        int selectedNeighbour;

        PruferCode = GeneratePruferSequence();

        unrootedTree = PruferTreeGeneration(PruferCode);
        rootLabel = CalculateRoot(PruferCode);
        rootElement = SearchNode(new List<UnrootedNode>(unrootedTree), rootLabel);

        //We choose randomly between the root element and his childs to get more variety of possible trees
        selectedNeighbour = random.Next(-1, rootElement.Neighbours.Count);
        if (selectedNeighbour >= 0)
        {
            rootElement = rootElement.Neighbours[selectedNeighbour];
        }

        rootedTree = RootTree(unrootedTree, rootElement);
        mapGraph = GenerateLoops(rootedTree);

        return rootedTree;
    }

    public int[] GeneratePruferSequence()
    {
        random = new System.Random(Seed);

        int[] seq = new int[Size-2];

        List<int> seqElements = new List<int>();
        List<int> notDuplicatedFlag = new List<int>();
        List<int> uniqueSequence;

        if (Leaves < 2)
        {
            Leaves = 2;
        }

        for (int i = 0; i < Size;i++)
        {
            seqElements.Add(i + 1);
        }

        for(int i = 0; i < Leaves; i++)
        {
            int index = random.Next(0, seqElements.Count);
            
            seqElements.RemoveAt(index);
        }

        uniqueSequence = new List<int>(seqElements);

        //Duplicate elements in the sequence that has not been duplicated yet, as many times as leaves it should have
        for (int i = 0; i < Leaves - 2; i++)
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

    public UnrootedNode[] PruferTreeGeneration(int[] pruferCode)
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

    RootedNode[] RootTree(UnrootedNode[] unrooted, UnrootedNode rootElement)
    {
        Dictionary<int, RootedNode> tree = new Dictionary<int, RootedNode>();
        RootedNode newRootNode;
        RootedNode nodeParent;

        Stack<UnrootedNode> stack = new Stack<UnrootedNode>();
        UnrootedNode evaluated;

        stack.Push(rootElement);

        while (stack.Count > 0)
        {
            evaluated = stack.Pop();
            nodeParent = null;

            foreach (UnrootedNode neighbour in evaluated.Neighbours)
            {
                int parentID = neighbour.Label;
                if (tree.ContainsKey(parentID))
                {
                    nodeParent = tree[parentID];
                }
                else
                {
                    stack.Push(neighbour);
                }
            }

            newRootNode = new RootedNode(nodeParent, evaluated.Label);

            if (nodeParent != null)
            {
                if (nodeParent.Depth == -1)
                {
                    nodeParent.SetDepth(0);
                }

                nodeParent.AddChild(newRootNode);
            }

            newRootNode.SetDepth(nodeParent.Depth+1);

            tree.Add(evaluated.Label, newRootNode);
        }

        return tree.Values.ToArray();
    }

    RootedNode[] GenerateLoops(RootedNode[] tree)
    {
        List<RootedNode> leaves = new List<RootedNode>();

        //Collect all the leaves from the tree
        foreach (RootedNode node in tree)
        {
            if (node.Childs.Count == 0)
            {
                leaves.Add(node);
            }
        }

        for (int loopIndex = 0; loopIndex < loopNumber; loopIndex++)
        {
            RootedNode leaf = leaves[random.Next(0, leaves.Count)];
            RootedNode evaluatedNode = leaf;
            RootedNode splitNode = null;

            Stack<RootedNode> stack = new Stack<RootedNode>();

            List<int> loop = new List<int>();
            int loopLength = 0;

            bool loopEnd = false;

            for (int i = 0; i < maxLoopLength && splitNode == null; i++)
            {
                loop.Add(evaluatedNode.ID);
                

                if (evaluatedNode.Childs.Count > 1)
                {
                    splitNode = evaluatedNode;
                }
                else
                {
                    evaluatedNode = evaluatedNode.Parent;
                }

                loopLength++;
            }

            if (loopLength >= minLoopLength && evaluatedNode.Childs.Count >= 4)
            {
                loopEnd = true;
            }

            for (int i = evaluatedNode.Childs.Count - 1; i >= 0; i--)
            {
                RootedNode child = evaluatedNode.Childs[i];
                if (child.ID != loop[loop.Count - 2]) {
                    stack.Push(child);
                }
            }

            while (!loopEnd)
            {
                //TODO: Completar fase 2, recorrer hijos
            }
        }

        return tree;
    }

    int CalculateRoot(int[] sequence)
    {
        float center = sequence.Length / 2f;
        int root;

        //Detect all repeated values in the sequence.
        //Key represents the element, value stores the index of each same element.
        Dictionary<int, List<int>> elementsFrequencies = new Dictionary<int, List<int>>();

        for (int i = 0; i < sequence.Length; i++)
        {
            int element = sequence[i];

            if (!elementsFrequencies.ContainsKey(element))
            {
                elementsFrequencies.Add(element, new List<int>());
            }

            elementsFrequencies[element].Add(i);
        }

        float nearestMeanDistance = center;
        int nearestElement = -1;

        float currentMean;
        int elementCount;

        //Only store elements repeated more than once
        //Save unique elements
        foreach (int element in elementsFrequencies.Keys)
        {
            elementCount = elementsFrequencies[element].Count;
            
            if (elementCount >= 2)
            {
                currentMean = 0;

                foreach (int index in elementsFrequencies[element])
                {
                    currentMean += index + 1;
                }

                currentMean /= elementCount;

                float distance = Mathf.Abs(center - currentMean);

                if (distance < nearestMeanDistance)
                {
                    nearestMeanDistance = distance;
                    nearestElement = element;
                }
            }
        }

        root = nearestElement;

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

    public int ID { get; private set; } = -1;
    public int Depth { get; private set; } = -1;

    public RootedNode(RootedNode Parent, int ID)
    {
        this.Parent = Parent;
        this.ID = ID;

        Childs = new List<RootedNode>();
    }

    public void AddChild(RootedNode child)
    {
        if (!Childs.Contains(child))
        {
            Childs.Add(child);
        }
    }

    public void SetParent(RootedNode Parent)
    {
        this.Parent = Parent;
    }

    public void SetDepth(int Depth)
    {
        this.Depth = Depth;
    }
}