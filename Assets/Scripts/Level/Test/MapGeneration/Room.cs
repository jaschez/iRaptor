using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    int[,] rawMap;
    public int[,] gameMap { get; private set; }

    int rawWidth;
    int rawHeight;

    float minFillPercentage = .4f;
    float maxFillPercentage = .6f;

    public float fillPercentage { get; private set; }

    public int gameWidth { get; private set; }
    public int gameHeight { get; private set; }

    //Lista de salas enlazadas, el primer valor corresponde con el indice de la sala enlazada a esta,
    // el segundo corresponde al indice de pasadizo en la lista de MapInfo
    public List<int> linkedRooms { get; private set; }

    public int maxLoot { get; private set; } = 1;

    int maxEnemies = 2;

    int scaleFactor;

    int id;

    public int distanceFromStart = 0;

    public List<Coord> floorCoords { get; private set; }
    public List<Entry> entries { get; private set; }
    public List<Coord> startPoints { get; private set; }
    public List<Coord> scaledStartPoints { get; private set; }
    public List<Coord> lootPoints { get; private set; }
    public List<Coord> interestingPoints { get; private set; }
    public List<Coord> enemyCoords { get; private set; }

    Coord worldPos;

    public int left { get; private set; }
    public int right { get; private set; }
    public int top { get; private set; }
    public int bottom { get; private set; }

    public Room(Coord pos, int rawW, int rawH, int _scaleFactor, int _id, float fillPerc)
    {
        worldPos = pos;

        scaleFactor = _scaleFactor;

        rawWidth = rawW;
        rawHeight = rawH;

        gameWidth = rawW * scaleFactor;
        gameHeight = rawH * scaleFactor;

        rawMap = new int[rawWidth, rawHeight];
        gameMap = new int[gameWidth, gameHeight];

        id = _id;

        fillPercentage = fillPerc;

        linkedRooms = new List<int>();
        floorCoords = new List<Coord>();
        entries = new List<Entry>();
        startPoints = new List<Coord>();
        scaledStartPoints = new List<Coord>();
        lootPoints = new List<Coord>();
        enemyCoords = new List<Coord>();

        SetWorldPosition(pos);

        InitRoom();
    }

    void InitRoom()
    {
        //By defalut, all the room is walls
        for (int i = 0; i < rawWidth; i++)
        {
            for (int j = 0; j < rawHeight; j++)
            {
                rawMap[i, j] = 1;
            }
        }
    }

    public void CreateRoom()
    {
        GenerateRandomWalk();
        ScaleGameMap();

        //Create floor coordinates

        for (int i = 0; i < gameWidth; i++)
        {
            for (int j = 0; j < gameHeight; j++)
            {
                if (gameMap[i, j] == 0)
                {
                    floorCoords.Add(new Coord(i, j));
                }
            }
        }

        //Calculate interesting room points and lootPoints

        CalculateInterestingCoords();

        CalculateLootCoords();

        CalculateEnemyCoords();
    }

    void GenerateRandomWalk()
    {
        List<int> activeExplorers = new List<int>();
        List<Coord> scaledPoints = new List<Coord>();

        Coord[] explorers;

        Coord currentExplorer;
        Coord dir;

        int[,,] flags;

        int totalRoomTiles = rawWidth * rawHeight;
        int floorTiles = 0;

        int activeExplorerIndex = 0;
        int explorerIndex;

        float targetFillPerc;
        float currentFillPerc = 0;

        if (fillPercentage != -1)
        {
            targetFillPerc = fillPercentage;
        }
        else
        {
            targetFillPerc = Random.Range(minFillPercentage, maxFillPercentage);
        }

        //Firstly we check if there's only one starting point
        //If that's the case, we create another one in the middle of the room

        if (startPoints.Count == 1)
        {
            AddEntry(new Coord((left + right) / 2, (top + bottom) / 2), EntryType.Central);
        }

        //Later, we scale the points to a smaller room format

        foreach (Coord c in startPoints)
        {

            Coord scaled = new Coord(c.x / scaleFactor, c.y / scaleFactor);
            scaledPoints.Add(scaled);
            scaledStartPoints.Add(scaled);
        }

        explorers = scaledPoints.ToArray();
        flags = new int[explorers.Length, rawWidth, rawHeight];

        for (int i = 0; i < explorers.Length; i++)
        {
            activeExplorers.Add(i);
        }

        //The main generation algorithm starts

        //Check first tile
        for (int i = 0; i < explorers.Length; i++)
        {
            Coord c = explorers[i];

            rawMap[c.x, c.y] = 0;
            flags[i, c.x, c.y] = 1;

            floorTiles++;

            currentFillPerc = floorTiles / (float)totalRoomTiles;
        }

        if (id == 0)
        {
            targetFillPerc = .8f;
        }

        while (activeExplorers.Count > 1 || currentFillPerc < targetFillPerc) //Mientras el numero de caminos sea mayor que 1 o el porcentaje de llenado sea menor que el establecido
        {

            explorerIndex = activeExplorers[activeExplorerIndex];
            currentExplorer = explorers[explorerIndex]; //El explorador esta siendo elegido para seguir trazando el camino...

            //Choose a random direction inside the room frontiers

            do
            {

                dir = ChooseDirection();

            } while (OutOfBounds(currentExplorer.x + dir.x, currentExplorer.y + dir.y, rawWidth, rawHeight));

            //Move the explorer 1 unit in the chosen direction

            currentExplorer.x += dir.x;
            currentExplorer.y += dir.y;

            //Mark the current position in the room and in the flag as explored

            if (rawMap[currentExplorer.x, currentExplorer.y] == 1)
            {
                rawMap[currentExplorer.x, currentExplorer.y] = 0;

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
                        if (Random.Range(0, 10) > 5)
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
        for (int i = 0; i < rawWidth; i++)
        {
            for (int j = 0; j < rawHeight; j++)
            {
                for (int xi = i * scaleFactor; xi < (i + 1) * scaleFactor; xi++)
                {
                    for (int yj = j * scaleFactor; yj < (j + 1) * scaleFactor; yj++)
                    {
                        gameMap[xi, yj] = rawMap[i, j];
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

        int[,] pGameMap = gameMap;

        for (int i = 0; i < gameWidth; i++)
        {
            for (int j = 0; j < gameHeight; j++)
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
        gameMap = pGameMap;
    }

    void CalculateInterestingCoords()
    {
        DensityMap densityMap = new DensityMap(gameMap, 1, 0, startPoints);

        List<List<Coord>> interestingAreas = densityMap.GetHighPeaks(0.01f, 2).OrderBy(area => area.Count).ToList();

        interestingPoints = new List<Coord>();

        foreach (List<Coord> area in interestingAreas)
        {
            interestingPoints.Add(area[Random.Range(0, area.Count)]);
        }
    }

    void CalculateLootCoords()
    {
        for (int i = 0; i < maxLoot && i < interestingPoints.Count; i++)
        {
            Coord lootPoint = interestingPoints[i];
            interestingPoints.RemoveAt(i);

            lootPoint = TakeCoordOffWall(lootPoint);

            lootPoints.Add(lootPoint);
        }
    }

    void CalculateEnemyCoords()
    {
        if (id != 0)
        {
            for (int i = 0; i < maxEnemies; i++)
            {
                enemyCoords.Add(floorCoords[Random.Range(0, floorCoords.Count)]);
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
                if (xOff >= 1 && xOff < gameWidth - 1 && yOff >= 1 && yOff < gameHeight - 1)
                {
                    if (xOff != x || yOff != y)
                    {
                        if (gameMap[xOff, yOff] == 1)
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
            if (x / scaleFactor == c.x && y / scaleFactor == c.y)
            {
                n = 0;
            }
        }

        return n;
    }

    Coord TakeCoordOffWall(Coord originalPoint)
    {
        int lessNeighbours = GetNeighbours(originalPoint.x, originalPoint.y);
        int newNeighbours;

        Coord betterCoord = new Coord();

        bool betterCoordFound = false;

        //Search for non wall sticked points

        for (int xOff = originalPoint.x - 1; xOff <= originalPoint.x + 1; xOff++)
        {
            for (int yOff = originalPoint.y - 1; yOff <= originalPoint.y + 1; yOff++)
            {
                if (!(xOff == originalPoint.x && yOff == originalPoint.y))
                {
                    if (xOff >= 0 && xOff < gameWidth && yOff >= 0 && yOff < gameHeight)
                    {
                        if (gameMap[xOff, yOff] == 0)
                        {
                            newNeighbours = GetNeighbours(xOff, yOff);

                            if (newNeighbours < lessNeighbours)
                            {
                                lessNeighbours = newNeighbours;
                                betterCoord = new Coord(xOff, yOff);

                                betterCoordFound = true;
                            }
                        }
                    }
                }
            }
        }

        if (betterCoordFound)
        {
            originalPoint = betterCoord;
        }

        return originalPoint;
    }

    Coord ChooseDirection()
    {
        Coord[] directions = { new Coord(1, 0), new Coord(-1, 0), new Coord(0, 1), new Coord(0, -1) };

        int p = Random.Range(0, 100);

        int index;

        if (p > 75)
        {
            index = 0;
        }
        else if (p > 50)
        {
            index = 1;
        }
        else if (p > 25)
        {
            index = 2;
        }
        else
        {
            index = 3;
        }

        return directions[index];
    }

    public bool IsEdge(int x, int y)
    {
        for (int xOff = x - 1; xOff <= x + 1; xOff++)
        {
            for (int yOff = y - 1; yOff <= y + 1; yOff++)
            {
                if (xOff >= 0 && xOff < gameWidth && yOff >= 0 && yOff < gameHeight)
                {
                    if ((xOff == x || yOff == y) && !(xOff == x && yOff == y))
                    {
                        if (gameMap[xOff, yOff] == 0)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    bool OutOfBounds(int x, int y, int w, int h)
    {
        return x < 0 || y < 0 || x >= w || y >= h;
    }

    public bool OverlapsRoom(Room another)
    {
        return OverlapsValueX(another.left, another.right) && OverlapsValueY(another.bottom, another.top);
    }

    public bool OverlapsValueX(int min, int max)
    {
        return right > min && max > left;
    }

    public bool OverlapsValueY(int min, int max)
    {
        return top > min && max > bottom;
    }

    public int GetId()
    {
        return id;
    }

    public void SetId(int i)
    {
        id = i;
    }

    public Coord GetWorldPosition()
    {
        return worldPos;
    }

    public void SetWorldPosition(Coord pos)
    {
        worldPos = pos;

        left = pos.x;
        right = pos.x + gameWidth;
        top = pos.y;
        bottom = pos.y - gameHeight;
    }

    public bool IsLinkedToRoomIndex(int index)
    {
        return linkedRooms.Contains(index);
    }

    public void LinkToRoomIndex(int index)
    {
        linkedRooms.Add(index);
    }

    public void AddEntry(Coord c, EntryType e, int linkedCorridor = -1)
    {
        entries.Add(new Entry(c, e, linkedCorridor));

        Coord localCoord = new Coord(c.x - worldPos.x, Mathf.Abs(c.y - worldPos.y));
        startPoints.Add(localCoord);
    }

    public struct Entry
    {
        public Coord coord;
        public EntryType type;

        public int linkedCorridorIndex;

        public Entry(Coord c, EntryType e, int linkedCorridor)
        {
            coord = c;
            type = e;
            linkedCorridorIndex = linkedCorridor;
        }
    }

    public enum EntryType
    {
        Horizontal,
        Vertical,
        Central
    }
}