using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    WorldGraphGenerator graphGenerator;
    WorldGraphOutput graphOutput;

    public WorldGenerator(WorldGenerationParameters param)
    {
        graphGenerator = new WorldGraphGenerator(param);
        graphOutput = graphGenerator.GenerateWorldGraph();
    }

    
}

public class RoomNode
{
    public RoomType Type { get; private set; }
    public List<RoomNode> Neighbours { get; private set; }

    public RoomNode()
    {

    }

    public void SetNode(RoomType Type)
    {
        this.Type = Type;
    }

    public void AddNeighbour(RoomNode Neighbour)
    {
        if (!Neighbours.Contains(Neighbour))
        {
            Neighbours.Add(Neighbour);
        }
    }
}
