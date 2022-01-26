using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject square;
    WorldGraph graph;

    private void Start()
    {
        //Instantiate(square, Vector3.zero, Quaternion.identity);
        graph = new WorldGraph();

        graph.Init();
        graph.Generate();

        Debug.Log(graph.GetSeed());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            graph = new WorldGraph();
            graph.Init();
            graph.Generate();

            Debug.Log(graph.GetSeed());
        }
    }

    bool inLoop = false;
    int currentLoopID = -1;
    float xLoopCoord = 0;

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.black;
        Stack<Node> stack = new Stack<Node>();
        Dictionary<int, int> organization = new Dictionary<int, int>();
        int loopCount = 0;

        if (graph != null)
        {
            if (graph.Start != null)
            {
                stack.Push(graph.Start);
                while (stack.Count > 0)
                {
                    Node current = stack.Pop();

                    if (current.FromLoop && !inLoop)
                    {
                        inLoop = true;
                        xLoopCoord = organization.ContainsKey(current.Depth) ? organization[current.Depth] + .5f : 0.5f;
                        loopCount = 0;

                        foreach (Node child in current.Childs)
                        {
                            if (child.LoopID != -1)
                            {
                                currentLoopID = child.LoopID;
                                break;
                            }
                        }
                    }
                    else if (!current.FromLoop && inLoop)
                    {
                        inLoop = false;
                    }

                    if (!organization.ContainsKey(current.Depth))
                    {
                        organization.Add(current.Depth, 0);
                    }
                    else
                    {
                        organization[current.Depth]++;
                    }

                    int i = 0;

                    if (organization.ContainsKey(current.Depth + 1))
                    {
                        i = organization[current.Depth + 1] + 1;
                    }

                    Color nodeColor = Color.black;

                    switch (current.roomType)
                    {
                        case RoomType.Entrance:
                            nodeColor = Color.blue;
                            break;

                        case RoomType.Normal:
                            nodeColor = Color.gray;
                            break;

                        case RoomType.Reward:
                            nodeColor = Color.yellow;
                            break;

                        case RoomType.Shop:
                            nodeColor = Color.green;
                            break;

                        case RoomType.Miniboss:
                            nodeColor = Color.red;
                            break;

                        case RoomType.Boss:
                            nodeColor = Color.black;
                            break;

                        default:
                            break;
                    }

                    if (!inLoop)
                    {

                        if (current.Childs != null)
                        {
                            foreach (Node child in current.Childs)
                            {
                                stack.Push(child);

                                Gizmos.color = Color.black;
                                Gizmos.DrawLine(new Vector3(organization[current.Depth] * 1.3f, -current.Depth, 1) * 2, new Vector3(i * 1.3f, -current.Depth - 1, 1) * 2);

                                i++;
                            }
                        }

                        Gizmos.color = nodeColor;
                        Gizmos.DrawCube(new Vector3(organization[current.Depth] * 1.3f, -current.Depth, 1) * 2, Vector3.one);
                    }
                    else
                    {
                        if (current.Childs != null)
                        {
                            Node loopChild = null;
                            foreach (Node child in current.Childs)
                            {
                                if (current.Depth < child.Depth)
                                {
                                    if (child.FromLoop && child.LoopID == currentLoopID)
                                    {
                                        loopChild = child;
                                    }
                                    else
                                    {
                                        stack.Push(child);
                                    }
                                }

                                if (child.Depth > current.Depth) {
                                    Gizmos.color = Color.black;
                                    
                                    bool finalLoopChild = false;
                                    foreach (Node notFinalChild in current.Childs)
                                    {
                                        if (notFinalChild.Depth < current.Depth)
                                        {
                                            finalLoopChild = true;
                                            break;
                                        }
                                    }

                                    if (finalLoopChild)
                                    {
                                        Gizmos.DrawLine(new Vector3(loopCount == 0 ? organization[current.Depth] : xLoopCoord, -current.Depth, 1) * 2, new Vector3(child.FromLoop ? xLoopCoord : i, -child.Depth, 1) * 2);
                                    }
                                    else
                                    {
                                        Gizmos.DrawLine(new Vector3(loopCount == 0 ? organization[current.Depth] : xLoopCoord, -current.Depth, 1) * 2, new Vector3(xLoopCoord + i, -child.Depth, 1) * 2);
                                    }
                                }
                                else
                                {
                                    Gizmos.color = Color.magenta;
                                    Gizmos.DrawLine(new Vector3(xLoopCoord, -current.Depth, 1) * 2, new Vector3(-xLoopCoord - 1, (-child.Depth - current.Depth)/2f, 1) * 2);
                                    Gizmos.DrawLine(new Vector3(-xLoopCoord - 1, (-child.Depth - current.Depth) / 2f, 1) * 2, new Vector3(organization[child.Depth], -child.Depth, 1) * 2);
                                }

                                Gizmos.color = nodeColor;

                                if (loopCount == 0)
                                {
                                    Gizmos.DrawCube(new Vector3(organization[current.Depth], -current.Depth, 1) * 2, Vector3.one);
                                }
                                else
                                {
                                    Gizmos.DrawCube(new Vector3(xLoopCoord, -current.Depth, 1) * 2, Vector3.one);
                                }

                                i++;
                            }

                            if (loopChild != null)
                            {
                                stack.Push(loopChild);
                            }
                        }

                        loopCount++;
                    }

                }
            }
        }
    }
}
