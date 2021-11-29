using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{

    static LevelGenerator instance;

    DungeonGenerator mapGenerator;

    public Tilemap tilemap;

    public GameObject[] drops;

    public GameObject player;
    public GameObject enemyPrefab;
    public GameObject lootPrefab;
    public GameObject eggPrefab;
    public GameObject exitPrefab;

    MapInfo mapInfo;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public MapInfo GenerateLevel(int width, int height, int fillPercentage, int smoothness, int seed, bool connectCaves, int enemyNumber, int lootNumber)
    {

        Random.InitState(seed);

        mapGenerator = new DungeonGenerator();

        mapInfo = mapGenerator.CreateMap();

        Vector3 playerPos = CoordToVect(mapInfo.PlayerCoord, mapInfo.tileSize);

        player.transform.position = playerPos;

        LoadLevelFromMap(mapInfo);

        return mapInfo;
    }

    void LoadLevelFromMap(MapInfo mapInfo)
    {
        Tile wall = Resources.Load("wall2", typeof(Tile)) as Tile;
        Tile floor = Resources.Load("floor", typeof(Tile)) as Tile;
        /*Tile floor2 = Resources.Load("floor2", typeof(Tile)) as Tile;
        Tile floor3 = Resources.Load("floor3", typeof(Tile)) as Tile;*/

        Vector3Int position;

        int[,] map = mapInfo.map;

        int width = mapInfo.width;
        int height = mapInfo.height;

        tilemap.ClearAllTiles();

        //Capa de bloques de cueva
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                position = new Vector3Int(i, j, 1);

                if (map[i, j] == 1)
                {
                    tilemap.SetTile(position, floor);
                }
                else if(map[i, j] == 2)
                {
                    tilemap.SetTile(position, wall);
                }
            }
        }

        //Capa de recompensas
        foreach (Coord c in mapInfo.lootCoords)
        {
            Instantiate(lootPrefab, CoordToVect(c, mapInfo.tileSize), Quaternion.identity);
        }

        
        //Capa de enemigos
        foreach (Coord c in mapInfo.enemyCoords)
        {
            Instantiate(enemyPrefab, CoordToVect(c, mapInfo.tileSize), Quaternion.identity);
        }

        /*
        //Capa de generadores
        foreach (Vector2 c in mapInfo.GetEggCoords())
        {
            Instantiate(eggPrefab, new Vector3(c.x + 0.5f, c.y + 0.5f, 1) * 16, Quaternion.identity);
        }*/

        Instantiate(exitPrefab, new Vector3(mapInfo.ExitPos.x + 0.5f, mapInfo.ExitPos.y + 0.5f, 1) * 16, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        int tileSize = mapInfo.tileSize;

        if (mapInfo.rooms != null)
        {
            foreach (Room r in mapInfo.rooms)
            {
                if (r.GetId() >= 0)
                {
                    Vector3 tl = new Vector3(r.left, -r.top, 1) * tileSize;
                    Vector3 tr = new Vector3(r.right, -r.top, 1) * tileSize;
                    Vector3 bl = new Vector3(r.left, -r.bottom, 1) * tileSize;
                    Vector3 br = new Vector3(r.right, -r.bottom, 1) * tileSize;

                    Gizmos.color = Color.magenta;

                    //Muestra conexiones logicas entre cuevas

                    foreach (int iB in r.linkedRooms)
                    {
                        Room roomB = mapInfo.rooms[iB];

                        Vector2 tlB = new Vector2(roomB.left, -roomB.top) * tileSize;
                        Vector2 trB = new Vector2(roomB.right, -roomB.top) * tileSize;
                        Vector2 blB = new Vector2(roomB.left, -roomB.bottom) * tileSize;
                        Vector2 brB = new Vector2(roomB.right, -roomB.bottom) * tileSize;

                        Vector2 pointA = (tl + tr + bl + br) / 4;
                        Vector2 pointB = (tlB + trB + blB + brB) / 4;

                        //Gizmos.DrawLine(pointA, pointB);
                    }

                    //PUNTOS DE COMIENZO DE PERFORACION DE HABITACIONES (Circulos grandes = habitacion donde comienza el jugador)

                    foreach (Coord c in r.startPoints)
                    {
                        Coord worldPos = r.GetWorldPosition();
                        Gizmos.DrawWireSphere(new Vector3(worldPos.x + c.x + .5f, -worldPos.y + c.y + .5f, 1) * tileSize, r.GetId() == 0 ? 25f : 8f);
                    }

                    //LIMITES DE HABITACIONES

                    Gizmos.color = Color.green;

                    Gizmos.DrawWireSphere(tl, 8f);
                    Gizmos.DrawWireSphere(tr, 8f);

                    Gizmos.color = Color.cyan;

                    Gizmos.DrawWireSphere(bl, 8f);
                    Gizmos.DrawWireSphere(br, 8f);

                    Gizmos.DrawLine(tl, tr);
                    Gizmos.DrawLine(tr, br);
                    Gizmos.DrawLine(br, bl);
                    Gizmos.DrawLine(bl, tl);
                }
            }
        }

        Gizmos.color = Color.red;

        //UNIONES ENTRE HABITACIONES

        if (mapInfo.corridors != null)
        {
            foreach (Corridor c in mapInfo.corridors)
            {
                Vector3 a = new Vector3(c.PointA.x + .5f, -c.PointA.y + .5f, 1) * tileSize;
                Vector3 b = new Vector3(c.PointB.x + .5f, -c.PointB.y + .5f, 1) * tileSize;

                Gizmos.DrawLine(a, b);
            }
        }

        Gizmos.color = Color.cyan;
    }

    public GameObject[] GetDrops()
    {
        return drops;
    }

    Vector3 CoordToVect(Coord c, int tileSize)
    {
        return new Vector3((c.x + .5f) * tileSize, (c.y + .5f) * tileSize, 1);
    }

    public static LevelGenerator GetInstance()
    {
        return instance;
    }
}
