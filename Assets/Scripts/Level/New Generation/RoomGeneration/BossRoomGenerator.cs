using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomGenerator : RoomGeneration
{
    public Coord Exit;

    public BossRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        Initialize(RoomType.Boss, random.Next(400, 500), 3, .8f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
    }

    protected override void AdditionalGeneration()
    {
        Exit = new Coord(Width / 2, Height / 2);
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Shop);
    }
}
