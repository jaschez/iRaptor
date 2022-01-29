using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    GraphGenerator graph;

    public int size = 0;
    public int leaves = 0;
    public int seed = 0;
    public bool autoSeed = true;

    int[] sequence;
    int calculatedLeaves = 0;

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
            calculatedLeaves = 0;

            if (autoSeed)
            {
                seed = new System.Random().Next();
            }
            
            graph = new GraphGenerator(seed, size, leaves);
            RootedNode[] nodes = graph.GenerateGraph();
            sequence = graph.PruferCode;

            foreach (int i in sequence)
            {
                seq += i.ToString() + ",";
            }

            foreach (RootedNode node in nodes)
            {
                if (node.Childs.Count == 0)
                {
                    calculatedLeaves++;
                }
            }

            Debug.Log("Seed: " + seed);
            Debug.Log(seq);
            Debug.Log("Leaves: " + calculatedLeaves);

        }
    }
}
