using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    static LevelManager instance;

    System.Random random;

    WorldGenerator output;

    TilemapGenerator tilemapGenerator;

    CamManager camManager;

    UIVisualizer uiVisualizer;

    Movement movManager;

    PlayerModule player;

    public GameObject enemyPrefab;
    public GameObject lootPrefab;
    public GameObject chestPrefab;
    public GameObject barrierPrefab;
    public GameObject exitPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Initialize(WorldGenerator output)
    {
        this.output = output;

        player = (PlayerModule)PlayerModule.GetInstance();
        camManager = CamManager.GetInstance();
        movManager = Movement.GetInstance();
        uiVisualizer = UIVisualizer.GetInstance();
        tilemapGenerator = TilemapGenerator.GetInstance();

        random = new System.Random(new System.Random(output.GraphInfo.GraphInfo.Seed).Next());

        //LoadPlayerState(playerState);
        Generate();

        movManager.SetStartMode();
        camManager.SetCamPos(movManager.gameObject.transform.position);
        uiVisualizer.InitUI();
    }

    void Generate()
    {
        tilemapGenerator.LoadLevel(output.RoomComposites);

        PlacePlayer();
        PlaceRoomObjects();
    }

    void PlacePlayer()
    {
        RoomNode entranceRoom = null;
        Coord playerCoord;

        foreach (List<RoomNode> composite in output.RoomComposites)
        {
            foreach (RoomNode room in composite)
            {
                if (room.Type == RoomType.Entrance)
                {
                    entranceRoom = room;
                    break;
                }
            }
        }

        playerCoord = entranceRoom.Floor[random.Next(0, entranceRoom.Floor.Count)];

        player.transform.position = CoordToVect(playerCoord, entranceRoom.Position);
    }

    void PlaceRoomObjects()
    {
        GameObject waveManObj;

        GameObject roomParent;
        GameObject lootParent;
        GameObject chestParent;
        GameObject enemyParent;

        GameObject roomsParent = new GameObject("Rooms");
        GameObject triggerParent = new GameObject("EntryRooms");

        //Se usará para crear las barreras de los pasadizos
        /*List<Corridor> remainingCorridors = new List<Corridor>(mapInfo.corridors);
        Dictionary<int, GameObject> barrierIndex = new Dictionary<int, GameObject>();*/


        //Sistema de oleadas
        waveManObj = new GameObject("WaveManager");
        WaveManager waveManager = waveManObj.AddComponent<WaveManager>();

        foreach(List<RoomNode> composite in output.RoomComposites) {
            foreach (RoomNode room in composite)
            {
                roomParent = new GameObject("Room" + room.ID);
                lootParent = new GameObject("Loot");
                chestParent = new GameObject("Chests");
                enemyParent = new GameObject("Enemies");

                //Capa de enemigos
                //InstantiateObjectList(room.enemyCoords, room.GetWorldPosition(), enemyPrefab, enemyParent);

                //Capa de cofres
                //InstantiateObjectList(room.Chest, room.Position, chestPrefab, chestParent);

                //Capa de recompensas
                //InstantiateObjectList(room.Loot, room.Position, lootPrefab, lootParent);

                enemyParent.transform.SetParent(roomParent.transform);
                chestParent.transform.SetParent(roomParent.transform);
                lootParent.transform.SetParent(roomParent.transform);
                roomParent.transform.SetParent(roomsParent.transform);

                //Capa de triggers de entrada a las habitaciones
                if (room.Type != RoomType.Entrance)
                {
                    foreach (Coord entry in room.Entries)
                    {
                        GameObject trigger = new GameObject("EntryRoom");
                        BoxCollider2D col;
                        Coord worldPos = room.Position;

                        trigger.layer = LayerMask.NameToLayer("Entry");

                        col = trigger.AddComponent<BoxCollider2D>();
                        trigger.AddComponent<EntryTrigger>().Initialize(room, roomParent);
                        trigger.transform.position = new Vector3(worldPos.x + entry.x + .5f, worldPos.y - entry.y + .5f, 1) * 16;

                        if (entry.x == 0 || entry.x == room.Width - 1)
                        {
                            col.size = Vector2.one + Vector2.up * 6;
                        }
                        else if (entry.y == 0 || entry.y == room.Height - 1)
                        {
                            col.size = Vector2.one + Vector2.right * 6;
                        }

                        col.size *= 16;
                        col.isTrigger = true;

                        trigger.transform.SetParent(triggerParent.transform);
                    }
                }
            }
        }

        //Instantiate(exitPrefab, new Vector3(mapInfo.ExitPos.x + 0.5f, mapInfo.ExitPos.y + 0.5f, 1) * 16, Quaternion.identity);
    }

    public void InstantiateObjectList(List<Coord> objList, Coord origin, GameObject instantiable, GameObject parentTo = null)
    {
        foreach (Coord objCoord in objList)
        {
            Vector3 position = CoordToVect(objCoord, origin);

            Instantiate(instantiable, position, Quaternion.identity, parentTo.transform);
        }
    }

    void AdvanceLevel()
    {
        //Realizar transición y avanzar nivel

        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Movement>().enabled = false;
        player.GetComponent<AttackModule>().enabled = false;
        player.enabled = false;

        camManager.Zoom(10, 5);

        uiVisualizer.TransitionScene();

        Invoke("FinishLevel", 1f);
    }

    void FinishLevel()
    {
        PlayerState playerState = SavePlayerState();
        GameManagerModule.GetInstance().FinishLevel(playerState);
    }

    void ClearLevel()
    {
        //Stage Clear
        UIVisualizer.GetInstance().PopUpImportantMessage("STAGE CLEAR.\nADVANCE TO THE NEXT AREA.");
        //UIVisualizer.GetInstance().PopUp(PopUpType.Info, "Stage Clear!", PlayerModule.GetInstance().transform, 1.5f, 20);
        Debug.Log("Stage Clear!");

        camManager.ShakeQuake(10, 2.5f, false);
        camManager.Flash();
    }

    PlayerState SavePlayerState()
    {
        return player.SavePlayerState();
    }

    void LoadPlayerState(PlayerState state)
    {
        player.LoadPlayerState(state);
    }

    public WorldGenerator GetGeneratorInfo()
    {
        return output;
    }

    Vector3 CoordToVect(Coord c, Coord relativeTo, int tileSize = 16)
    {
        return new Vector3(relativeTo.x + (c.x + .5f) * tileSize, relativeTo.y - (c.y + .5f) * tileSize, 1);
    }

    public static LevelManager GetInstance()
    {
        return instance;
    }
}
