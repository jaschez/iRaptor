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
    public GameObject npc;
    public GameObject enemyPrefab;
    public GameObject lootPrefab;
    public GameObject chestPrefab;
    public GameObject barrierPrefab;
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

        npc.transform.position = CoordToVect(mapInfo.PlayerCoord, mapInfo.tileSize);

        LoadLevelFromMap(mapInfo);

        return mapInfo;
    }

    void LoadLevelFromMap(MapInfo mapInfo)
    {
        Tile wall = Resources.Load("wall2", typeof(Tile)) as Tile;
        Tile floor = Resources.Load("floor", typeof(Tile)) as Tile;
        /*Tile floor2 = Resources.Load("floor2", typeof(Tile)) as Tile;
        Tile floor3 = Resources.Load("floor3", typeof(Tile)) as Tile;*/

        GameObject waveManObj;

        GameObject roomParent;
        GameObject lootParent;
        GameObject chestParent;
        GameObject enemyParent;

        GameObject roomsParent = new GameObject("Rooms");
        GameObject triggerParent = new GameObject("EntryRooms");

        //Se usará para crear las barreras de los pasadizos
        List<Corridor> remainingCorridors = new List<Corridor>(mapInfo.corridors);
        Dictionary<int, GameObject> barrierIndex = new Dictionary<int, GameObject>();

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


        //Sistema de oleadas
        waveManObj = new GameObject("WaveManager");
        WaveManager waveManager = waveManObj.AddComponent<WaveManager>();

        foreach (Room room in mapInfo.rooms)
        {
            roomParent = new GameObject("Room" + room.ID);
            lootParent = new GameObject("Loot");
            chestParent = new GameObject("Chests");
            enemyParent = new GameObject("Enemies");

            //Capa de enemigos
            //InstantiateObjectList(room.enemyCoords, room.GetWorldPosition(), enemyPrefab, enemyParent);

            //Capa de cofres
            InstantiateObjectList(room.Chest, room.Position, chestPrefab, chestParent);

            //Capa de recompensas
            InstantiateObjectList(room.Loot, room.Position, lootPrefab, lootParent);

            enemyParent.transform.SetParent(roomParent.transform);
            chestParent.transform.SetParent(roomParent.transform);
            lootParent.transform.SetParent(roomParent.transform);
            roomParent.transform.SetParent(roomsParent.transform);

            //Capa de triggers de entrada a las habitaciones
            foreach (Room.Entry roomEntry in room.Entries)
            {
                if (roomEntry.type != Room.EntryType.Central)
                {
                    GameObject trigger = new GameObject("EntryRoom");

                    trigger.layer = LayerMask.NameToLayer("Entry");

                    BoxCollider2D col;

                    Coord worldPos = mapInfo.rooms[0].Position;
                    Coord entry = roomEntry.coord;

                    col = trigger.AddComponent<BoxCollider2D>();
                    trigger.AddComponent<EntryTrigger>().Initialize(room, roomParent);
                    trigger.transform.position = new Vector3(worldPos.x + entry.x + .5f, -worldPos.y - entry.y + .5f, 1) * mapInfo.tileSize;

                    if (roomEntry.type == Room.EntryType.Vertical)
                    {
                        col.size = Vector2.one + Vector2.up * 6;
                    }
                    else
                    {
                        col.size = Vector2.one + Vector2.right * 6;
                    }

                    col.size *= mapInfo.tileSize;
                    col.isTrigger = true;

                    trigger.transform.SetParent(triggerParent.transform);

                    Corridor corridor = mapInfo.corridors[roomEntry.LinkedCorridor];

                    if (remainingCorridors.Contains(corridor))
                    {
                        int middleX = (corridor.PointA.x + corridor.PointB.x) / 2;
                        int middleY = -(corridor.PointA.y + corridor.PointB.y) / 2;

                        Vector3 barrierPos = CoordToVect(new Coord(middleX, middleY), mapInfo.tileSize);
                        Quaternion barrierRotation = Quaternion.identity;

                        if (roomEntry.type == Room.EntryType.Vertical)
                        {
                            barrierRotation = Quaternion.Euler(0, 0, 90);
                        }

                        GameObject barrierObj = Instantiate(barrierPrefab, barrierPos, barrierRotation);

                        barrierObj.name = "Barrier"+ roomEntry.LinkedCorridor.ToString();

                        barrierIndex.Add(roomEntry.LinkedCorridor, barrierObj);
                        waveManager.AddBarrierToRoom(room.ID, barrierObj);

                        remainingCorridors.Remove(corridor);
                    }
                    else
                    {
                        waveManager.AddBarrierToRoom(room.ID, barrierIndex[roomEntry.LinkedCorridor]);
                    }
                }
            }
        }

        Instantiate(exitPrefab, new Vector3(mapInfo.ExitPos.x + 0.5f, mapInfo.ExitPos.y + 0.5f, 1) * 16, Quaternion.identity);
    }

    public void InstantiateObjectList(List<Coord> objList, Coord origin, GameObject instantiable, GameObject parentTo = null)
    {
        foreach (Coord objCoord in objList)
        {
            Coord c = objCoord;

            c.x += origin.x;
            c.y -= origin.y;

            Instantiate(instantiable, CoordToVect(c, mapInfo.tileSize), Quaternion.identity, parentTo.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        int tileSize = mapInfo.tileSize;

        if (mapInfo.rooms != null)
        {
            foreach (RoomGenerator r in mapInfo.debugRooms)
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
                        RoomGenerator roomB = mapInfo.debugRooms[iB];

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
