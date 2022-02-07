using System.Collections.Generic;

public class WorldGenerator
{

    WorldGraphGenerator graphGenerator;

    List<List<RootedNode>> composites;
    List<RootedNode> roomList;
    List<RootedNode> unexploredComposites;

    List<RootedNode> loopStartNode;
    List<int> loopStartIndex;

    public WorldGraphOutput GraphOutput { get; private set; }

    public WorldGenerator(WorldGenerationParameters param)
    {
        graphGenerator = new WorldGraphGenerator(param);
        GraphOutput = graphGenerator.GenerateWorldGraph();

        composites = new List<List<RootedNode>>();
        roomList = new List<RootedNode>(GraphOutput.Rooms);
    }

    public List<List<RootedNode>> GenerateWorld()
    {
        GenerateComposites();

        //Process each composite; convert each RootedNode into a RoomNode

        return composites;
    }

    void GenerateComposites()
    {
        SearchComposites();
        CreateLoopComposites();
        CreateRemainingComposites();
    }

    public void SearchComposites()
    {
        unexploredComposites = new List<RootedNode>();
        loopStartNode = new List<RootedNode>();
        loopStartIndex = new List<int>();

        foreach (List<RootedNode> loop in GraphOutput.GraphInfo.Loops)
        {
            for (int i = 0; i < loop.Count; i++)
            {
                RootedNode node = loop[i];

                if (!loop.Contains(node.Parent))
                {
                    loopStartIndex.Add(i);
                    loopStartNode.Add(node);
                }

                if (node.Childs.Count > 1)
                {
                    foreach (RootedNode child in node.Childs)
                    {
                        if (!loop.Contains(child))
                        {
                            unexploredComposites.Add(child);
                        }
                    }
                }
            }
        }
    }

    void CreateLoopComposites()
    {
        for (int i = 0; i < GraphOutput.GraphInfo.Loops.Count; i++)
        {
            List<RootedNode> loop = GraphOutput.GraphInfo.Loops[i];
            List<RootedNode> composite = new List<RootedNode>();
            RootedNode currentNode;

            int startingLoopIndex = loopStartIndex[i];
            int currentIndex;

            for (int j = 0; j < loop.Count; j++)
            {
                currentIndex = (startingLoopIndex + j) % loop.Count;
                currentNode = loop[currentIndex];

                composite.Add(currentNode);
            }

            composites.Add(composite);
        }
    }

    void CreateRemainingComposites()
    {
        composites.Add(ExploreComposite(roomList[0]));

        foreach (RootedNode compStart in unexploredComposites)
        {
            composites.Add(ExploreComposite(compStart));
        }
    }

    List<RootedNode> ExploreComposite(RootedNode root)
    {
        List<RootedNode> composite = new List<RootedNode>();
        Stack<RootedNode> stack = new Stack<RootedNode>();

        stack.Push(root);

        while (stack.Count > 0)
        {
            RootedNode evaluated = stack.Pop();

            foreach (RootedNode child in evaluated.Childs)
            {
                if (!loopStartNode.Contains(child))
                {
                    stack.Push(child);
                }
            }

            composite.Add(evaluated);
        }

        return composite;
    }
}