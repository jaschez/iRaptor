using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalGeneration : RoomGeneration
{
    public NormalGeneration(RoomNode room, int seed) : base(room, seed)
    {
        Initialize(RoomType.Normal, random.Next(8, 10) * 3, random.Next(8, 10) * 3, 3, .4f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
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
}
