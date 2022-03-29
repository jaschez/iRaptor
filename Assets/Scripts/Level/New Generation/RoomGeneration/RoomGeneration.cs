using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class RoomGeneration
{
    protected System.Random random;

    public RoomType GenerationType { get; protected set; }

    public RoomNode AssociatedRoom { get; protected set; }

    public TileType[,] Map { get; private set; }

    public TileSkin[,] TileMap { get; private set; }

    //Dimensions
    public int Width { get; private set; }
    public int Height { get; private set; }

    public int ScaleFactor { get; private set; }

    public int Left { get; private set; }
    public int Right { get; private set; }
    public int Top { get; private set; }
    public int Bottom { get; private set; }

    public float FillPercentage { get; private set; }

    protected TileType[,] simpleMap;

    protected int simpleWidth;
    protected int simpleHeight;

    protected int widthScale;
    protected int heightScale;

    public List<Coord> FloorCoords { get; private set; }
    public List<Coord> StartPoints { get; private set; }
    protected List<Coord> ScaledStartPoints { get; private set; }
    public List<Coord> InterestingPoints { get; private set; }
    public List<Tuple<Coord, float>> LightPoints { get; private set; }

    public RoomGeneration(RoomNode room, int seed, int level)
    {
        random = new Random(seed);

        AssociatedRoom = room;

        FloorCoords = new List<Coord>();
        StartPoints = new List<Coord>();
        ScaledStartPoints = new List<Coord>();
    }

    protected void Initialize(RoomType roomType, int width, int height, int scaleFactor, float fillPercentage)
    {
        GenerationType = roomType;

        Width = width;
        Height = height;
        ScaleFactor = scaleFactor;

        FillPercentage = fillPercentage;

        Map = new TileType[Width, Height];
        TileMap = new TileSkin[Width, Height];

        AssociatedRoom.SetDimensions(Width, Height);

        widthScale = Width >= scaleFactor ? scaleFactor : Width;
        heightScale = Height >= scaleFactor ? scaleFactor : Height;

        simpleWidth = Width / widthScale;
        simpleHeight = Height / heightScale;

        simpleMap = new TileType[simpleWidth, simpleHeight];

        //By defalut, all the room is walls
        for (int i = 0; i < simpleWidth; i++)
        {
            for (int j = 0; j < simpleHeight; j++)
            {
                simpleMap[i, j] = TileType.Wall;
            }
        }
    }

    public void Generate()
    {
        GenerateMap();
        CalculateFloorCoordinates();
        CalculateInterestingCoords();
        CalculateLightPoints();
        GenerateTileMap();

        AdditionalGeneration();

        //We transfer the generated data to the associated room
        AssociatedRoom.Generate(this);
    }

    protected abstract void GenerateMap();

    protected abstract void GenerateTileMap();

    protected virtual void AdditionalGeneration()
    {

    }

    protected void DefaultMapGeneration()
    {
        GenerateRandomWalk();
        ScaleGameMap();
        OpenEntries();
    }

    protected void DefaultTilemapGeneration(TileSkin floor, TileSkin wall)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                switch (Map[i, j])
                {
                    case TileType.Wall:
                        TileMap[i, j] = wall;
                        break;

                    case TileType.Floor:
                        TileMap[i, j] = floor;
                        break;

                    case TileType.Empty:
                        TileMap[i, j] = TileSkin.Empty;
                        break;

                    default:
                        break;
                }
            }
        }
    }

    void GenerateRandomWalk()
    {
        List<int> activeExplorers = new List<int>();
        List<Coord> scaledPoints = new List<Coord>();
        List<Coord> centralEntries = new List<Coord>();

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

        //(We will get it out of the entry list after the map is generated)

        if (StartPoints.Count == 1)
        {
            Coord centralEntry = new Coord(Width / 2, Height / 2);

            AddEntry(centralEntry);
            centralEntries.Add(centralEntry);
        }

        //Later, we scale the points to a smaller room format

        foreach (Coord c in StartPoints)
        {

            Coord scaled = new Coord(c.x / ScaleFactor, c.y / ScaleFactor);
            scaledPoints.Add(scaled);
            ScaledStartPoints.Add(scaled);
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

            simpleMap[c.x, c.y] = TileType.Floor;
            flags[i, c.x, c.y] = 1;

            floorTiles++;

            currentFillPerc = floorTiles / (float)totalRoomTiles;
        }

        if (activeExplorers.Count > 0) {
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

                if (simpleMap[currentExplorer.x, currentExplorer.y] == TileType.Wall)
                {
                    simpleMap[currentExplorer.x, currentExplorer.y] = TileType.Floor;

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

        foreach (Coord centralEntry in centralEntries)
        {
            StartPoints.Remove(centralEntry);
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

    protected void SmoothMap()
    {
        TileType[,] pGameMap = Map;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                int n = GetNeighbours(i, j);

                if (Map[i, j] != TileType.Empty) {
                    if (n > 4)
                    {
                        pGameMap[i, j] = TileType.Wall;
                    }
                    else if (n < 4)
                    {
                        pGameMap[i, j] = TileType.Floor;
                    }
                }
                else
                {
                    pGameMap[i, j] = TileType.Empty;
                }
            }
        }

        Map = pGameMap;
    }

    protected void OpenEntries()
    {
        foreach (Coord entry in StartPoints)
        {
            for (int x = entry.x - 1; x <= entry.x + 1; x++)
            {
                for (int y = entry.y - 1; y <= entry.y + 1; y++)
                {
                    if (!OutOfBounds(x, y, Width, Height))
                    {
                        Map[x, y] = TileType.Floor;
                    }
                }
            }
        }
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
                        if (Map[xOff, yOff] == TileType.Wall || Map[xOff, yOff] == TileType.Empty)
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

        foreach (Coord c in ScaledStartPoints)
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
                if (Map[i, j] == TileType.Floor)
                {
                    FloorCoords.Add(new Coord(i, j));
                }
            }
        }
    }

    void CalculateInterestingCoords()
    {
        DensityMap<TileType> densityMap = new DensityMap<TileType>(Map, TileType.Floor);

        List<List<Coord>> interestingAreas = densityMap.GetHighPeaks(0.3f, 4);

        InterestingPoints = new List<Coord>();

        foreach (List<Coord> area in interestingAreas)
        {
            foreach (Coord point in area)
            {
                InterestingPoints.Add(point);
            }
        }
    }

    void CalculateLightPoints()
    {
        LightPoints = new List<Tuple<Coord, float>>();

        Coord averageCoord = new Coord();

        foreach (Coord c in InterestingPoints)
        {
            averageCoord.x += c.x;
            averageCoord.y += c.y;
        }

        averageCoord.x /= InterestingPoints.Count;
        averageCoord.y /= InterestingPoints.Count;

        LightPoints.Add(new Tuple<Coord, float>(averageCoord, InterestingPoints.Count * 2f));
    }

    public void AddEntry(Coord localCoord)
    {
        StartPoints.Add(localCoord);
    }

    protected Coord ChooseDirection()
    {
        Coord[] directions = { new Coord(1, 0), new Coord(-1, 0), new Coord(0, 1), new Coord(0, -1) };

        int index = random.Next(0, directions.Length);

        return directions[index];
    }

    protected bool OutOfBounds(int x, int y, int w, int h)
    {
        return x < 0 || y < 0 || x >= w || y >= h;
    }
}

public enum TileType
{
    Floor,
    Wall,
    Empty
}
