using System.Collections.Generic;

public class WorldGenerator
{

    WorldGraphGenerator graphGenerator;

    List<List<RoomNode>> roomComposites;

    List<RootedNode> nodeList;
    List<RootedNode> unexploredComposites;

    List<RootedNode> loopStartNode;
    List<int> loopStartIndex;

    int generationSeed;

    public WorldGraphOutput GraphOutput { get; private set; }

    public WorldGenerator(WorldGenerationParameters param)
    {
        graphGenerator = new WorldGraphGenerator(param);
        GraphOutput = graphGenerator.GenerateWorldGraph();

        roomComposites = new List<List<RoomNode>>();

        nodeList = new List<RootedNode>(GraphOutput.Rooms);

        generationSeed = param.GraphParameters.Seed;
    }

    public List<List<RoomNode>> GenerateWorld()
    {
        //1. Process each composite; convert each RootedNode into a RoomNode.
        GenerateComposites();

        //2. Generate each room based on room parameters.
        //Create Room Generators
        GenerateRooms();

        //3. Locate phisycally each composite on relative spaces, then join them.

        return roomComposites;
    }

    void GenerateComposites()
    {
        SearchComposites();
        CreateLoopComposites();
        CreateRemainingComposites();
    }

    void GenerateRooms()
    {
        foreach (List<RoomNode> composite in roomComposites)
        {
            //Generar direccion del composite
            //Debe comprobarse si es un bucle
            foreach (RoomNode node in composite)
            {
                RoomGeneration generator;

                Coord nextPosition = new Coord(0, 0);

                switch (node.Type)
                {
                    case RoomType.Normal:
                        generator = new NormalGeneration(nextPosition, generationSeed);
                        break;
                }
            }
        }
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
            List<RoomNode> composite = new List<RoomNode>();
            RootedNode currentNode;

            int startingLoopIndex = loopStartIndex[i];
            int currentIndex;

            for (int j = 0; j < loop.Count; j++)
            {
                currentIndex = (startingLoopIndex + j) % loop.Count;
                currentNode = loop[currentIndex];

                AddToRoomComposite(currentNode, composite);

                //Ensure that every node in the loop is connected
                if (j > 0 && !composite[j].Neighbours.Contains(composite[j - 1]))
                {
                    composite[j].AddNeighbour(composite[j - 1]);
                    composite[j - 1].AddNeighbour(composite[j]);
                }
            }

            roomComposites.Add(composite);
        }
    }

    void CreateRemainingComposites()
    {
        roomComposites.Add(ExploreComposite(nodeList[0]));

        foreach (RootedNode compStart in unexploredComposites)
        {
            roomComposites.Add(ExploreComposite(compStart));
        }
    }

    List<RoomNode> ExploreComposite(RootedNode root)
    {
        List<RoomNode> composite = new List<RoomNode>();
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

            AddToRoomComposite(evaluated, composite);
        }

        return composite;
    }

    void AddToRoomComposite(RootedNode node, List<RoomNode> composite)
    {
        RoomNode newRoom = FindRoomID(composite, node.ID);
        RoomNode neighbour;

        if (newRoom == null)
        {
            newRoom = new RoomNode(node);
            composite.Add(newRoom);
        }

        if (node.Parent != null)
        {
            neighbour = FindRoomID(composite, node.Parent.ID);

            if (neighbour != null)
            {
                newRoom.AddNeighbour(neighbour);
                neighbour.AddNeighbour(newRoom);
            }
        }
    }

    RoomNode FindRoomID(List<RoomNode> list, int id)
    {
        foreach (RoomNode room in list)
        {
            if (room.ID == id)
            {
                return room;
            }
        }

        return null;
    }
}