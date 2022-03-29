using System;
using System.Collections.Generic;
using System.Linq;

public class WorldGenerator
{
    WorldGraphGenerator graphGenerator;
    CompositeGenerator compositeGenerator;

    System.Random random;

    public List<List<RoomNode>> RoomComposites { get; private set; }

    public List<RoomNode> CorridorList { get; private set; }

    public Dictionary<int, EntryConnection> Connections { get; private set; }

    List<RoomGeneration> roomGenerators;
    List<RootedNode> rootedNodeList;
    List<RoomLink> pendingRoomLinks;

    public WorldGraphOutput GraphInfo { get; private set; }

    WorldGenerationParameters generationParameters;

    public WorldGenerator(WorldGenerationParameters param)
    {
        random = new System.Random(param.GraphParameters.Seed);

        generationParameters = param;

        graphGenerator = new WorldGraphGenerator(param);
        GraphInfo = graphGenerator.GenerateWorldGraph();

        roomGenerators = new List<RoomGeneration>();
        CorridorList = new List<RoomNode>();
        pendingRoomLinks = new List<RoomLink>();

        Connections = new Dictionary<int, EntryConnection>();

        compositeGenerator = new CompositeGenerator(GraphInfo);

        //Process each composite; convert each RootedNode into a RoomNode.
        RoomComposites = compositeGenerator.GenerateComposites();
        rootedNodeList = compositeGenerator.RootedNodeList;
    }

    public void GenerateWorld(bool debug)
    {
        //Locate relatively each room based on room parameters, creating room generators
        LocateCompositesSequentially();

        //Link the remaining rooms, creating corridors between them if necessary
        LinkRooms();

        //Generate the inners of each room
        CreateRooms();

        RoomComposites.Add(CorridorList);
    }

    void CreateRooms()
    {
        foreach (RoomGeneration generator in roomGenerators)
        {
            generator.Generate();
        }
    }

    void LinkRooms()
    {
        foreach (RoomLink link in pendingRoomLinks)
        {
            GenerateRoomLink(link.RoomA, link.RoomB);
        }
    }

    void LocateCompositesSequentially()
    {
        List<RoomNode> firstComposite = new List<RoomNode>();
        List<RoomNode> existingRooms = new List<RoomNode>();
        List<RoomNode> currentComposite;

        Dictionary<List<RoomNode>, RoomNode> parentedComposites = new Dictionary<List<RoomNode>, RoomNode>();
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

                                if (!parentedComposites.ContainsKey(otherComposite))
                                {
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

                int spacing = GetSpacing(previousRoomGenerator.AssociatedRoom, room, directions[nearestIndex]);

                if (spacing == 0)
                {
                    AddRoomEntries(roomGenerator, previousRoomGenerator, directions[nearestIndex]);
                }
                else
                {
                    //Add to the list of pending links
                    RoomLink link = new RoomLink(previousRoomGenerator, roomGenerator);
                    pendingRoomLinks.Add(link);
                }

                lastDirection = directions[nearestIndex];
            }

            roomGenerators.Add(roomGenerator);
            locatedRooms.Add(room);

            previousRoomGenerator = roomGenerator;
        }

        //Finally, the start and the end of the loop are linked
        //We should add entries in a middle corridor if the rooms are separated
        RoomLink loopLink = new RoomLink(firstGenerator, previousRoomGenerator);
        pendingRoomLinks.Add(loopLink);
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

