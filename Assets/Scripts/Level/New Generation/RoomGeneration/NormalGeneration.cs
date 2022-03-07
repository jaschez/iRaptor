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
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Rock);
    }
}
