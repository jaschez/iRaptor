using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    public WorldGenerationParameters parameters;
    public bool autoSeed = true;

    WorldGraphGenerator generator;
    WorldGraphOutput output;
    GraphOutput graph;

    int[] sequence;

    string seq = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            seq = "";

            if (autoSeed)
            {
                int seed = new System.Random().Next();
                parameters.GraphParameters.Seed = seed;
            }

            generator = new WorldGraphGenerator(parameters);
            output = generator.GenerateWorldGraph();

            graph = output.GraphInfo;
            sequence = graph.PruferCode;

            foreach (int i in sequence)
            {
                seq += i.ToString() + ",";
            }

            Debug.Log("Seed: " + graph.Seed);
            Debug.Log("Sequence: " + seq);
            Debug.Log("Parent: " + graph.Nodes[0].ID);
            Debug.Log("Leaves: " + graph.Leaves.Length);

            foreach (List<RootedNode> loop in graph.Loops)
            {
                string loopStr = "Loop in: ";
                foreach (RootedNode node in loop)
                {
                    loopStr += node.ID + ",";
                }

                Debug.Log(loopStr);
            }

            foreach (RoomType roomType in output.FilteredRooms.Keys)
            {
                Debug.Log(roomType.ToString());
                foreach (RootedNode node in output.FilteredRooms[roomType])
                {
                    Debug.Log("ID: " + node.ID);
                    Debug.Log("Childs: " + node.Childs.Count.ToString());
                }
            }
        }
    }
}
