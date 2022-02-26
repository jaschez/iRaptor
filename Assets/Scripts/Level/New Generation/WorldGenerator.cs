using System.Collections.Generic;
using System.Linq;

public class WorldGenerator
{
    WorldGraphGenerator graphGenerator;

    System.Random random;

    List<RoomGeneration> roomGenerators;

    public List<List<RoomNode>> roomComposites;

    List<RootedNode> nodeList;
    List<RootedNode> unexploredComposites;

    List<RootedNode> loopStartNode;
    
    List<int> loopStartIndex;

    public List<RoomNode> RoomList { get; private set; }

    public WorldGraphOutput GraphOutput { get; private set; }

    public WorldGenerator(WorldGenerationParameters param)
    {
        graphGenerator = new WorldGraphGenerator(param);
        GraphOutput = graphGenerator.GenerateWorldGraph();

        roomGenerators = new List<RoomGeneration>();
        roomComposites = new List<List<RoomNode>>();
        RoomList = new List<RoomNode>();

        nodeList = new List<RootedNode>(GraphOutput.Rooms);

        random = new System.Random(param.GraphParameters.Seed);
    }

    public void GenerateWorld(bool debug)
    {
        //1. Process each composite; convert each RootedNode into a RoomNode.
        GenerateComposites();

        //2. Locate relatively each room based on room parameters, creating room generators
        LocateComposites();

        //3. Join all the resulting composites in the global space.
        if(!debug)
            JoinComposites();

        //4. Generate the inners of each room
        CreateRooms();
    }

    void GenerateComposites()
    {
        SearchComposites();
        CreateLoopComposites();
        CreateRemainingComposites();
    }

    void LocateComposites()
    {
        foreach (List<RoomNode> composite in roomComposites)
        {
            //Check if composite is a loop, if that is the case we build the composite in a certain way
            if (IsLoopComposite(composite))
            {
                LocateLoop(composite);
            }
            else
            {
                LocateBranch(composite);
            }
        }
    }

    void JoinComposites()
    {
        //Order joining list by doing a depth-first exploration
        List<List<RoomNode>> orderedCompostites = new List<List<RoomNode>>();
        Stack<List<RoomNode>> compositeStack = new Stack<List<RoomNode>>();

        //1. Search root composite
        for(int i = 0; i < roomComposites.Count && compositeStack.Count == 0; i++)
        {
            List<RoomNode> composite = roomComposites[i];

            RoomNode firstRoom = composite[0];
            RootedNode evaluatedNode = FindNodeID(nodeList, firstRoom.ID);

            if (evaluatedNode.Parent == null)
            {
                compositeStack.Push(composite);
            }
        }

        //2. Explore in depth-first search
        while (compositeStack.Count > 0)
        {
            List<RoomNode> composite = compositeStack.Pop();
            orderedCompostites.Add(composite);

            foreach (RoomNode node in composite)
            {
                RootedNode evaluatedNode = FindNodeID(nodeList, node.ID);

                foreach (RootedNode child in evaluatedNode.Childs)
                {
                    foreach (List<RoomNode> otherComposite in roomComposites)
                    {
                        if (otherComposite[0].ID == child.ID)
                        {
                            compositeStack.Push(otherComposite);
                        }
                    }
                }
            }

        }

        //roomComposites = roomComposites.OrderByDescending(x => x.Count).ToList();

        foreach (List<RoomNode> composite in orderedCompostites)
        {
            //Get the parent from the original Node Tree, if its actually null, original position
            //will be kept as intact
            RoomNode firstRoom = composite[0];
            RootedNode evaluatedNode = FindNodeID(nodeList, firstRoom.ID);

            if (evaluatedNode.Parent != null)
            {
                RoomGeneration parentGenerator = FindGenerationID(roomGenerators, evaluatedNode.Parent.ID);
                RoomGeneration compositeGenerator = FindGenerationID(roomGenerators, firstRoom.ID);

                PlaceComposite(compositeGenerator, parentGenerator, composite, RoomList);
            }

            RoomList.AddRange(composite);
        }
    }

