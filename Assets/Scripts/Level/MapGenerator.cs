using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{

    int[,] map;
    int[,] gameMap;

    int width;
    int height;

    int mapWidth;
    int mapHeight;

    int childCount = 0;
    int maxChild = 2;

    int tileSize = 16;
    int scaleFactor = 2; 

    int startX;
    int startY;

    int playerX;
    int playerY;

    int maxLoot = 4;

    Coord[] directions = new Coord[] { new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1), new Coord(1, 0) };

    MapInfo mapInfo;

    List<Coord> lootCoords;
    List<Coord> enemyCoords;

    List<Coord> floorCoords;

    public MapGenerator(int w, int h, int fillPercentage)
    {
        mapWidth = w;
        mapHeight = h;

        width = w * scaleFactor;
        height = h * scaleFactor;

        map = new int[w, h];
        gameMap = new int[w * scaleFactor, h * scaleFactor];
        childCount = 0;

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                map[i, j] = 1;
            }
        }

        lootCoords = new List<Coord>();
        enemyCoords = new List<Coord>();
        floorCoords = new List<Coord>();
    }

    public MapInfo GenerateMap(int smoothLevel)
    {

        startX = mapWidth / 2;
        startY = mapHeight / 2;

        GenerateRandomWalkMap(startX, startY, 400);
        ScaleGameMap();
        SmoothMap(smoothLevel);

        for(int i = 0; i < width; i++)
        {
            for (int j = 0;j < height; j++)
            {
                if(gameMap[i, j] == 0)
                {
                    Coord c = new Coord(i, j);

                    floorCoords.Add(c);
                }
            }
        }

        while (lootCoords.Count < maxLoot)
        {

            Coord c = floorCoords[Random.Range(0, floorCoords.Count)];

            lootCoords.Add(c);
        }

        while (lootCoords.Count > maxLoot)
        {

            lootCoords.RemoveAt(lootCoords.Count - 1);
        }

        mapInfo.width = width;
        mapInfo.height = height;

        mapInfo.tileSize = tileSize;
        mapInfo.map = gameMap;

        mapInfo.PlayerCoord = new Coord(playerX, playerY);
        mapInfo.lootCoords = lootCoords;
        mapInfo.enemyCoords = enemyCoords;

        return mapInfo;
    }

    void GenerateRandomWalkMap(int _x, int _y, int maxIterations, bool child = true, int id = 0)
    {
        int x = _x;
        int y = _y;
        int possibleX;
        int possibleY;
        int numOfTries;
        int iterations = 0;
        int lastChildIteration = 0;

        map[x, y] = 0;

        Coord step = new Coord(0, 0);
        Coord possibleStep;

        while (iterations < maxIterations)
        {
            numOfTries = 0;

            do
            {
                possibleStep = directions[ChooseDirection()];
                possibleX = x + possibleStep.x;
                possibleY = y + possibleStep.y;

                numOfTries++;

                if (Random.Range(0, 100) < 15 && child && childCount < maxChild)
                {
                    if (iterations - lastChildIteration > 30) {
                        GenerateRandomWalkMap(x, y, maxIterations / 2, false, id + 1);
                        lastChildIteration = iterations;
                        childCount++;
                        continue;
                    }
                }

            } while (((possibleStep.x == -step.x && possibleStep.y == -step.y) || OutOfBounds(possibleX, possibleY, mapWidth, mapHeight) || map[possibleX, possibleY] == 0 || GetMapNeighbours(possibleX, possibleY) < 2) && numOfTries < 10);

            if (numOfTries > 10)
            {
                break;
            }

            if (!OutOfBounds(x, y, mapWidth, mapHeight))
            {
                x = possibleX;
                y = possibleY;

                map[x, y] = 0;

                if (possibleStep.x == -step.x && possibleStep.y == -step.y && lootCoords.Count < maxLoot)
                {
                    lootCoords.Add(new Coord(x * scaleFactor, y * scaleFactor));
                }
            }

            step = possibleStep;

            iterations++;
        }

        if(id != 0)
        {
            lootCoords.Add(new Coord(x * scaleFactor, y * scaleFactor));
        }
        else
        {
            playerX = x * scaleFactor;
            playerY = y * scaleFactor;
        }
    }

    void ScaleGameMap()
    {
        startX *= scaleFactor;
        startY *= scaleFactor;

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                for (int xi = i * scaleFactor; xi < (i + 1) * scaleFactor; xi++)
                {
                    for (int yj = j * scaleFactor; yj < (j + 1) * scaleFactor; yj++)
                    {
                        gameMap[xi, yj] = map[i, j];
                    }
                }
            }
        }
    }

    void SmoothMap(int smoothLevel)
    {
        int[,] tempMap;

        for (int iterations = 0; iterations < smoothLevel; iterations++)
        {
            tempMap = gameMap;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbours = GetNeighbours(x, y);

                    if (neighbours > 4)
                    {
                        tempMap[x, y] = 1;

                        //Recolocate enities if necessary
                        Coord finalCoord;

                        if (playerX == x && playerY == y)
                        {
                            finalCoord = RecolocateCoord(new Coord(x, y), tempMap);

                            playerX = finalCoord.x;
                            playerY = finalCoord.y;
                        }
                        
                        for (int index = 0; index < lootCoords.Count; index++)
                        {
                            Coord c = lootCoords[index];

                            if (c.x == x && c.y == y)
                            {
                                lootCoords[index] = RecolocateCoord(c, tempMap);
                            }
                        }
                    }
                    else if (neighbours < 4)
                    {
                        tempMap[x, y] = 0;
                    }
                }
            }

            gameMap = tempMap;
        }
    }

    int GetNeighbours(int X, int Y)
    {
        int neighbours = 0;

        for (int xOffset = X - 1; xOffset <= X + 1; xOffset++)
        {
            for (int yOffset = Y - 1; yOffset <= Y + 1; yOffset++)
            {
                if (xOffset >= 0 && xOffset < width && yOffset >= 0 && yOffset < height)
                {
                    if (xOffset != X || yOffset != Y)
                    {
                        if (gameMap[xOffset, yOffset] == 1)
                        {
                            neighbours++;
                        }
                    }
                }
                else
                {
                    neighbours++;
                }
            }
        }

        return neighbours;
    }

    int GetMapNeighbours(int X, int Y)
    {
        int neighbours = 0;

        for (int xOffset = X - 1; xOffset <= X + 1; xOffset++)
        {
            for (int yOffset = Y - 1; yOffset <= Y + 1; yOffset++)
            {
                if (xOffset >= 0 && xOffset < width && yOffset >= 0 && yOffset < height)
                {
                    if (xOffset != X || yOffset != Y)
                    {
                        if (map[xOffset, yOffset] == 1)
                        {
                            neighbours++;
                        }
                    }
                }
                else
                {
                    neighbours++;
                }
            }
        }

        return neighbours;
    }

    bool OutOfBounds(int x, int y, int w, int h)
    {
        return x < 1 || y < 1 || x > (w - 2) || y > (h - 2);
    }

    int ChooseDirection()
    {
        int p = Random.Range(0, 100);

        if (p > 75)
        {
            return 0;
        }
        else if (p > 50)
        {
            return 1;
        }
        else if (p > 25)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    Coord RecolocateCoord(Coord c, int[,] tempMap)
    {
        Coord finalCoord = new Coord(-1, -1);

        bool found = false;

        //Search nearby tiles
        for (int xOff = c.x - 1; xOff <= c.x + 1 && !found; xOff++)
        {
            for (int yOff = c.y - 1; yOff <= c.y + 1 && !found; yOff++)
            {
                if (xOff != c.x || yOff != c.y)
                {
                    if (!OutOfBounds(xOff, yOff, width, height))
                    {
                        if (tempMap[xOff, yOff] == 0)
                        {
                            finalCoord.x = xOff;
                            finalCoord.y = yOff;
                            found = true;
                        }
                    }
                }
            }
        }

        return finalCoord;
    }


}

/*public struct Coord
{
    public int x, y;

    public Coord(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}*/
