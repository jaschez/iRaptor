using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class TestMap : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase floor;
    public TileBase wall;

    public GameObject placeholder;

    GameObject parent;

    public int width, height;
    public int time = 2;

    [Range(0, 100)]
    public int fill;

    public int seed;
    public bool hasSeed = false;

    bool[,] map;

    static TestMap instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        CreateLevel();
        DontDestroyOnLoad(this);
    }

    void CreateLevel()
    {
        if (!hasSeed)
        {
            seed = Random.Range(-99999999,99999999);
        }

        Random.InitState(seed);

        Debug.Log("Seed " + seed);

        Destroy(parent);
        Generate();
        LoadLevel();

        List<Coord> floor = new List<Coord>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!map[i, j])
                {
                    floor.Add(new Coord(i, j));
                }
            }
        }

        parent = new GameObject();

        parent.transform.SetParent(transform);

        ObjectPlacement placer = new ObjectPlacement(Random.Range(-99999, 99999), new TileType[width, height], floor, floor[Random.Range(0, floor.Count)]);

        List<Coord> valid = placer.GetValidCoords();

        foreach (Coord c in valid)
        {
            Vector3 pos = CoordToVect(c, new Coord(0, 0));

            //Instantiate(placeholder, pos, Quaternion.identity, parent.transform);
        }

        for (int i = 0; i < 10; i++)
        {
            Coord place = placer.GenerateNextPoint(3);
            Debug.Log(place.x + "," + place.y);

            Vector3 pos = CoordToVect(place, new Coord(0, 0));

            Instantiate(placeholder, pos, Quaternion.identity, parent.transform);
        }
    }

    Vector3 CoordToVect(Coord c, Coord relativeTo, int tileSize = 16)
    {
        return new Vector3((relativeTo.x + (c.x + .5f)) * tileSize, (relativeTo.y - (c.y + .5f)) * tileSize, 1);
    }

    void Generate()
    {
        map = new bool[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map[i, j] = Random.Range(0, 100) > fill;
            }
        }

        for (int times = 0; times < time; times++)
        {
            bool[,] temp = new bool[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    temp[i, j] = map[i, j];
                }
            }

            //temp = map;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int n = CheckNeighbours(i, j);

                    if (n > 4)
                    {
                        map[i, j] = true;
                    }
                    else if (n < 4)
                    {
                        map[i, j] = false;
                    }
                }
            }

        }
    }

    int CheckNeighbours(int x, int y)
    {
        int n = 0;

        for (int xOff = x - 1; xOff <= x + 1; xOff++)
        {
            for (int yOff = y - 1; yOff <= y + 1; yOff++)
            {
                if (xOff != x || yOff != y)
                {
                    if (xOff < width - 1 && xOff > 0 && yOff < height - 1 && yOff > 0)
                    {
                        if (map[xOff, yOff])
                        {
                            n++;
                        }
                    }
                    else
                    {
                        n++;
                    }
                }
            }
        }

        return n;
    }

    public void LoadLevel()
    {

        tilemap.ClearAllTiles();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3Int tilePosition = new Vector3Int(i, -j, 1);
                TileBase tile = map[i, j]? wall : floor;

                tilemap.SetTile(tilePosition, tile);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(0);
            CreateLevel();
        }
    }
}
