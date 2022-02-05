using System.Collections.Generic;
using UnityEngine;

public struct WorldGraphOutput
{
    public readonly Dictionary<RoomType, List<RootedNode>> FilteredRooms;
    public readonly GraphOutput GraphInfo;

    public WorldGraphOutput(
        Dictionary<RoomType, List<RootedNode>> FilteredRooms,
        GraphOutput GraphInfo
    )
    {
        this.FilteredRooms = FilteredRooms;
        this.GraphInfo = GraphInfo;
    }
}