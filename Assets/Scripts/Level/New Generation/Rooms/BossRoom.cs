using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : RoomNode
{
    public Coord Exit;

    public BossRoom(RoomNode room) : base(room)
    {

    }

    public override void Generate(RoomGeneration generator)
    {
        base.Generate(generator);
        Exit = ((BossRoomGenerator)generator).Exit;
    }
}