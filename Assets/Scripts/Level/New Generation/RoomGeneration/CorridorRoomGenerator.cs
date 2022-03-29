using System.Collections;
using System.Collections.Generic;

public class CorridorRoomGenerator : RoomGeneration
{
    static int CorridorCounter = 100;

    int totalRoomTiles;

    bool[,] obstacleMap;
    bool valid;

    public CorridorRoomGenerator(int seed, int level, int top, int bottom, int left, int right)
        : base(new RoomNode(CorridorCounter, RoomType.Corridor), seed, level)
    {
        Initialize(RoomType.Corridor, right - left, top - bottom, 3, .4f);

        AssociatedRoom.SetWorldPosition(new Coord(left, top));

        CorridorCounter++;
    }

    public void GenerateObstacleMap(List<List<RoomNode>> worldComposites)
    {
        obstacleMap = new bool[simpleWidth, simpleHeight];
        totalRoomTiles = 0;

        //Get the list of rooms that overlaps with the corridor
        foreach (List<RoomNode> composite in worldComposites)
        {
            foreach (RoomNode room in composite)
            {
                if (AssociatedRoom.OverlapsRoom(room))
                {
                    for (int x = 0; x < simpleWidth; x++)
                    {
                        for (int y = 0; y < simpleHeight; y++)
                        {
                            int cellLeft = (x * widthScale) + AssociatedRoom.Left;
                            int cellRight = ((x + 1) * widthScale - 1) + AssociatedRoom.Left;
                            int cellTop = AssociatedRoom.Top - (y * heightScale);
                            int cellBottom = AssociatedRoom.Top - ((y + 1) * heightScale - 1);

                            //Except for empty tiles the rest are obstacles

                            //Check if the cell bounds collide with the room bounds
                            bool result = (room.Right > cellLeft && room.Left < cellRight)
                                && (room.Top > cellBottom && room.Bottom < cellTop);

                            if (result) {
                                obstacleMap[x, y] = true;
                            }
                        }
                    }
                }
            }
        }
    }

