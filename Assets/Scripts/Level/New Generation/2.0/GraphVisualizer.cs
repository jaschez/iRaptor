using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    GraphGenerator graph = new GraphGenerator(new int[] { 2, 1, 4, 1 , 3, 2 });

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
            if (autoSeed)
            {
                seed = new System.Random().Next();
            }

            sequence = graph.GenerateSequence(size, leaves, seed);
            graph = new GraphGenerator(sequence);
            UnrootedNode[] nodes = graph.GenerateGraph();

            foreach (int i in sequence)
            {
                seq += i.ToString() + ",";
            }

            foreach (UnrootedNode node in nodes)
            {
                if (node.Neighbours.Count == 1)
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