    void GenerateRoomLink(RoomGeneration roomGeneratorA, RoomGeneration roomGeneratorB)
    {
        RoomNode roomA = roomGeneratorA.AssociatedRoom;
        RoomNode roomB = roomGeneratorB.AssociatedRoom;

        List<List<RoomNode>> obstacles = new List<List<RoomNode>>(RoomComposites);

        CorridorRoomGenerator corridor = null;

        Coord roomAEntry = new Coord(0, 0);
        Coord roomBEntry = new Coord(0, 0);
        Coord corridorAEntry = new Coord(0, 0);
        Coord corridorBEntry = new Coord(0, 0);

        Coord direction;

        int edgeMax;
        int edgeMin;
        int edgeLimit = 3;

        obstacles.Add(CorridorList);

        //Decide for a parallel or a perpendicular link
        //For each case, we will generate an exploration (rectangular) zone

        //Beforehand, calculate the direction
        direction = GetDirectionBetween(roomA, roomB);

        //Assign the range limits to evaluate

        if (direction.x == 0)
        {
            //Vertical link
            edgeMin = System.Math.Max(roomA.Left, roomB.Left);
            edgeMax = System.Math.Min(roomA.Right, roomB.Right);
        }
        else
        {
            //Horizontal link
            edgeMin = System.Math.Max(roomA.Bottom, roomB.Bottom);
            edgeMax = System.Math.Min(roomA.Top, roomB.Top);
        }

        int min = edgeMin + edgeLimit;
        int max = edgeMax - edgeLimit;

        int corridorTop;
        int corridorBottom;
        int corridorLeft;
        int corridorRight;

        int parallelLinkParameter;

        //Check range coincidence
        if (min < max)
        {
            //Parallel Link

            //Set corridor bounds
            if (direction.x == 0)
            {
                //Vertical
                corridorLeft = edgeMin;
                corridorRight = edgeMax;
                corridorTop = direction.y > 0 ? roomB.Bottom : roomA.Bottom;
                corridorBottom = direction.y > 0 ? roomA.Top : roomB.Top;
            }
            else
            {
                //Horizontal
                corridorBottom = edgeMin;
                corridorTop = edgeMax;
                corridorLeft = direction.x > 0 ? roomA.Right : roomB.Right;
                corridorRight = direction.x > 0 ? roomB.Left : roomA.Left;
            }

            if (corridorTop - corridorBottom == 0 || corridorRight - corridorLeft == 0)
            {
                AddRoomEntries(roomGeneratorB, roomGeneratorA, direction);
                return;
            }

            corridor = new CorridorRoomGenerator(random.Next(), generationParameters.Level, corridorTop, corridorBottom, corridorLeft, corridorRight);

            //Set entry for roomA, roomB and corridor
            parallelLinkParameter = random.Next(min, max);

            if (direction.x == 0)
            {
                roomAEntry.x = parallelLinkParameter - roomA.Left;
                roomAEntry.y = roomA.Top - (direction.y < 0 ? roomA.Bottom + 1 : roomA.Top);

                roomBEntry.x = parallelLinkParameter - roomB.Left;
                roomBEntry.y = roomB.Top - (direction.y < 0 ? roomB.Top : roomB.Bottom + 1);

                corridorAEntry.x = parallelLinkParameter - corridor.AssociatedRoom.Left;
                corridorAEntry.y = corridor.AssociatedRoom.Top - (direction.y < 0 ? corridor.AssociatedRoom.Top : corridor.AssociatedRoom.Bottom + 1);

                corridorBEntry.x = parallelLinkParameter - corridor.AssociatedRoom.Left;
                corridorBEntry.y = corridor.AssociatedRoom.Top - (direction.y < 0 ? corridor.AssociatedRoom.Bottom + 1 : corridor.AssociatedRoom.Top);
            }
            else
            {
                roomAEntry.x = (direction.x < 0 ? roomA.Left : roomA.Right - 1) - roomA.Left;
                roomAEntry.y = roomA.Top - parallelLinkParameter;

                roomBEntry.x = (direction.x < 0 ? roomB.Right - 1 : roomB.Left) - roomB.Left;
                roomBEntry.y = roomB.Top - parallelLinkParameter;

                corridorAEntry.x = (direction.x < 0 ? corridor.AssociatedRoom.Right - 1 : corridor.AssociatedRoom.Left) - corridor.AssociatedRoom.Left;
                corridorAEntry.y = corridor.AssociatedRoom.Top - parallelLinkParameter;

                corridorBEntry.x = (direction.x < 0 ? corridor.AssociatedRoom.Left : corridor.AssociatedRoom.Right - 1) - corridor.AssociatedRoom.Left;
                corridorBEntry.y = corridor.AssociatedRoom.Top - parallelLinkParameter;
            }

            roomGeneratorA.AddEntry(roomAEntry);
            roomGeneratorB.AddEntry(roomBEntry);

            corridor.GenerateObstacleMap(obstacles);

            corridor.AddEntry(corridorAEntry);
            corridor.AddEntry(corridorBEntry);

            corridor.Generate();

            //Check if the corridor is valid
            if (corridor.IsValidCorridor())
            {
                CorridorList.Add(corridor.AssociatedRoom);

                AddConnection(roomA, corridor.AssociatedRoom, roomAEntry, corridorAEntry, direction);
                AddConnection(corridor.AssociatedRoom, roomB, corridorBEntry, roomBEntry, direction);
            }
            else
            {
                //Add to collided rooms
            }
        }
        else
        {
            //Perpendicular Link

            Coord originEntryA;
            Coord originEntryB;
            Coord destinationEntryA;
            Coord destinationEntryB;

            //Get direction vector
            Coord roomACenter;
            Coord roomBCenter;

            roomACenter.x = (roomA.Left + roomA.Right) / 2;
            roomACenter.y = (roomA.Top + roomA.Bottom) / 2;

            roomBCenter.x = (roomB.Left + roomB.Right) / 2;
            roomBCenter.y = (roomB.Top + roomB.Bottom) / 2;

            float distance = RoomDistance(roomA, roomB);
            float dX = (roomBCenter.x - roomACenter.x) / distance;
            float dY = (roomBCenter.y - roomACenter.y) / distance;

            //Based on direction, calculate 2 possible corridors (B is destination, A is origin)
            CorridorRoomGenerator corridorA;
            CorridorRoomGenerator corridorB;

            //Set bounds for each corridor

            corridorA = new CorridorRoomGenerator(random.Next(), generationParameters.Level,
                dY > 0? roomB.Top : roomA.Bottom,
                dY > 0? roomA.Top : roomB.Bottom,
                dX > 0? roomA.Left : roomB.Right,
                dX > 0? roomB.Left : roomA.Right);

            corridorB = new CorridorRoomGenerator(random.Next(), generationParameters.Level,
                dY > 0 ? roomB.Bottom : roomA.Top,
                dY > 0 ? roomA.Bottom : roomB.Top,
                dX > 0 ? roomA.Right : roomB.Left,
                dX > 0 ? roomB.Right : roomA.Left);

            //Create obstacle map
            corridorA.GenerateObstacleMap(obstacles);
            corridorB.GenerateObstacleMap(obstacles);

            //Assign nearest entries to each other

            if (dY > 0)
            {
                originEntryA.y = corridorA.AssociatedRoom.Height - 1;
                destinationEntryA.y = corridorA.AssociatedRoom.Top - 
                    (roomA.Top > roomB.Bottom ? roomA.Top : roomB.Bottom) - edgeLimit;

                originEntryB.y = corridorB.AssociatedRoom.Top -
                    (roomA.Top < roomB.Bottom ? roomA.Top : roomB.Bottom) + edgeLimit;
                destinationEntryB.y = 0;
            }
            else
            {
                originEntryA.y = 0;
                destinationEntryA.y = corridorA.AssociatedRoom.Top -
                    (roomA.Bottom < roomB.Top ? roomA.Bottom : roomB.Top) + edgeLimit;

                originEntryB.y = corridorB.AssociatedRoom.Top -
                    (roomA.Bottom > roomB.Top ? roomA.Bottom : roomB.Top) - edgeLimit;
                destinationEntryB.y = corridorB.AssociatedRoom.Height - 1;
            }

            if (dX > 0)
            {
                originEntryA.x = (roomA.Right < roomB.Left ? roomA.Right : roomB.Left) -
                    corridorA.AssociatedRoom.Left - edgeLimit;
                destinationEntryA.x = corridorA.AssociatedRoom.Width - 1;

                originEntryB.x = 0;
                destinationEntryB.x = (roomA.Right > roomB.Left ? roomA.Right : roomB.Left) -
                    corridorB.AssociatedRoom.Left + edgeLimit;
            }
            else
            {
                originEntryA.x = (roomA.Left > roomB.Right ? roomA.Left : roomB.Right) -
                    corridorA.AssociatedRoom.Left + edgeLimit;
                destinationEntryA.x = 0;

                originEntryB.x = corridorB.AssociatedRoom.Width - 1;
                destinationEntryB.x = (roomA.Left < roomB.Right ? roomA.Left : roomB.Right) -
                    corridorB.AssociatedRoom.Left - edgeLimit;
            }

            //Search nearest valid entry on each one
            bool entriesAValid = corridorA.AddValidCorridorEntry(originEntryA, 
                dX > 0 ? new Coord(-1, 0) : new Coord(1, 0),
                dX > 0 ? new Coord(edgeLimit, originEntryA.y) : new Coord(corridorA.Width - edgeLimit, originEntryA.y))
                && corridorA.AddValidCorridorEntry(destinationEntryA,
                dY > 0 ? new Coord(0, -1) : new Coord(0, 1),
                dY > 0 ? new Coord(destinationEntryA.x, edgeLimit) : new Coord(destinationEntryA.x, corridorA.Height - edgeLimit));

            bool entriesBValid = corridorB.AddValidCorridorEntry(originEntryB,
                dY > 0 ? new Coord(0, 1) : new Coord(0, -1),
                dY > 0 ? new Coord(originEntryB.x, corridorB.Height - edgeLimit) : new Coord(originEntryB.x, edgeLimit))
                && corridorB.AddValidCorridorEntry(destinationEntryB,
                dX > 0 ? new Coord(1, 0) : new Coord(-1, 0),
                dX > 0 ? new Coord(corridorB.Width - edgeLimit, destinationEntryB.y) : new Coord(edgeLimit, destinationEntryB.y));

            //Check if all corridor entries are valid
            //(Assign one of the corridors if only one does)

            if (entriesAValid)
            {
                corridorA.Generate();
            }
            
            if (entriesBValid)
            {
                corridorB.Generate();
            }

            if (corridorA.IsValidCorridor() && corridorB.IsValidCorridor())
            {
                corridor = random.NextDouble() % 1 > .5 ? corridorA : corridorB;
            }
            else if(corridorA.IsValidCorridor() ^ corridorB.IsValidCorridor())
            {
                if (corridorA.IsValidCorridor())
                {
                    corridor = corridorA;
                }
                else
                {
                    corridor = corridorB;
                }
            }
            else
            {
                //Add to collided rooms
            }

            if (corridor != null)
            {
                //Add corridor entries to room A and B
                //Convert local corridor coords into local room coords
                Coord origin = ConvertRoomCoordinates(corridor.StartPoints[0], corridor.AssociatedRoom, roomA);
                Coord destination = ConvertRoomCoordinates(corridor.StartPoints[1], corridor.AssociatedRoom, roomB);

                roomGeneratorA.AddEntry(origin);
                roomGeneratorB.AddEntry(destination);

                AddConnection(roomA, corridor.AssociatedRoom, origin, corridor.StartPoints[0], dX > 0? new Coord(1, 0) : new Coord(-1, 0));
                AddConnection(corridor.AssociatedRoom, roomB, corridor.StartPoints[1], destination, dY > 0? new Coord(0, 1) : new Coord(0, -1));

                CorridorList.Add(corridor.AssociatedRoom);
            }
        }
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

        if (perpendicularOffsetFactor == -1)
        {
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
            RoomLink link = new RoomLink(roomGeneratorA, roomGeneratorB);
            pendingRoomLinks.Add(link);

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

            roomBEntry.x = (chosenDirection.x > 0 ? roomB.Right - 1 : roomB.Left) - roomB.Left;
            roomBEntry.y = roomB.Top - globalEntryParameter;
        }

        roomGeneratorA.AddEntry(roomAEntry);
        roomGeneratorB.AddEntry(roomBEntry);

        AddConnection(roomA, roomB, roomAEntry, roomBEntry, chosenDirection);

    }

