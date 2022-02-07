using System.Collections.Generic;
using UnityEngine;

public struct WorldGraphOutput
{
    public readonly Dictionary<RoomType, List<RootedNode>> FilteredRooms;
    public readonly List<RootedNode> Rooms;
    public readonly GraphOutput GraphInfo;

    public WorldGraphOutput(
        Dictionary<RoomType, List<RootedNode>> FilteredRooms,
        List<RootedNode> Rooms,
        GraphOutput GraphInfo
    )
    {
        this.FilteredRooms = FilteredRooms;
        this.Rooms = Rooms;
        this.GraphInfo = GraphInfo;
    }
}