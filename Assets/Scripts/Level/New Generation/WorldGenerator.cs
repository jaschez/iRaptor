﻿using System.Collections.Generic;
using System.Linq;

public class WorldGenerator
{
    WorldGraphGenerator graphGenerator;
    CompositeGenerator compositeGenerator;

    System.Random random;

    public List<List<RoomNode>> RoomComposites { get; private set; }
    public List<RoomNode> RoomList { get; private set; }

    List<RoomGeneration> roomGenerators;
    List<RootedNode> rootedNodeList;

    public WorldGraphOutput GraphInfo { get; private set; }

    public WorldGenerator(WorldGenerationParameters param)
    {
        random = new System.Random(param.GraphParameters.Seed);

        graphGenerator = new WorldGraphGenerator(param);
        GraphInfo = graphGenerator.GenerateWorldGraph();

        roomGenerators = new List<RoomGeneration>();
        RoomList = new List<RoomNode>();

        compositeGenerator = new CompositeGenerator(GraphInfo);

        //Process each composite; convert each RootedNode into a RoomNode.
        RoomComposites = compositeGenerator.GenerateComposites();
        rootedNodeList = compositeGenerator.RootedNodeList;
    }

    public void GenerateWorld(bool debug)
    {
        //Locate relatively each room based on room parameters, creating room generators
        LocateCompositesSequentially();

        //Generate the inners of each room
        CreateRooms();
    }

