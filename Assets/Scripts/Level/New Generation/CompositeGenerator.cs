using System.Collections;
using System.Collections.Generic;

public class CompositeGenerator
{
    public List<RootedNode> RootedNodeList { get; private set; }
    List<List<RoomNode>> roomComposites;

    List<RootedNode> unexploredComposites;
    List<RootedNode> loopStartNode;

    WorldGraphOutput graphInfo;

    List<int> loopStartIndex;

    public CompositeGenerator(WorldGraphOutput graphInfo)
    {
        this.graphInfo = graphInfo;

        RootedNodeList = new List<RootedNode>(this.graphInfo.Rooms);
        roomComposites = new List<List<RoomNode>>();
    }

    public List<List<RoomNode>> GenerateComposites()
    {
        SearchComposites();
        CreateLoopComposites();
        CreateRemainingComposites();

        return roomComposites;
    }

    void SearchComposites()
    {
        unexploredComposites = new List<RootedNode>();
        loopStartNode = new List<RootedNode>();
        loopStartIndex = new List<int>();

        foreach (List<RootedNode> loop in graphInfo.GraphInfo.Loops)
        {
            for (int i = 0; i < loop.Count; i++)
            {
                RootedNode node = loop[i];

                if (!loop.Contains(node.Parent))
                {
                    loopStartIndex.Add(i);
                    loopStartNode.Add(node);
                }

                foreach (RootedNode child in node.Childs)
                {
                    if (!loop.Contains(child))
                    {
                        unexploredComposites.Add(child);
                    }
                }
            }
        }

        unexploredComposites.Add(RootedNodeList[0]);

        //Must also verify that childs extern to loops do not belong to any other loop
        for (int i = 0; i < unexploredComposites.Count; i++)
        {
            foreach (RootedNode node in loopStartNode)
            {
                if (i < unexploredComposites.Count)
                {
                    if (unexploredComposites[i].ID == node.ID)
                    {
                        unexploredComposites.RemoveAt(i);
                    }
                }
            }
        }
    }

    void CreateLoopComposites()
    {
        for (int i = 0; i < graphInfo.GraphInfo.Loops.Count; i++)
        {
            List<RootedNode> loop = graphInfo.GraphInfo.Loops[i];
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

    //Coverts from RootedNode to RoomNode
    //This is where RoomNode can be polymorphed
    void AddToRoomComposite(RootedNode node, List<RoomNode> composite)
    {
        RoomNode newRoom = FindRoomID(composite, node.ID);
        RoomNode neighbour;

        if (newRoom == null)
        {
            newRoom = new RoomNode(node);

            //We derive the room object depending on the type of room
            switch (node.Type)
            {
                case RoomType.Reward:
                    newRoom = new RewardRoom(newRoom);
                    break;

                case RoomType.Shop:
                    newRoom = new ShopRoom(newRoom);
                    break;

                default:
                    break;
            }

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
