using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField]
    public GraphInput graphInput;
    public bool autoSeed = true;

    GraphGenerator graphGenerator;
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
                graphInput = new GraphInput(seed, graphInput.Size, graphInput.Leaves, graphInput.LoopNumber, graphInput.MinimumLoopLength);
            }
            
            graphGenerator = new GraphGenerator(graphInput);
            graph = graphGenerator.GenerateGraph();
            sequence = graph.PruferCode;

            foreach (int i in sequence)
            {
                seq += i.ToString() + ",";
            }

            foreach (RootedNode node in graph.Map)
            {
                if (node.Parent == null)
                {
                    Debug.Log("Parent ID: " + node.ID);
                }
            }

            foreach (List<RootedNode> loop in graph.Loops)
            {
                string loopStr = "Loop in: ";
                foreach (RootedNode node in loop)
                {
                    loopStr += node.ID + ",";
                }

                Debug.Log(loopStr);
            }

            Debug.Log("Seed: " + graph.Seed);
            Debug.Log(seq);
            Debug.Log("Leaves: " + graph.Leaves);

        }
    }
}