    void CreateRooms()
    {
        foreach (RoomGeneration generator in roomGenerators)
        {
            generator.Generate();
        }
    }

    void LocateBranch(List<RoomNode> composite)
    {
        List<RoomNode> locatedRooms = new List<RoomNode>();

        RoomGeneration previousRoomGenerator = null;

        for (int i = 0; i < composite.Count; i++)
        {
            RoomNode room = composite[i];

            foreach (RoomNode neighbour in room.Neighbours)
            {
                if (locatedRooms.Contains(neighbour))
                {
                    previousRoomGenerator = FindGenerationID(roomGenerators, neighbour.ID);
                }
            }

            //Generate a list of possible directions
            Coord[] directions = GenerateRandomDirectionList();

            //Create the room generator
            //Assign dimensions to the room according to its type
            RoomGeneration roomGenerator = CreateRoomGenerator(room);

            //Place the room given the order of possible directions
            PlaceRoom(roomGenerator, previousRoomGenerator, directions, locatedRooms);

            roomGenerators.Add(roomGenerator);
            locatedRooms.Add(room);
        }
    }

    void LocateLoop(List<RoomNode> composite)
    {
        List<RoomNode> locatedRooms = new List<RoomNode>();

        RoomGeneration firstGenerator = null;
        RoomGeneration previousRoomGenerator = null;

        //Generate a list of possible directions
        Coord[] directions = GenerateRandomDirectionList();
        Coord lastDirection = new Coord(0, 0);

        for (int i = 0; i < composite.Count; i++)
        {
            RoomNode room = composite[i];

            //Create the room generator
            //Assign dimensions to the room according to its type
            RoomGeneration roomGenerator = CreateRoomGenerator(room);

            if (i == 0)
            {
                firstGenerator = roomGenerator;
            }

            if (i < composite.Count / 2) {
                //Place the room given the order of possible directions
                lastDirection = PlaceRoom(roomGenerator, previousRoomGenerator, directions, locatedRooms);
            }
            else
            {
                //Evaluate which direction is nearer the start of the loop
                RoomNode startRoom = composite[0];

                List<RoomNode> evaluatedRooms = new List<RoomNode>();
                
                int nearestIndex = -1;
                int selectedSpaceness = 0;
                float nearestDistance = int.MaxValue;
                float selectedRandomOffset = -1;

                for (int j = 0; j < directions.Length; j++)
                {
                    Coord evaluatedDir = directions[j];

                    if (evaluatedDir.x == -lastDirection.x && evaluatedDir.y == -lastDirection.y)
                    {
                        continue;
                    }

                    float offset = 0.5f;

                    SpawnRoom(room, previousRoomGenerator.AssociatedRoom, evaluatedDir, 0, offset);

                    bool roomCollided = CheckRoomCollision(room, locatedRooms);
                    int spaceness = 0;

                    while (roomCollided && spaceness < 1000) {
                        for (float range = 0; range < 1 && roomCollided; range += .02f)
                        {
                            offset = range <= .5f ? range + .5f : 1f - range;

                            SpawnRoom(room, previousRoomGenerator.AssociatedRoom, evaluatedDir, spaceness, offset);

                            //Check if it collides with any other room
                            roomCollided = CheckRoomCollision(room, locatedRooms);
                        }

                        spaceness++;
                    }

                    float distance = RoomDistance(room, startRoom);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestIndex = j;

                        selectedRandomOffset = offset;
                        selectedSpaceness = spaceness;
                    }

                    evaluatedRooms.Add(room);
                }

                SpawnRoom(room, previousRoomGenerator.AssociatedRoom, directions[nearestIndex], selectedSpaceness, selectedRandomOffset);

                AddRoomEntries(roomGenerator, previousRoomGenerator, directions[nearestIndex]);

                lastDirection = directions[nearestIndex];
            }

            roomGenerators.Add(roomGenerator);
            locatedRooms.Add(room);

            previousRoomGenerator = roomGenerator;
        }

