using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityMap
{
    int[,] source;
    int[,] density;
    int[,] highPeaksMap;

    int filterTarget;
    int filterGround;

    int width;
    int height;

    int maximumValue = 0;
    int minimumValue = int.MaxValue;

    List<Coord> connections;

    public DensityMap(int[,] map, int target, int ground, List<Coord> conns = null)
    {
        source = map;

        width = map.GetLength(0);
        height = map.GetLength(1);

        density = new int[width, height];

        filterTarget = target;
        filterGround = ground;

        if (conns != null)
        {
            connections = conns;
        }
        else {
            connections = new List<Coord>();
        }
    }

    public List<List<Coord>> GetHighPeaks(float percentile, int radius)
    {
        int threshold;

        highPeaksMap = new int[width, height];

        CalculateMap(radius);

        threshold = (int)(maximumValue * (1.0f - percentile));

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (density[i, j] >= threshold)
                {
                    highPeaksMap[i, j] = 1;
                }
                else
                {
                    highPeaksMap[i, j] = 0;
                }
            }
        }

        return CalculateHighPeakAreas();
    }

    void CalculateMap(int amplitude)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (source[i, j] == filterGround)
                {

                    int value = GetDensityAt(i, j, amplitude, filterTarget);

                    Coord valCoord = new Coord(i, j);

                    foreach (Coord con in connections)
                    {
                        if (GetDistance(valCoord, con) <= 64)
                        {
                            value = (int)(value * .5f);

                            break;
                        }
                    }

                    density[i, j] = value;

                    if (value > maximumValue)
                    {
                        maximumValue = value;
                    }

                    if (value < minimumValue)
                    {
                        minimumValue = value;
                    }
                }
            }
        }
    }

    List<List<Coord>> CalculateHighPeakAreas()
    {
        List<List<Coord>> areas = new List<List<Coord>>();
        int[,] flags = new int[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (highPeaksMap[i, j] == 1 && flags[i, j] == 0)
                {
                    List<Coord> areaCoords = new List<Coord>();
                    Queue<Coord> queue = new Queue<Coord>();

                    queue.Enqueue(new Coord(i, j));
                    flags[i, j] = 1;

                    while (queue.Count > 0)
                    {
                        Coord c = queue.Dequeue();

                        //Find neighbours
                        for (int nX = c.x - 2; nX <= c.x + 2; nX++)
                        {
                            for (int nY = c.y - 2; nY <= c.y + 2; nY++)
                            {
                                if (nX >= 0 && nX < width && nY >= 0 && nY < height)
                                {
                                    //Get horizontal, vertical, and diagonal neighbours
                                    if (!(nX == c.x && nY == c.y))
                                    {
                                        if (highPeaksMap[nX, nY] == 1 && flags[nX, nY] == 0)
                                        {
                                            queue.Enqueue(new Coord(nX, nY));
                                            flags[nX, nY] = 1;
                                        }
                                    }
                                }
                            }
                        }

                        areaCoords.Add(c);
                    }

                    areas.Add(areaCoords);
                }
            }
        }

        return areas;
    }

    int GetDensityAt(int x, int y, int amplitude, int target)
    {
        int density = 0;

        for (int xOff = x - amplitude; xOff <= x + amplitude; xOff++)
        {
            for (int yOff = y - amplitude; yOff <= y + amplitude; yOff++)
            {
                if (xOff >= 0 && xOff < width && yOff >= 0 && yOff < height)
                {
                    if (source[xOff, yOff] == target)
                    {
                        density++;
                    }
                }
                else
                {
                    density++;
                }
            }
        }

        return density;
    }

    int GetDistance(Coord a, Coord b)
    {
        int deltaX = a.x - b.x;
        int deltaY = a.y - b.y;

        return deltaX * deltaX + deltaY * deltaY;
    }
}
