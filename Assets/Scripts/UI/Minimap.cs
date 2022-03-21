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

    private MapInfo minimapInfo;

    private RectTransform rect;

    private Transform player;

    private Vector2 mapPos;

    Color[] colourMap;

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

    void Initialize()
    {
        width = minimapInfo.width;
        height = minimapInfo.height;

        imgWidth = width * 3;
        imgHeight = height * 3;

        mapTexture = new Texture2D(width, height);
        mapTexture.filterMode = FilterMode.Point;

        colourMap = new Color[width * height];
    }

    void Update()
    {
        if (Controls.GetMoveKey())//Debe arreglarse
        {
            if (minimapInfo.map != null && player != null) {
                UpdateMapPosition(player.position);
            }
        }
    }

    public void DrawMapInfo(MapInfo mapInfo)
    {

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (mapInfo.map[x, y] == 1)
                {
                    colourMap[y * width + x] = new Color(.8f,.8f,.8f);
                }
                else if(mapInfo.map[x, y] == 2)
                {
                    colourMap[y * width + x] = new Color(.2f, .2f, .2f);
                }
                else
                {
                    colourMap[y * width + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        foreach (Coord c in mapInfo.lootCoords)
        {
            colourMap[c.y * width + c.x] = Color.magenta;

        }

        mapTexture.SetPixels(colourMap);
        mapTexture.Apply();

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, width, height), Vector2.zero, 1);
        minimapImg.sprite = sprite;

        minimapImg.rectTransform.sizeDelta = new Vector2(imgWidth, imgHeight);

        minimapImg.preserveAspect = true;

        UpdateMapPosition(player.position);
    }

    public void UpdateMapRegion(Room room)
    {
        //int[,] map = room.gameMap;

        int roomWidth = room.Width;
        int roomHeight = room.Height;

        int colorIndex;

        Coord worldPos = room.Position;

        

        foreach (Coord tile in room.Floor)
        {
            colorIndex = (tile.y - worldPos.y) * width + tile.x + worldPos.x;
            colourMap[colorIndex] = new Color(.8f, .8f, .8f);
        }

        foreach (int corridorIndex in room.LinkedCorridors)
        {
            Corridor corridor = minimapInfo.corridors[corridorIndex];

            foreach (Coord c in corridor.Coords)
            {
                colourMap[-c.y * width + c.x] = new Color(.8f, .8f, .8f);
            }
        }

        foreach (Coord c in room.Loot)
        {
            colourMap[(c.y - worldPos.y) * width + c.x + worldPos.x] = Color.magenta;
        }

        mapTexture.SetPixels(colourMap);
        mapTexture.Apply();

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, width, height), Vector2.zero, 1);
        minimapImg.sprite = sprite;

        minimapImg.rectTransform.sizeDelta = new Vector2(imgWidth, imgHeight);

        minimapImg.preserveAspect = true;

        UpdateMapPosition(player.position);
    }

    void UpdateMapPosition(Vector2 playerPos)
    {
        mapPos.x = (int)(playerPos.x / (minimapInfo.width * minimapInfo.tileSize) * imgWidth);
        mapPos.y = (int)(playerPos.y / (minimapInfo.height * minimapInfo.tileSize) * imgHeight);
        rect.localPosition = -mapPos;
    }

    public void Generate()
    {
        levelManager = LevelManager.GetInstance();

        StartCoroutine(GenerateWhenReady());
    }

    IEnumerator GenerateWhenReady()
    {
        while (minimapInfo.map == null)
        {
            //minimapInfo = levelManager.GetMapInfo();
            yield return null;
        }

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            yield return null;
        }

        //DrawMapInfo(minimapInfo);
        Initialize();
        UpdateMapRegion(minimapInfo.rooms[0]);
    }

    public static Minimap GetInstance()
    {
        return instance;
    }
}
