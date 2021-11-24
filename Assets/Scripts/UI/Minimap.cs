using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    static Minimap instance;

    public Image minimapImg;

    private LevelManager levelManager;

    private MapInfo minimapInfo;

    private RectTransform rect;

    private Transform player;

    private Vector2 mapPos;

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

    void Update()
    {
        if (Controls.GetMoveKey())
        {
            if (minimapInfo.map != null && player != null) {
                UpdateMapPosition(player.position);
            }
        }
    }

    public void DrawMapInfo(MapInfo mapInfo)
    {
        minimapInfo = mapInfo;

        int width = mapInfo.width;
        int height = mapInfo.height;

        imgWidth = width * 3;
        imgHeight = height * 3;

        Texture2D texture = new Texture2D(width, height);

        texture.filterMode = FilterMode.Point;

        Color[] colourMap = new Color[width * height];

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
        /*
        foreach (Coord c in mapInfo.enemyCoords)
        {
            colourMap[c.y * width + c.x] = Color.red;

        }*/

        texture.SetPixels(colourMap);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero, 1);
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

    public void GenerateMinimap()
    {
        levelManager = LevelManager.GetInstance();

        StartCoroutine(GenerateWhenReady());
    }

    IEnumerator GenerateWhenReady()
    {
        while (minimapInfo.map == null)
        {
            minimapInfo = levelManager.GetMapInfo();
            yield return null;
        }

        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            yield return null;
        }

        DrawMapInfo(minimapInfo);
    }

    public static Minimap GetInstance()
    {
        return instance;
    }
}