    Coord PlaceRoom(RoomGeneration roomGenerator, RoomGeneration previousRoomGenerator, Coord[] directions, List<RoomNode> locatedRooms)
    {
        RoomNode room = roomGenerator.AssociatedRoom;
        RoomNode previousRoom;

        Coord chosenDirection = new Coord(0, 0);

        bool roomCollided = true;

        int spaceness = 0;

        if (previousRoomGenerator != null)
        {
            previousRoom = previousRoomGenerator.AssociatedRoom;

            while (roomCollided)
            {
                //Iterate through each possible direction
                for (int i = 0; i < directions.Length && roomCollided; i++)
                {
                    chosenDirection = directions[i];

                    SpawnRoom(room, previousRoom, chosenDirection, spaceness, .5f);

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
            int spacing = GetSpacing(previousRoomGenerator.AssociatedRoom, room, chosenDirection);

            if (spacing == 0) {
                AddRoomEntries(roomGenerator, previousRoomGenerator, chosenDirection);
            }
            else
            {
                //Add to the list of pending links
                RoomLink link = new RoomLink(previousRoomGenerator, roomGenerator);
                pendingRoomLinks.Add(link);
            }
        }

        return chosenDirection;
    }

    Coord ConvertRoomCoordinates(Coord localCoords, RoomNode originRoom, RoomNode targetRoom)
    {
        Coord targetCoords = new Coord(localCoords.x, localCoords.y);

        //Convert from corridor local to global
        targetCoords.x += originRoom.Left;
        targetCoords.y = originRoom.Top - targetCoords.y;

        //Convert from global to room local
        targetCoords.x -= targetRoom.Left;
        targetCoords.y = targetRoom.Top - targetCoords.y;

        //Restrict conversion with room boundaries
        targetCoords.x = targetCoords.x < 0 ? 0 : targetCoords.x;
        targetCoords.x = targetCoords.x > targetRoom.Width - 1 ? targetRoom.Width - 1 : targetCoords.x;

        targetCoords.y = targetCoords.y < 0 ? 0 : targetCoords.y;
        targetCoords.y = targetCoords.y > targetRoom.Height - 1 ? targetRoom.Height - 1 : targetCoords.y;

        return targetCoords;
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

    //An accurate algorithm for calculate an appropiate direction for linking rooms
    Coord GetDirectionBetween(RoomNode roomA, RoomNode roomB)
    {
        Dictionary<Coord, int> directionsDistance = new Dictionary<Coord, int>();
        
        Coord closestDirection = new Coord(0, 0);
        int closestDistance = int.MaxValue;

        //If room A shares some parallel coordinates with room B,
        //will check which one of the opposite pair of the rooms edges is closer to 0
        if (roomA.OverlapsValueX(roomB.Left, roomB.Right) || roomA.OverlapsValueY(roomB.Bottom, roomB.Top)) {

            //Up
            directionsDistance.Add(new Coord(0, 1), roomB.Bottom - roomA.Top);
            //Down
            directionsDistance.Add(new Coord(0, -1), roomB.Top - roomA.Bottom);
            //Left
            directionsDistance.Add(new Coord(-1, 0), roomB.Right - roomA.Left);
            //Right
            directionsDistance.Add(new Coord(1, 0), roomB.Left - roomA.Right);

            foreach (Coord direction in directionsDistance.Keys)
            {
                int distance = System.Math.Abs(directionsDistance[direction]);

                if (distance < closestDistance)
                {
                    if ((direction.x == 0 && roomA.OverlapsValueX(roomB.Left, roomB.Right))
                        || (direction.y == 0 && roomA.OverlapsValueY(roomB.Bottom, roomB.Top))) {
                        closestDirection = direction;
                        closestDistance = distance;
                    }
                }
            }
        }
        else
        {
            //If no near edge pair was found, we will base on direction vector...
            if (closestDirection.x == closestDirection.y)
            {
                Coord roomACenter;
                Coord roomBCenter;

                roomACenter.x = (roomA.Left + roomA.Right) / 2;
                roomACenter.y = (roomA.Top + roomA.Bottom) / 2;

                roomBCenter.x = (roomB.Left + roomB.Right) / 2;
                roomBCenter.y = (roomB.Top + roomB.Bottom) / 2;

                float distance = RoomDistance(roomA, roomB);
                float dX = (roomBCenter.x - roomACenter.x) / distance;
                float dY = (roomBCenter.y - roomACenter.y) / distance;

                if (System.Math.Abs(dX) > System.Math.Abs(dY))
                {
                    closestDirection.x = System.Math.Sign(dX);
                }
                else
                {
                    closestDirection.y = System.Math.Sign(dY);
                }
            }
        }

        return closestDirection;
    }

    int GetSpacing(RoomNode roomA, RoomNode roomB, Coord direction)
    {
        int boundA;
        int boundB;
        int spacing;

        if (direction.x == 0)
        {
            boundA = direction.y > 0 ? roomA.Top : roomA.Bottom;
            boundB = direction.y > 0 ? roomB.Bottom : roomB.Top;
        }
        else
        {
            boundA = direction.x > 0 ? roomA.Right : roomA.Left;
            boundB = direction.x > 0 ? roomB.Left : roomB.Right;
        }

        spacing = System.Math.Abs(boundB - boundA);

        return spacing;
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

    RoomGeneration CreateRoomGenerator(RoomNode room)
    {
        RoomGeneration generator;

        int generationSeed = random.Next();

        switch (room.Type)
        {
            case RoomType.Normal:
                generator = new NormalRoomGenerator(room, generationSeed, generationParameters.Level);
                break;

            case RoomType.Entrance:
                generator = new EntranceRoomGenerator(room, generationSeed, generationParameters.Level);
                break;

            default:
                generator = new NormalRoomGenerator(room, generationSeed, generationParameters.Level);
                break;
        }

        return generator;
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

    public int GetConnectionID(int idA, int idB)
    {
        int x = Math.Min(idA, idB);
        int y = Math.Max(idA, idB);

        return (int)(Math.Pow(2, x) * (2 * y + 1));
    }

    void AddConnection(RoomNode roomA, RoomNode roomB, Coord entryA, Coord entryB, Coord direction)
    {
        int connectionID = GetConnectionID(roomA.ID, roomB.ID);

        entryA.x += roomA.Position.x;
        entryA.y = roomA.Position.y - entryA.y;

        entryB.x += roomB.Position.x;
        entryB.y = roomB.Position.y - entryB.y;

        EntryConnection connection = new EntryConnection(roomA.ID, roomB.ID, entryA, entryB, direction.x == 0);

        Connections.Add(connectionID, connection);
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

    struct RoomLink
    {
        public RoomGeneration RoomA;
        public RoomGeneration RoomB;

        public RoomLink(RoomGeneration roomA, RoomGeneration roomB)
        {
            RoomA = roomA;
            RoomB = roomB;
        }
    }
}

public struct EntryConnection
{
    public Coord EntryA { get; private set; }
    public Coord EntryB { get; private set; }

    public bool IsVertical { get; private set; }

    public int RoomAID;
    public int RoomBID;

    public EntryConnection(int roomAID, int roomBID, Coord entryA, Coord entryB, bool isVertical)
    {
        EntryA = entryA;
        EntryB = entryB;

        RoomAID = roomAID;
        RoomBID = roomBID;

        IsVertical = isVertical;
    }
}