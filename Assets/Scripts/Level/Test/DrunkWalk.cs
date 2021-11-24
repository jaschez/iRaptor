using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DrunkWalk : MonoBehaviour
{

    public GameObject prefabF;
    public GameObject prefabW;

    bool[,] map;
    bool[,] gameMap;

    int scaleFactor = 3;

    Vector2[] directions;

    int w = 50, h = 50;

    int maxChild = 2;
    int childCount = 0;

    int mapCount = 0;
    int gameCount = 0;

    void Start()
    {
        map = new bool[w,h];
        gameMap = new bool[w * scaleFactor, h * scaleFactor];

        directions = new Vector2[]{Vector2.up, Vector2.left, Vector2.down, Vector2.right};

        GenerateMap(25, 10, 80);
        RenderGameMap();
        InstantiateMap();

        Debug.Log("map:" + mapCount + ", game: " + gameCount);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void RenderGameMap()
    {
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                for (int xi = i * scaleFactor; xi < (i + 1) * scaleFactor; xi++)
                {
                    for (int yj = j * scaleFactor; yj < (j + 1) * scaleFactor; yj++)
                    {
                        gameMap[xi, yj] = map[i, j];
                        gameCount++;
                    }
                }
            }
        }

        for (int s = 0; s < 3; s++)
        {
            SmoothMap();
        }
    }

    void SmoothMap()
    {

        bool[,] pGameMap = gameMap;

        for (int i = 0; i < w * scaleFactor; i++)
        {
            for (int j = 0; j < h * scaleFactor; j++)
            {
                int n = GetNeighbours(i, j);

                if (n > 4)
                {
                    pGameMap[i, j] = false;
                }
                else if (n < 4)
                {
                    pGameMap[i, j] = true;
                }
            }
        }
        gameMap = pGameMap;
    }


    int GetNeighbours(int x, int y)
    {
        int n = 0;

        for (int xOff = x - 1; xOff <= x + 1; xOff++)
        {
            for (int yOff = y - 1; yOff <= y + 1; yOff++)
            {
                if (xOff >= 0 && xOff < w * scaleFactor && yOff >= 0 && yOff < h * scaleFactor)
                {
                    if (xOff != x || yOff != y) {
                        if (!gameMap[xOff, yOff])
                        {
                            n++;
                        }
                    }
                }
                else
                {
                    n++;
                }
            }
        }

        return n;
    }

    void InstantiateMap()
    {
        for(int i = 0; i < w * scaleFactor; i++)
        {
            for (int j = 0; j < h * scaleFactor; j++)
            {
                if (gameMap[i, j])
                {
                    Instantiate(prefabF, new Vector3(i * 16 - 25 * 16, j * 16 - 25 * 16, 0), Quaternion.identity);
                }
            }
        }
    }

    void GenerateMap(int _x, int _y, int maxIterations, bool child = true)
    {
        int x = _x;
        int y = _y;
        int possibleX = 0;
        int possibleY = 0;

        int numOfTries = 0;

        int iterations = 0;
        Vector2 step = Vector2.zero;
        Vector2 possibleStep;

        while (iterations < maxIterations)
        {
            numOfTries = 0;

            do
            {
                possibleStep = directions[ChooseDirection()];

                possibleX = x + (int)possibleStep.x;
                possibleY = y + (int)possibleStep.y;

                numOfTries++;

                if (Random.Range(0,100) < 3 && child && childCount < maxChild)
                {
                    GenerateMap(possibleX, possibleY, maxIterations/2, false);
                    childCount++;
                    continue;
                }

            } while ((possibleStep == -step || OutOfBounds(possibleX, possibleY, w, h) || map[possibleX, possibleY]) && numOfTries < 10);

            if (numOfTries > 10)
            {
                break;
            }

            step = possibleStep;

            x = possibleX;
            y = possibleY;

            if (!OutOfBounds(x, y, w, h))
            {
                map[x, y] = true;
                mapCount++;
            }

            iterations++;
        }
    }

    bool OutOfBounds(int x, int y, int w, int h)
    {
        return x < 0 || y < 0 || x >= w || y >= h;
    }

    int ChooseDirection()
    {
        int p = Random.Range(0, 100);
        if (p > 60)
        {
            return 0;
        }else if(p > 35)
        {
            return 3;
        }
        else if(p > 10)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
}
