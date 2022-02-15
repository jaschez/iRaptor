using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalGeneration : RoomGeneration
{
    public NormalGeneration(Coord localPosition, int seed) : base(localPosition, seed)
    {
        Initialize(RoomType.Normal, random.Next(24, 30), random.Next(24, 30), 3, .4f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
    }

    protected override void GenerateTileMap()
    {
        
    }
}