    void GenerateLoop(RoomNode roomParent, List<RoomNode> composite, List<RoomNode> existingRooms)
    {
        List<RoomNode> locatedRooms = new List<RoomNode>();

        RoomGeneration firstGenerator = null;
        RoomGeneration previousRoomGenerator = null;

        //Generate a list of possible directions
        Coord[] directions = GenerateRandomDirectionList();
        Coord lastDirection = new Coord(0, 0);

        if (roomParent != null)
        {
            previousRoomGenerator = FindGeneratorID(roomGenerators, roomParent.ID);
        }

        locatedRooms.AddRange(existingRooms);

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

            if (i < composite.Count / 2)
            {
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

                    //If it is about to go backwards, skip that direction
                    if (evaluatedDir.x == -lastDirection.x && evaluatedDir.y == -lastDirection.y)
                    {
                        continue;
                    }

                    float offset = 0.5f;

                    SpawnRoom(room, previousRoomGenerator.AssociatedRoom, evaluatedDir, 0, offset);

                    bool roomCollided = CheckRoomCollision(room, locatedRooms);
                    int spaceness = 0;

                    while (roomCollided && spaceness < 1000)
                    {
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

    void GenerateBranch(RoomNode roomParent, List<RoomNode> composite, List<RoomNode> existingRooms)
    {
        List<RoomNode> locatedRooms = new List<RoomNode>();

        RoomGeneration previousRoomGenerator = null;

        if (roomParent != null)
        {
            previousRoomGenerator = FindGeneratorID(roomGenerators, roomParent.ID);
        }

        locatedRooms.AddRange(existingRooms);

        for (int i = 0; i < composite.Count; i++)
        {
            RoomNode room = composite[i];

            //We are searching for the parent like this because there may be a split in the branch,
            //and because the room composite is ordered in depth-first we could have jumps between
            //each evaluated 'previous room'
            foreach (RoomNode neighbour in room.Neighbours)
            {
                if (locatedRooms.Contains(neighbour))
                {
                    previousRoomGenerator = FindGeneratorID(roomGenerators, neighbour.ID);
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

    void LocateCompositesSequentially()
    {
        List<RoomNode> firstComposite = new List<RoomNode>();
        List<RoomNode> existingRooms = new List<RoomNode>();
        List<RoomNode> currentComposite;

        Dictionary<List<RoomNode>, RoomNode > parentedComposites = new Dictionary<List<RoomNode>, RoomNode>();
        Stack<List<RoomNode>> compositeParentsStack = new Stack<List<RoomNode>>();

        //0. Search root composite
        for (int i = 0; i < RoomComposites.Count && firstComposite.Count == 0; i++)
        {
            List<RoomNode> composite = RoomComposites[i];

            RoomNode firstRoom = composite[0];
            RootedNode evaluatedNode = FindNodeID(rootedNodeList, firstRoom.ID);

            if (evaluatedNode.Parent == null)
            {
                firstComposite = composite;
            }
        }

        compositeParentsStack.Push(firstComposite);
        parentedComposites.Add(firstComposite, null);

        //We run through the generation stack
        while (compositeParentsStack.Count > 0)
        {
            currentComposite = compositeParentsStack.Pop();

            RoomNode roomParent;

            //1. We get the room parent out of the child composite
            roomParent = parentedComposites[currentComposite];

            //2. Generates the composite, if it is a loop we build the composite in a certain way
            if (IsLoopComposite(currentComposite))
            {
                GenerateLoop(roomParent, currentComposite, existingRooms);
            }
            else
            {
                GenerateBranch(roomParent, currentComposite, existingRooms);
            }

            //3. Search for composite parents
            foreach (RoomNode room in currentComposite)
            {
                RootedNode evaluatedNode = FindNodeID(rootedNodeList, room.ID);

                foreach (RootedNode child in evaluatedNode.Childs)
                {
                    foreach (List<RoomNode> otherComposite in RoomComposites)
                    {
                        if (otherComposite != currentComposite)
                        {
                            if (otherComposite[0].ID == child.ID)
                            {
                                compositeParentsStack.Push(otherComposite);

                                if (!parentedComposites.ContainsKey(otherComposite)) {
                                    parentedComposites.Add(otherComposite, room);
                                }
                                else
                                {
                                    parentedComposites[otherComposite] = room;
                                }
                            }
                        }
                    }
                }
            }

            existingRooms.AddRange(currentComposite);
        }
    }

    void CreateRooms()
    {
        foreach (RoomGeneration generator in roomGenerators)
        {
            generator.Generate();
        }
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

    void AddRoomEntries(RoomGeneration roomGeneratorA, RoomGeneration roomGeneratorB, Coord chosenDirection)
    {
        RoomNode roomA = roomGeneratorA.AssociatedRoom;
        RoomNode roomB = roomGeneratorB.AssociatedRoom;

        Coord roomAEntry = new Coord(0, 0);
        Coord roomBEntry = new Coord(0, 0);

        int globalEntryParameter;

        int edgeMax;
        int edgeMin;

        int edgeLimit = 3;

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

        if (min > max)
        {
            return;
        }

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
        
        foreach(RoomNode room in composite)
        {
            if (room.Neighbours.Count != 2)
            {
                return false;
            }
        }

        return true;
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

    RoomGeneration FindGeneratorID(List<RoomGeneration> list, int id)
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

        foreach (int i in GraphInfo.GraphInfo.PruferCode)
        {
            sequence += i.ToString() + ",";
        }

        result = "Seed: " + GraphInfo.GraphInfo.Seed + "\n";
        result += "Prufer Code: " + sequence + "\n";
        result += "Graph Parent ID: " + GraphInfo.GraphInfo.Nodes[0].ID + "\n";
        result += "Leaves: " + GraphInfo.GraphInfo.Leaves.Length + "\n";

        foreach (List<RootedNode> loop in GraphInfo.GraphInfo.Loops)
        {
            result += "Loop in: ";

            foreach (RootedNode node in loop)
            {
                result += node.ID + ",";
            }

            result += "\n";
        }

        foreach (RoomType roomType in GraphInfo.FilteredRooms.Keys)
        {
            result += "ID of rooms of type" + roomType.ToString() + ": ";

            foreach (RootedNode node in GraphInfo.FilteredRooms[roomType])
            {
                result += "ID: " + node.ID + ",";
            }

            result += "\n";
        }

        result += "Composites:\n";

        foreach (List<RoomNode> comp in RoomComposites)
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