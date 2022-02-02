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

    public List<List<RootedNode>> Loops { get; private set; }

    List<RootedNode> DeepestPossibleLeaves;
    RootedNode DeepestLeaf;

    public GraphGenerator(int Seed, int Size, int Leaves, int loops, int minLoopLength) {
        this.Seed = Seed;
        this.Size = Size;
        this.Leaves = Leaves;
        this.minLoopLength = minLoopLength;
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

        return mapGraph;
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

        DeepestPossibleLeaves = new List<RootedNode>();

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
                
                newRootNode.SetDepth(nodeParent.Depth+1);

                if (DeepestPossibleLeaves.Count > 0)
                {
                    if (newRootNode.Depth > DeepestPossibleLeaves[0].Depth)
                    {
                        DeepestPossibleLeaves = new List<RootedNode>();
                        DeepestPossibleLeaves.Add(newRootNode);
                    }else if (newRootNode.Depth == DeepestPossibleLeaves[0].Depth)
                    {
                        DeepestPossibleLeaves.Add(newRootNode);
                    }
                }
                else
                {
                    DeepestPossibleLeaves.Add(newRootNode);
                }
            }

            tree.Add(evaluated.Label, newRootNode);
        }

        return tree.Values.ToArray();
    }

    RootedNode[] GenerateLoops(RootedNode[] tree)
    {
        List<RootedNode> leaves = new List<RootedNode>();

        if (DeepestPossibleLeaves.Count > 1)
        {
            DeepestLeaf = null;
        }
        else
        {
            DeepestLeaf = DeepestPossibleLeaves[0];
        }

        //Collect all the leaves from the tree
        foreach (RootedNode node in tree)
        {
            if (node.Childs.Count == 0)
            {
                if (node != DeepestLeaf) {
                    leaves.Add(node);
                }
            }
        }

        Loops = new List<List<RootedNode>>();

        for (int loopIndex = 0; loopIndex < loopNumber && leaves.Count > 0; loopIndex++)
        {
            List<RootedNode> loop = new List<RootedNode>();
            Stack<RootedNode> stack = new Stack<RootedNode>();
            RootedNode leaf;
            RootedNode evaluatedNode;
            RootedNode splitNode = null;
            
            int loopLength = 0;
            int leafIndex;

            leafIndex = random.Next(0, leaves.Count);
            leaf = leaves[leafIndex];
            evaluatedNode = leaf;

            leaves.RemoveAt(leafIndex);

            //First part, ascend through the branch
            for (int i = 0; i < minLoopLength && splitNode == null; i++)
            {
                if (!BelongsToLoop(evaluatedNode.ID)) {
                    loop.Add(evaluatedNode);

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
            }

            if (splitNode == null)
            {
                loopIndex--;
                continue;
            }

            if (loopLength < minLoopLength || evaluatedNode.Childs.Count >= 4)
            {
                for (int i = evaluatedNode.Childs.Count - 1; i >= 0; i--)
                {
                    RootedNode child = evaluatedNode.Childs[i];
                    if (child.ID != loop[loop.Count - 2].ID)
                    {
                        if (!BelongsToLoop(child.ID) && child != DeepestLeaf) {
                            stack.Push(child);
                        }
                    }
                }

                //Second part, walk trough the childs until the desired loop path is achieved
                bool belongsToOtherLoop;

                do
                {
                    belongsToOtherLoop = false;
                    evaluatedNode = stack.Pop();

                    for (int i = evaluatedNode.Childs.Count - 1; i >= 0; i--)
                    {
                        RootedNode child = evaluatedNode.Childs[i];

                        if (!BelongsToLoop(child.ID) && child != DeepestLeaf) {
                            stack.Push(child);
                        }
                        else
                        {
                            belongsToOtherLoop = true;
                        }
                    }
                } while (((evaluatedNode.Depth - splitNode.Depth) + loopLength < minLoopLength || evaluatedNode.Childs.Count >= 4 || belongsToOtherLoop) && stack.Count > 0);

                while (evaluatedNode.ID != splitNode.ID)
                {
                    loop.Insert(loopLength, evaluatedNode);
                    evaluatedNode = evaluatedNode.Parent;
                }
            }

            if (loop.Count >= minLoopLength) {

                if (DeepestLeaf == null) {
                    foreach (RootedNode element in loop)
                    {
                        if (DeepestPossibleLeaves.Contains(element))
                        {
                            DeepestPossibleLeaves.Remove(element);
                        }
                    }

                    if (DeepestPossibleLeaves.Count == 1)
                    {
                        DeepestLeaf = DeepestPossibleLeaves[0];
                    }else if (DeepestPossibleLeaves.Count == 0)
                    {
                        loopIndex--;
                        continue;
                    }
                }

                loop[0].AddChild(loop[loop.Count - 1]);
                Loops.Add(loop);
            }
            else
            {
                loopIndex--;
                continue;
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

    bool BelongsToLoop(int ID)
    {
        bool belongsToLoop = false;

        for (int j = 0; j < Loops.Count && !belongsToLoop; j++)
        {
            if (Loops[j].FindIndex(element => element.ID == ID) >= 0)
            {
                belongsToLoop = true;
            }
        }

        return belongsToLoop;
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