using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{
    public List<Coord> Coords { get; private set; }

    public Coord PointA { get; private set; }
    public Coord PointB { get; private set; }

    int passageSize = 2;

    public Corridor(Coord a, Coord b)
    {
        Coords = new List<Coord>();

        PointA = a;
        PointB = b;
    }

    public void Generate()
    {
        List<Coord> line = GetLine(PointA, PointB);

        Coords.Clear();

        foreach (Coord c in line)
        {
            List<Coord> circle = GetSurroundingCoords(c, passageSize);

            foreach (Coord areaCoord in circle)
            {
                if (!Coords.Contains(areaCoord))
                {
                    Coords.Add(areaCoord);
                }
            }
        }
    }

    public void ShiftCorridor(int x, int y)
    {
        PointA = ShiftCoord(PointA, x, y);
        PointB = ShiftCoord(PointB, x, y);

        for (int i = 0; i < Coords.Count; i++)
        {
            Coords[i] = ShiftCoord(Coords[i], x, y);
        }
    }

    Coord ShiftCoord(Coord c, int x, int y)
    {
        Coord r = c;

        r.x += x;
        r.y += y;

        return r;
    }

    List<Coord> GetSurroundingCoords(Coord c, int r)
    {

        List<Coord> surrounding = new List<Coord>();

        for (int circleX = -r; circleX <= r; circleX++)
        {
            for (int circleY = -r; circleY <= r; circleY++)
            {
                if (circleX * circleX + circleY * circleY <= r * r)
                {
                    int surroundX = c.x + circleX;
                    int surroundY = c.y + circleY;

                    surrounding.Add(new Coord(surroundX, surroundY));
                }
            }
        }

        return surrounding;
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.x;
        int y = from.y;

        int dx = to.x - from.x;
        int dy = to.y - from.y;

        bool inverted = false;
        int step = (int)Mathf.Sign(dx);
        int gradientStep = (int)Mathf.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = (int)Mathf.Sign(dy);
            gradientStep = (int)Mathf.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

}
