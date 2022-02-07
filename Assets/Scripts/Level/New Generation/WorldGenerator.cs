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
