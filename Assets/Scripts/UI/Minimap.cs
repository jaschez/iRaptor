using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    static Minimap instance;

    public Image minimapImg;

    private LevelManager levelManager;

    private Texture2D mapTexture;

    private WorldGenerator generatorInfo;

    private RectTransform rect;

    private Transform player;

    private Vector2 mapPos;

    Coord mapOrigin;

    RoomNode entrance;

    Color[] colourMap;
    Color[] discoveredColourMap;

    TileType[,] tileMap;

    List<int> discoveredRooms;

    int width;
    int height;

    float imgWidth = 0;
    float imgHeight = 0;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        rect = minimapImg.rectTransform;
    }

    void ProcessTileMap()
    {
        //Calculate width and height
        Coord minimumCoord = new Coord(int.MaxValue, int.MinValue);
        Coord maximumCoord = new Coord(int.MinValue, int.MaxValue);

        Coord roomMinimum;
        Coord roomMaximum;

        foreach (List<RoomNode> composite in generatorInfo.RoomComposites)
        {
            foreach (RoomNode room in composite)
            {
                roomMinimum = room.Position;
                roomMaximum = new Coord(room.Position.x + room.Width, room.Position.y - room.Height);

                if (roomMinimum.x < minimumCoord.x)
                {
                    minimumCoord.x = roomMinimum.x;
                }

                if (roomMinimum.y > minimumCoord.y)
                {
                    minimumCoord.y = roomMinimum.y;
                }

                if (roomMaximum.x > maximumCoord.x)
                {
                    maximumCoord.x = roomMaximum.x;
                }

                if (roomMaximum.y < maximumCoord.y)
                {
                    maximumCoord.y = roomMaximum.y;
                }

                if (room.Type == RoomType.Entrance)
                {
                    entrance = room;
                }
            }
        }

        width = maximumCoord.x - minimumCoord.x;
        height = minimumCoord.y - maximumCoord.y;

        mapOrigin = minimumCoord;

        tileMap = new TileType[width, height];

        //Initialize to empty
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tileMap[x, y] = TileType.Empty;
            }
        }

        //Get map from generated source
        foreach (List<RoomNode> composite in generatorInfo.RoomComposites)
        {
            foreach (RoomNode room in composite)
            {
                TileType[,] map = room.TileTypeMap;

                for (int y = 0; y < room.Height; y++)
                {
                    for (int x = 0; x < room.Width; x++)
                    {
                        TileType tile = map[x, y];
                        Coord tileMapPos = new Coord(room.Position.x + x - mapOrigin.x, mapOrigin.y - (room.Position.y - y));

                        if (tile == TileType.Wall)
                        {
                            if (CountNeighbours(map, TileType.Floor, new Coord(x, y), room.Width, room.Height) > 0)
                            {
                                tileMap[tileMapPos.x, tileMapPos.y] = TileType.Wall;
                            }
                            else
                            {
                                tileMap[tileMapPos.x, tileMapPos.y] = TileType.Empty;
                            }
                        }
                        else
                        {
                            tileMap[tileMapPos.x, tileMapPos.y] = map[x, y];
                        }
                    }
                }
            }
        }
    }

    void Initialize()
    {
        imgWidth = width * 5;
        imgHeight = height * 5;

        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;

        colourMap = new Color[width * height];
        discoveredColourMap = new Color[width * height];

        discoveredRooms = new List<int>();
    }

    void Update()
    {
        if (Controls.GetMoveKey())//Debe arreglarse
        {
            if (tileMap != null && player != null) {
                UpdateMapPosition(player.position);
            }
        }
    }

    public void GenerateColourMap()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tileMap[x, height - y - 1] == TileType.Floor)
                {
                    colourMap[y * width + x] = new Color(.8f,.8f,.8f);
                }
                else if(tileMap[x, height - y - 1] == TileType.Wall)
                {
                    colourMap[y * width + x] = new Color(.2f, .2f, .2f);
                }
                else
                {
                    colourMap[y * width + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        /*foreach (Coord c in mapInfo.lootCoords)
        {
            colourMap[c.y * width + c.x] = Color.magenta;
        }*/
    }

    public void UpdateMapRegion(RoomNode room)
    {
        int roomWidth = room.Width;
        int roomHeight = room.Height;

        int colorIndex;

        Coord worldPos = room.Position;

        if (!discoveredRooms.Contains(room.ID))
        {
            discoveredRooms.Add(room.ID);

            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    colorIndex = (height - mapOrigin.y + worldPos.y - y - 1) * width + (x - mapOrigin.x) + worldPos.x;
                    discoveredColourMap[colorIndex] = colourMap[colorIndex];
                }
            }

            //Print entries
            foreach (Coord entry in room.Entries)
            {
                colorIndex = (height - mapOrigin.y + worldPos.y - entry.y - 1) * width + (entry.x - mapOrigin.x) + worldPos.x;

                Color entryColor = Color.blue;
                bool nearEntry = false;

                //If an entry on the next room is already printed, dont print this one
                for (int x = entry.x - 1; x <= entry.x + 1 && !nearEntry; x++)
                {
                    for (int y = entry.y - 1; y <= entry.y + 1 && !nearEntry; y++)
                    {
                        int coordIndex = (height - mapOrigin.y + worldPos.y - y - 1) * width + (x - mapOrigin.x) + worldPos.x;

                        if (x == entry.x || y == entry.y)
                        {
                            if (discoveredColourMap[coordIndex] == entryColor)
                            {
                                nearEntry = true;

                                //Delete last print
                                colorIndex = coordIndex;
                                entryColor = new Color(.8f, .8f, .8f);
                            }
                        }
                    }
                }

                int step = 0;

                if (entry.x == 0 || entry.x == room.Width - 1)
                {
                    step = width;
                }
                else if (entry.y == 0 || entry.y == room.Height - 1)
                {
                    step = 1;
                }

                discoveredColourMap[colorIndex] = entryColor;
                discoveredColourMap[colorIndex + step] = entryColor;
                discoveredColourMap[colorIndex - step] = entryColor;
            }
        }

        /*foreach (int corridorIndex in room.LinkedCorridors)
        {
            Corridor corridor = minimapInfo.corridors[corridorIndex];

            foreach (Coord c in corridor.Coords)
            {
                colourMap[-c.y * width + c.x] = new Color(.8f, .8f, .8f);
            }
        }*/

        /*foreach (Coord c in room.Loot)
        {
            colourMap[(worldPos.y - c.y) * width + c.x + worldPos.x] = Color.magenta;
        }*/

        mapTexture.SetPixels(discoveredColourMap);
        mapTexture.Apply();

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, width, height), Vector2.zero, 1);
        minimapImg.sprite = sprite;

        minimapImg.rectTransform.sizeDelta = new Vector2(imgWidth, imgHeight);

        minimapImg.preserveAspect = true;

        UpdateMapPosition(player.position);
    }

    void UpdateMapPosition(Vector2 playerPos)
    {
        mapPos.x = (int)((playerPos.x - mapOrigin.x * 16) / (width * 16) * imgWidth);
        mapPos.y = (int)((playerPos.y + (height - mapOrigin.y) * 16 - 8) / (height * 16) * imgHeight);

        rect.localPosition = -mapPos;
    }

    public void Generate()
    {
        levelManager = LevelManager.GetInstance();

        StartCoroutine(GenerateWhenReady());
    }

    IEnumerator GenerateWhenReady()
    {
        while (generatorInfo == null)
        {
            generatorInfo = levelManager.GetGeneratorInfo();
            yield return null;
        }

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            yield return null;
        }

        ProcessTileMap();
        Initialize();
        GenerateColourMap();

        //Print first room
        UpdateMapRegion(entrance);
    }

    int CountNeighbours(TileType[,] map, TileType tileTarget, Coord position, int width, int height)
    {
        int neighbours = 0;

        for (int x = position.x - 1; x <= position.x + 1; x++)
        {
            for (int y = position.y - 1; y <= position.y + 1; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (x != position.x || y != position.y)
                    {
                        if (map[x, y] == tileTarget)
                        {
                            neighbours++;
                        }
                    }
                }
            }
        }

        return neighbours;
    }

    public static Minimap GetInstance()
    {
        return instance;
    }
}
