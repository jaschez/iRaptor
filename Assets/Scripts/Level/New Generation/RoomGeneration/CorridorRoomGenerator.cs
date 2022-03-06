using System.Collections;
using System.Collections.Generic;

public class CorridorRoomGenerator : RoomGeneration
{
    static int CorridorCounter = 100;

    int totalRoomTiles;

    bool[,] obstacleMap;
    bool valid;

    public CorridorRoomGenerator(int seed, int top, int bottom, int left, int right)
        : base(new RoomNode(CorridorCounter, RoomType.Corridor), seed)
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
                            int cellLeft = (x * widthScale) + Left;
                            int cellRight = (x * (widthScale + 1) - 1) * +Left;
                            int cellTop = Top - (y * heightScale);
                            int cellBottom = Top - (y * (heightScale + 1) - 1);

                            //Check if the cell bounds collide with the room bounds
                            bool result = (room.Right > cellLeft || room.Left < cellRight)
                                && (room.Top > cellBottom || room.Bottom < cellTop);

                            obstacleMap[x, y] = result;
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
            }
        }

        ExplorersGeneration();
        ScaleCorridor();
        OpenEntries();
    }

    protected override void GenerateTileMap()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (Map[i, j] == 1)
                {
                    TileMap[i, j] = TileType.Wall_Rock;
                }
                else
                {
                    TileMap[i, j] = TileType.Floor_Rock;
                }
            }
        }
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

        //Check first tile
        for (int i = 0; i < explorers.Length; i++)
        {
            Coord c = explorers[i];

            simpleMap[c.x, c.y] = 0;
            flags[i, c.x, c.y] = 1;

            floorTiles++;
        }

        if (activeExplorers.Count > 0)
        {
            //Check if all start points are literally on the same location
            Coord point = new Coord(-1, -1);
            bool sameLocation = true;
            foreach (Coord c in ScaledStartPoints)
            {
                if (point.x == -1 && point.y == -1)
                {
                    point = c;
                }
                else if (c.x != point.x || c.y != point.y)
                {
                    sameLocation = false;
                }
            }

            //If all explorers have the same coordinates, its not necesary to explore anymore
            if (sameLocation)
            {
                activeExplorers.Clear();
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

                if (simpleMap[currentExplorer.x, currentExplorer.y] == 1)
                {
                    simpleMap[currentExplorer.x, currentExplorer.y] = 0;
                    floorTiles++;
                }

                flags[explorerIndex, currentExplorer.x, currentExplorer.y] = 1;

                //Checking if the current path intersects another room path from an active explorer

                for (int flagI = 0; flagI < explorers.Length; flagI++)
                {
                    if (flagI != explorerIndex && activeExplorers.Contains(flagI))
                    {
                        if (flags[flagI, currentExplorer.x, currentExplorer.y] == 1)
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

        for (int s = 0; s < 3; s++)
        {
            SmoothMap();
        }
    }

    public bool IsValidCorridor()
    {
        return valid;
    }
}