    protected override void GenerateMap()
    {
        valid = false;

        for (int x = 0; x < simpleWidth; x++)
        {
            for (int y = 0; y < simpleHeight; y++)
            {
                if (!obstacleMap[x,y])
                {
                    totalRoomTiles++;
                }
                else
                {
                    simpleMap[x, y] = TileType.Empty;
                }
            }
        }

        ExplorersGeneration();
        ScaleCorridor();
        OpenEntries();
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Rock);
    }

    void ExplorersGeneration()
    {
        List<int> activeExplorers = new List<int>();

        Coord[] explorers;

        Coord currentExplorer;
        Coord dir;

        int[,,] flags;
        int floorTiles = 0;

        int activeExplorerIndex = 0;
        int explorerIndex;

        //Later, we scale the points to a smaller room format
        foreach (Coord c in StartPoints)
        {
            int scaledX = c.x / widthScale;
            int scaledY = c.y / heightScale;

            scaledX = scaledX <= simpleWidth - 1 ? scaledX : simpleWidth - 1;
            scaledY = scaledY <= simpleHeight - 1 ? scaledY : simpleHeight - 1;

            Coord scaled = new Coord(scaledX, scaledY);
            ScaledStartPoints.Add(scaled);
        }

        explorers = ScaledStartPoints.ToArray();
        flags = new int[explorers.Length, simpleWidth, simpleHeight];

        for (int i = 0; i < explorers.Length; i++)
        {
            activeExplorers.Add(i);
        }

        //The main generation algorithm starts

        //Check first tiles of exploration
        for (int i = 0; i < explorers.Length; i++)
        {
            Coord c = explorers[i];

            simpleMap[c.x, c.y] = TileType.Floor;
            flags[i, c.x, c.y] = 1;

            floorTiles++;
        }

        if (activeExplorers.Count > 0)
        {
            //Check if all start points are intersecting each other
            //If that is the case its not necesary to explore anymore
            for (int i = 0; i < explorers.Length; i++)
            {
                Coord c = explorers[i];

                for (int flagIndex = 0; flagIndex < explorers.Length; flagIndex++)
                {
                    if (flagIndex != i && activeExplorers.Contains(i))
                    {
                        if (IntersectingExplorer(flags, flagIndex, c))
                        {
                            activeExplorers.Remove(flagIndex);
                        }
                    }
                }
            }

            while (activeExplorers.Count > 1 && floorTiles < totalRoomTiles) //Mientras el numero de caminos sea mayor que 1
            {
                explorerIndex = activeExplorers[activeExplorerIndex];
                currentExplorer = explorers[explorerIndex]; //El explorador esta siendo elegido para seguir trazando el camino...

                //Choose a random direction inside the room frontiers
                bool validPosition = false;

                do
                {
                    dir = ChooseDirection();

                    if (!OutOfBounds(currentExplorer.x + dir.x, currentExplorer.y + dir.y, simpleWidth, simpleHeight))
                    {
                        validPosition = !obstacleMap[currentExplorer.x + dir.x, currentExplorer.y + dir.y];
                    }

                } while (!validPosition);

                //Move the explorer 1 unit in the chosen direction

                currentExplorer.x += dir.x;
                currentExplorer.y += dir.y;

                //Mark the current position in the room and in the flag as explored

                if (simpleMap[currentExplorer.x, currentExplorer.y] == TileType.Wall)
                {
                    simpleMap[currentExplorer.x, currentExplorer.y] = TileType.Floor;
                    floorTiles++;
                }

                flags[explorerIndex, currentExplorer.x, currentExplorer.y] = 1;

                //Checking if the current path intersects (or touches) another room path from an active explorer

                for (int flagI = 0; flagI < explorers.Length; flagI++)
                {
                    if (flagI != explorerIndex && activeExplorers.Contains(flagI))
                    {
                        if (IntersectingExplorer(flags, flagI, currentExplorer))
                        {
                            //Elige que explorador de los caminos intersectados deja de estar activo
                            if (random.Next(0, 10) > 5)
                            {
                                activeExplorers.Remove(explorerIndex);
                            }
                            else
                            {
                                activeExplorers.Remove(flagI);
                            }
                        }
                    }
                }

                explorers[explorerIndex] = currentExplorer;

                activeExplorerIndex = (activeExplorerIndex + 1) % activeExplorers.Count;
            }

            if (activeExplorers.Count <= 1)
            {
                valid = true;
            }
        }
    }

    bool IntersectingExplorer(int[,,] flags, int flagIndex, Coord explorer)
    {
        for (int x = explorer.x - 1; x <= explorer.x + 1; x++)
        {
            for (int y = explorer.y - 1; y <= explorer.y + 1; y++)
            {
                if (!OutOfBounds(x, y, flags.GetLength(1), flags.GetLength(2)))
                {
                    if (x == explorer.x || y == explorer.y) {
                        if (flags[flagIndex, x, y] == 1)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    void ScaleCorridor()
    {

        for (int i = 0; i < simpleWidth; i++)
        {
            for (int j = 0; j < simpleHeight; j++)
            {
                for (int xi = i * widthScale; xi < (i + 1) * widthScale; xi++)
                {
                    for (int yj = j * heightScale; yj < (j + 1) * heightScale; yj++)
                    {
                        Map[xi, yj] = simpleMap[i, j];
                    }
                }
            }
        }

        //Extend empty tiles to the bounds of the unscaled map
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int scaledX = (x / widthScale) > simpleWidth - 1 ? simpleWidth - 1 : x / widthScale;
                int scaledY = (y / heightScale) > simpleHeight - 1 ? simpleHeight - 1 : y / heightScale;

                if (simpleMap[scaledX, scaledY] == TileType.Empty) {
                    Map[x, y] = TileType.Empty;
                }
            }
        }

        for (int s = 0; s < 3; s++)
        {
            SmoothMap();
        }
    }

    public bool AddValidCorridorEntry(Coord entry, Coord validationDirection, Coord validationLimit)
    {
        Coord currentPos = new Coord(entry.x, entry.y);

        bool resizeX = false;
        bool resizeY = false;

        if (currentPos.x > (simpleWidth - 1) * widthScale)
        {
            currentPos.x = (simpleWidth - 1) * widthScale;
            resizeX = true;
        }

        if (currentPos.y > (simpleHeight - 1) * heightScale)
        {
            currentPos.y = (simpleHeight - 1) * heightScale;
            resizeY = true;
        }

        validationLimit.x = validationLimit.x > (simpleWidth - 1) * widthScale ? (simpleWidth - 1) * widthScale : validationLimit.x;
        validationLimit.y = validationLimit.y > (simpleHeight - 1) * heightScale ? (simpleHeight - 1) * heightScale : validationLimit.y;

        while (!(currentPos.x == validationLimit.x && currentPos.y == validationLimit.y))
        {
            if (!obstacleMap[currentPos.x / widthScale, currentPos.y / heightScale])
            {
                if (resizeX)
                {
                    currentPos.x = entry.x;
                }

                if (resizeY)
                {
                    currentPos.y = entry.y;
                }

                AddEntry(currentPos);

                return true;
            }

            currentPos.x += validationDirection.x;
            currentPos.y += validationDirection.y;
        }

        return false;
    }

    public bool IsValidCorridor()
    {
        return valid;
    }
}