        //Finally, the start and the end of the loop are joined
        //We should add entries in a middle corridor if the rooms are separated
        //AddRoomEntries(firstGenerator, previousRoomGenerator, chosenDirection);
    }

    Coord[] GenerateRandomDirectionList()
    {
        List<Coord> directions = new List<Coord>() { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1) };
        
        Coord[] unordered = new Coord[4];

        for (int i = 0; i < unordered.Length;i++)
        {
            Coord chosenDirection;
            int randomIndex;

            randomIndex = random.Next(0, directions.Count);
            chosenDirection = directions[randomIndex];

            directions.RemoveAt(randomIndex);

            unordered[i] = chosenDirection;
        }

        return unordered;
    }

    Coord PlaceRoom(RoomGeneration roomGenerator, RoomGeneration previousRoomGenerator, Coord[] directions, List<RoomNode> locatedRooms)
    {
        RoomNode room = roomGenerator.AssociatedRoom;
        RoomNode previousRoom;

        Coord chosenDirection = new Coord(0,0);

        bool roomCollided = true;

        int spaceness = 0;

        if (previousRoomGenerator != null) 
        {
            previousRoom = previousRoomGenerator.AssociatedRoom;

            while (roomCollided)
            {
                //Iterate through each possible direction
                for (int i = 0; i < directions.Length && roomCollided; i++) {
                    chosenDirection = directions[i];

                    SpawnRoom(room, previousRoom, chosenDirection, spaceness,.5f);

                    //Check if it collides with any other room
                    roomCollided = CheckRoomCollision(room, locatedRooms);

                    //Explore whole range of offset before trying another direction
                    if (roomCollided)
                    {
                        for (float range = 0; range < 1 && roomCollided; range += .02f)
                        {
                            float offset = range <= .5f ? range + .5f : 1f - range;

                            SpawnRoom(room, previousRoom, chosenDirection, spaceness, offset);

                            //Check if it collides with any other room
                            roomCollided = CheckRoomCollision(room, locatedRooms);
                        }
                    }
                }

                spaceness++;
            }

            //Add entries in room local positions, betwen previous and current room
            //May depend, if spaceness > 0, we should add entries in a middle corridor
            AddRoomEntries(roomGenerator, previousRoomGenerator, chosenDirection);
        }

        return chosenDirection;
    }

    void PlaceComposite(RoomGeneration firstRoom, RoomGeneration roomParent, List<RoomNode> composite, List<RoomNode> locatedRooms)
    {
        Coord[] directions = GenerateRandomDirectionList();

        RoomNode room = firstRoom.AssociatedRoom;
        RoomNode previousRoom = roomParent.AssociatedRoom;

        Coord chosenDirection = new Coord(0, 0);
        Coord deltaPos;

        bool compositeCollided = true;
        int spaceness = 0;

        while (compositeCollided && spaceness < 200)
        {
            //Iterate through each possible direction
            for (int i = 0; i < directions.Length && compositeCollided; i++)
            {
                chosenDirection = directions[i];
                deltaPos = room.Position;

                SpawnRoom(room, previousRoom, chosenDirection, spaceness, .5f);

                deltaPos.x = room.Position.x - deltaPos.x;
                deltaPos.y = room.Position.y - deltaPos.y;

                //Move the rest of the composite with it, using the position of the first room as a delta
                foreach (RoomNode compRoom in composite)
                {
                    if (compRoom.ID != room.ID)
                    {
                        compRoom.SetWorldPosition(new Coord(compRoom.Position.x + deltaPos.x, compRoom.Position.y + deltaPos.y));
                    }
                }

                //Check if it collides with any other room
                compositeCollided = CheckCompositeCollision(composite, locatedRooms);

                //Explore whole range of offset before trying another direction
                if (compositeCollided)
                {
                    for (float range = 0; range < 1 && compositeCollided; range += .02f)
                    {
                        float offset = range <= .5f ? range + .5f : 1f - range;
                        deltaPos = room.Position;

                        SpawnRoom(room, previousRoom, chosenDirection, spaceness, offset);

                        deltaPos.x = room.Position.x - deltaPos.x;
                        deltaPos.y = room.Position.y - deltaPos.y;

                        //Move the rest of the composite with it, using the movement of the first room
                        foreach (RoomNode compRoom in composite)
                        {
                            if (compRoom.ID != room.ID)
                            {
                                compRoom.SetWorldPosition(new Coord(compRoom.Position.x + deltaPos.x, compRoom.Position.y + deltaPos.y));
                            }
                        }

                        //Check if it collides with any other room
                        compositeCollided = CheckCompositeCollision(composite, locatedRooms);
                    }
                }
            }

            spaceness++;
        }

        //Add entries in room local positions, betwen previous and current room
        //May depend, if spaceness > 0, we should add entries in a middle corridor
        AddRoomEntries(firstRoom, roomParent, chosenDirection);
    }

    void SpawnRoom(RoomNode room, RoomNode previousRoom, Coord chosenDirection, int spaceness, float perpendicularOffsetFactor = -1)
    {
        Coord position = new Coord(0, 0);

        //Calculate a random offset perpendicular to the given direction
        int perpendicularOffset;

        int minOffset;
        int maxOffset;

        int offsetLimit = 6;

        if (chosenDirection.x == 0)
        {
            //Vertical direction
            minOffset = previousRoom.Left - room.Width + offsetLimit;
            maxOffset = previousRoom.Right - offsetLimit;
        }
        else
        {
            //Horizontal direction
            minOffset = previousRoom.Bottom + offsetLimit;
            maxOffset = previousRoom.Top + room.Height - offsetLimit;
        }

        if (perpendicularOffsetFactor == -1) {
            perpendicularOffset = random.Next(minOffset, maxOffset) - minOffset;
        }
        else
        {
            perpendicularOffset = (int)((maxOffset - minOffset) * perpendicularOffsetFactor);
        }

        perpendicularOffset -= (maxOffset - minOffset) / 2;

        //Place the room right next to the previous one, or with a certain spaceness and offset
        if (chosenDirection.x == 0)
        {
            position.x = (previousRoom.Position.x + previousRoom.Width / 2) - room.Width / 2 + perpendicularOffset;
            position.y = previousRoom.Position.y + (chosenDirection.y > 0 ? room.Height : -previousRoom.Height) + spaceness * chosenDirection.y;
        }
        else
        {
            position.x = previousRoom.Position.x + (chosenDirection.x > 0 ? previousRoom.Width : -room.Width) + spaceness * chosenDirection.x;
            position.y = (previousRoom.Position.y - previousRoom.Height / 2) + room.Height / 2 + perpendicularOffset;
        }

        room.SetWorldPosition(position);
    }

    RoomGeneration CreateRoomGenerator(RoomNode room)
    {
        RoomGeneration generator;

        int generationSeed = random.Next();

        switch (room.Type)
        {
            case RoomType.Normal:
                generator = new NormalGeneration(room, generationSeed);
                break;

            case RoomType.Entrance:
                generator = new EntranceGeneration(room, generationSeed);
                break;

            default:
                generator = new NormalGeneration(room, generationSeed);
                break;
        }

        return generator;
    }

    void SearchComposites()
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

        //Must also verify that childs extern to loops do not belong to any other loop
        for (int i = 0; i < unexploredComposites.Count; i++)
        {
            foreach (RootedNode node in loopStartNode)
            {
                if (unexploredComposites[i].ID == node.ID)
                {
                    unexploredComposites.RemoveAt(i);
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

    void AddRoomEntries(RoomGeneration roomGeneratorA, RoomGeneration roomGeneratorB, Coord chosenDirection)
    {
        RoomNode roomA = roomGeneratorA.AssociatedRoom;
        RoomNode roomB = roomGeneratorB.AssociatedRoom;

        Coord roomAEntry = new Coord(0, 0);
        Coord roomBEntry = new Coord(0, 0);

        int globalEntryParameter;

        int edgeMax;
        int edgeMin;

        int edgeLimit = 4;

        if (chosenDirection.x == 0)
        {
            edgeMin = System.Math.Max(roomA.Left, roomB.Left);
            edgeMax = System.Math.Min(roomA.Right, roomB.Right);
        }
        else
        {
            edgeMin = System.Math.Max(roomA.Bottom, roomB.Bottom);
            edgeMax = System.Math.Min(roomA.Top, roomB.Top);
        }

        int min = edgeMin + edgeLimit;
        int max = edgeMax - edgeLimit;

        globalEntryParameter = random.Next(min, max);

        if (chosenDirection.x == 0)
        {
            roomAEntry.x = globalEntryParameter - roomA.Left;
            roomAEntry.y = roomA.Top - (chosenDirection.y > 0 ? roomA.Bottom + 1 : roomA.Top);

            roomBEntry.x = globalEntryParameter - roomB.Left;
            roomBEntry.y = roomB.Top - (chosenDirection.y > 0 ? roomB.Top : roomB.Bottom + 1);
        }
        else
        {
            roomAEntry.x = (chosenDirection.x > 0 ? roomA.Left : roomA.Right - 1) - roomA.Left;
            roomAEntry.y = roomA.Top - globalEntryParameter;

            roomBEntry.x = (chosenDirection.x > 0 ? roomB.Right - 1: roomB.Left) - roomB.Left;
            roomBEntry.y = roomB.Top - globalEntryParameter;
        }

        roomGeneratorA.AddEntry(roomAEntry);
        roomGeneratorB.AddEntry(roomBEntry);
    }

    float RoomDistance(RoomNode roomA, RoomNode roomB)
    {
        float distance;

        int dX = roomA.Position.x - roomB.Position.x;
        int dY = roomA.Position.y - roomB.Position.y;

        distance = (float)System.Math.Sqrt(dX*dX + dY*dY);

        return distance;
    }

    bool IsLoopComposite(List<RoomNode> composite)
    {
        if (composite[composite.Count - 1].Neighbours.Count > 0)
        {
            foreach (RoomNode child in composite[composite.Count - 1].Neighbours)
            {
                if (child.ID == composite[0].ID)
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool CheckRoomCollision(RoomNode room, List<RoomNode> collisions)
    {
        foreach (RoomNode collision in collisions)
        {
            if (room.OverlapsRoom(collision))
            {
                return true;
            }
        }

        return false;
    }

    bool CheckCompositeCollision(List<RoomNode> composite, List<RoomNode> collisions)
    {
        foreach (RoomNode collision in collisions)
        {
            foreach (RoomNode room in composite)
            {
                if (room.OverlapsRoom(collision))
                {
                    return true;
                }
            }
        }

        return false;
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

    RootedNode FindNodeID(List<RootedNode> list, int id)
    {
        foreach (RootedNode room in list)
        {
            if (room.ID == id)
            {
                return room;
            }
        }

        return null;
    }

    RoomGeneration FindGenerationID(List<RoomGeneration> list, int id)
    {
        foreach (RoomGeneration room in list)
        {
            if (room.AssociatedRoom.ID == id)
            {
                return room;
            }
        }

        return null;
    }

    public override string ToString()
    {

        string result;
        string sequence = "";

        foreach (int i in GraphOutput.GraphInfo.PruferCode)
        {
            sequence += i.ToString() + ",";
        }

        result = "Seed: " + GraphOutput.GraphInfo.Seed + "\n";
        result += "Prufer Code: " + sequence + "\n";
        result += "Graph Parent ID: " + GraphOutput.GraphInfo.Nodes[0].ID + "\n";
        result += "Leaves: " + GraphOutput.GraphInfo.Leaves.Length + "\n";

        foreach (List<RootedNode> loop in GraphOutput.GraphInfo.Loops)
        {
            result += "Loop in: ";

            foreach (RootedNode node in loop)
            {
                result += node.ID + ",";
            }

            result += "\n";
        }

        foreach (RoomType roomType in GraphOutput.FilteredRooms.Keys)
        {
            result += "ID of rooms of type" + roomType.ToString() + ": ";

            foreach (RootedNode node in GraphOutput.FilteredRooms[roomType])
            {
                result += "ID: " + node.ID + ",";
            }

            result += "\n";
        }

        result += "Composites:\n";

        foreach (List<RoomNode> comp in roomComposites)
        {
            result += "\t";

            foreach (RoomNode node in comp)
            {
                result += node.ID.ToString() + ",";
            }

            result += "\n";
        }

        return result;
    }
}