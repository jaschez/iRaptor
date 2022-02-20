using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RoomGeneration
{
    protected System.Random random;

    public RoomType GenerationType { get; protected set; }

    public RoomNode AssociatedRoom { get; protected set; }

    protected int[,] Map { get; private set; }

    public TileType[,] TileMap { get; private set; }

    //Dimensions
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int ScaleFactor { get; private set; }

    public int Left { get; private set; }
    public int Right { get; private set; }
    public int Top { get; private set; }
    public int Bottom { get; private set; }

    public float FillPercentage { get; private set; }

    int[,] simpleMap;

    int simpleWidth;
    int simpleHeight;

    public List<Coord> FloorCoords { get; private set; }
    public List<Room.Entry> entries { get; private set; }
    public List<Coord> startPoints { get; private set; }
    public List<Coord> scaledStartPoints { get; private set; }
    public List<Coord> interestingPoints { get; private set; }

    public RoomGeneration(RoomNode room, int seed)
    {
        random = new System.Random(seed);

        AssociatedRoom = room;

        FloorCoords = new List<Coord>();
        entries = new List<Room.Entry>();
        startPoints = new List<Coord>();
        scaledStartPoints = new List<Coord>();
    }

    protected void Initialize(RoomType roomType, int width, int height, int scaleFactor, float fillPercentage)
    {
        GenerationType = roomType;

        Width = width;
        Height = height;
        ScaleFactor = scaleFactor;

        FillPercentage = fillPercentage;

        Map = new int[Width, Height];
        TileMap = new TileType[Width, Height];

        AssociatedRoom.SetDimensions(Width, Height);

        simpleWidth = Width / scaleFactor;
        simpleHeight = Height / scaleFactor;

        simpleMap = new int[simpleWidth, simpleHeight];

        //By defalut, all the room is walls
        for (int i = 0; i < simpleWidth; i++)
        {
            for (int j = 0; j < simpleHeight; j++)
            {
                simpleMap[i, j] = 1;
            }
        }
    }

    public void Generate()
    {
        GenerateMap();
        CalculateFloorCoordinates();
        CalculateInterestingCoords();
        GenerateTileMap();

        AssociatedRoom.Generate(TileMap, FloorCoords);
    }

    protected abstract void GenerateMap();

    protected abstract void GenerateTileMap();

    protected void DefaultMapGeneration()
    {
        GenerateRandomWalk();
        ScaleGameMap();
    }

    void GenerateRandomWalk()
    {
        List<int> activeExplorers = new List<int>();
        List<Coord> scaledPoints = new List<Coord>();

        Coord[] explorers;

        Coord currentExplorer;
        Coord dir;

        int[,,] flags;

        int totalRoomTiles = simpleWidth * simpleHeight;
        int floorTiles = 0;

        int activeExplorerIndex = 0;
        int explorerIndex;

        float currentFillPerc = 0;

        //Firstly we check if there's only one starting point
        //If that's the case, we create another one in the middle of the room

        if (startPoints.Count == 1)
        {
            AddEntry(new Coord((Left + Right) / 2, (Top + Bottom) / 2));
        }

        //Later, we scale the points to a smaller room format

        foreach (Coord c in startPoints)
        {

            Coord scaled = new Coord(c.x / ScaleFactor, c.y / ScaleFactor);
            scaledPoints.Add(scaled);
            scaledStartPoints.Add(scaled);
        }

        explorers = scaledPoints.ToArray();
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

            currentFillPerc = floorTiles / (float)totalRoomTiles;
        }

        while (activeExplorers.Count > 1 || currentFillPerc < FillPercentage) //Mientras el numero de caminos sea mayor que 1 o el porcentaje de llenado sea menor que el establecido
        {

            explorerIndex = activeExplorers[activeExplorerIndex];
            currentExplorer = explorers[explorerIndex]; //El explorador esta siendo elegido para seguir trazando el camino...

            //Choose a random direction inside the room frontiers

            do
            {
                dir = ChooseDirection();

            } while (OutOfBounds(currentExplorer.x + dir.x, currentExplorer.y + dir.y, simpleWidth, simpleHeight));

            //Move the explorer 1 unit in the chosen direction

            currentExplorer.x += dir.x;
            currentExplorer.y += dir.y;

            //Mark the current position in the room and in the flag as explored

            if (simpleMap[currentExplorer.x, currentExplorer.y] == 1)
            {
                simpleMap[currentExplorer.x, currentExplorer.y] = 0;

                floorTiles++;
                currentFillPerc = floorTiles / (float)totalRoomTiles;
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
    }

    void ScaleGameMap()
    {
        for (int i = 0; i < simpleWidth; i++)
        {
            for (int j = 0; j < simpleHeight; j++)
            {
                for (int xi = i * ScaleFactor; xi < (i + 1) * ScaleFactor; xi++)
                {
                    for (int yj = j * ScaleFactor; yj < (j + 1) * ScaleFactor; yj++)
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

    void SmoothMap()
    {
        int[,] pGameMap = Map;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                int n = GetNeighbours(i, j);

                if (n > 4)
                {
                    pGameMap[i, j] = 1;
                }
                else if (n < 4)
                {
                    pGameMap[i, j] = 0;
                }
            }
        }
        Map = pGameMap;
    }

    int GetNeighbours(int x, int y)
    {
        int n = 0;

        for (int xOff = x - 1; xOff <= x + 1; xOff++)
        {
            for (int yOff = y - 1; yOff <= y + 1; yOff++)
            {
                if (xOff >= 1 && xOff < Width - 1 && yOff >= 1 && yOff < Height - 1)
                {
                    if (xOff != x || yOff != y)
                    {
                        if (Map[xOff, yOff] == 1)
                        {
                            n++;
                        }
                    }
                }
                else
                {
                    n++;
                }
            }
        }

        foreach (Coord c in scaledStartPoints)
        {
            if (x / ScaleFactor == c.x && y / ScaleFactor == c.y)
            {
                n = 0;
            }
        }

        return n;
    }

    void CalculateFloorCoordinates()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (Map[i, j] == 0)
                {
                    FloorCoords.Add(new Coord(i, j));
                }
            }
        }
    }

    void CalculateInterestingCoords()
    {
        DensityMap densityMap = new DensityMap(Map, 1, 0, startPoints);

        List<List<Coord>> interestingAreas = densityMap.GetHighPeaks(0.01f, 2).OrderBy(area => area.Count).ToList();

        interestingPoints = new List<Coord>();

        foreach (List<Coord> area in interestingAreas)
        {
            interestingPoints.Add(area[random.Next(0, area.Count)]);
        }
    }

    public void AddEntry(Coord localCoord)
    {
        startPoints.Add(localCoord);
    }

    Coord ChooseDirection()
    {
        Coord[] directions = { new Coord(1, 0), new Coord(-1, 0), new Coord(0, 1), new Coord(0, -1) };

        int index = random.Next(0, directions.Length);

        return directions[index];
    }

    bool OutOfBounds(int x, int y, int w, int h)
    {
        return x < 0 || y < 0 || x >= w || y >= h;
    }
}
