using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    static LevelManager instance;

    System.Random random;

    [Space]
    public GameObject npcPrefab;
    public GameObject lightPrefab;
    public GameObject barrierPrefab;
    public GameObject exitPrefab;
    public GameObject summonPrefab;

    [Space]

    public DropObject[] DropObjects;
    public DropObject[] ChestObjects;

    public Dictionary<DropType, GameObject> Drops { get; private set; }
    public Dictionary<DropType, GameObject> Chests { get; private set; }

    GameManagerModule gameManager;

    WorldGenerator output;

    TilemapGenerator tilemapGenerator;

    CamManager camManager;

    UIVisualizer uiVisualizer;

    Movement movManager;

    PlayerModule player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Drops = new Dictionary<DropType, GameObject>();
        Chests = new Dictionary<DropType, GameObject>();

        foreach (DropObject drop in DropObjects)
        {
            if (!Drops.ContainsKey(drop.Type))
            {
                Drops.Add(drop.Type, drop.Object);
            }
        }

        foreach (DropObject drop in ChestObjects)
        {
            if (!Chests.ContainsKey(drop.Type))
            {
                Chests.Add(drop.Type, drop.Object);
            }
        }
    }

    private void OnEnable()
    {
        ExitHole.OnExitEnter += AdvanceLevel;
    }

    private void OnDisable()
    {
        ExitHole.OnExitEnter -= AdvanceLevel;
    }

    public void Initialize(WorldGenerator output)
    {
        this.output = output;

        gameManager = GameManagerModule.GetInstance();

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

        Debug.Log("Unsuccessful links: " + output.UnsuccessfulLinks);
        Debug.Log(output.ToString());

        Invoke("Present", .5f);
    }

    void Present()
    {
        uiVisualizer.PresentLevel(gameManager.CurrentLevel, GameManagerModule.LevelNames[gameManager.CurrentLevel]);
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

        playerCoord = entranceRoom.InterestingPoints[random.Next(0, entranceRoom.InterestingPoints.Count)];

        player.transform.position = CoordToVect(playerCoord, entranceRoom.Position);
    }

    void PlaceRoomObjects()
    {
        GameObject waveManObj;

        GameObject roomParent;
        GameObject lootParent;
        GameObject lightsParent;
        GameObject chestParent;
        GameObject enemyParent;

        GameObject roomsParent = new GameObject("Rooms");
        GameObject triggerParent = new GameObject("EntryRooms");

        //Se usará para crear las barreras de los pasadizos
        Dictionary<int, GameObject> barrierIndex = new Dictionary<int, GameObject>();


        //Sistema de oleadas
        waveManObj = new GameObject("WaveManager");
        WaveManager waveManager = waveManObj.AddComponent<WaveManager>();

        foreach(List<RoomNode> composite in output.RoomComposites) {
            foreach (RoomNode room in composite)
            {
                roomParent = new GameObject("Room" + room.ID);
                lightsParent = new GameObject("Light");
                lootParent = new GameObject("Loot");
                chestParent = new GameObject("Chests");
                enemyParent = new GameObject("Enemies");

                //Reward layer
                if (room.Type == RoomType.Reward)
                {
                    RewardRoom rewardRoom = (RewardRoom)room;

                    Vector3 rewardPos = CoordToVect(rewardRoom.Reward.Item2, room.Position);
                    ModuleChest chest = Instantiate(Chests[DropType.Item], rewardPos, Quaternion.identity).GetComponent<ModuleChest>();

                    chest.SetItem(rewardRoom.Reward.Item1);
                }
                else if(room.Type == RoomType.Shop)
                {
                    ShopRoom shopRoom = (ShopRoom)room;
                    Vector3 rewardPos = CoordToVect(shopRoom.NPC, room.Position);
                    Instantiate(npcPrefab, rewardPos, Quaternion.identity, lootParent.transform);

                    foreach (Tuple<DropType, Coord> item in shopRoom.Items)
                    {
                        Vector3 itemPos = CoordToVect(item.Item2, room.Position);
                        Chest chest = Instantiate(Chests[item.Item1], rewardPos, Quaternion.identity, lootParent.transform).GetComponent<Chest>();
                        chest.SetPrice(shopRoom.Prices[item.Item1]);
                    }
                }else if(room.Type == RoomType.Boss)
                {
                    Vector3 exitPos = CoordToVect(((BossRoom)room).Exit, room.Position);
                    Instantiate(exitPrefab, exitPos, Quaternion.identity);
                }

                InstantiateLights(room, lightsParent);

                enemyParent.transform.SetParent(roomParent.transform);
                chestParent.transform.SetParent(roomParent.transform);
                lightsParent.transform.SetParent(roomParent.transform);
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

        //Barrier layer
        foreach (int connectionID in output.Connections.Keys)
        {
            if (!barrierIndex.ContainsKey(connectionID))
            {
                EntryConnection connection = output.Connections[connectionID];

                float middleX = (connection.EntryA.x + connection.EntryB.x) / 2f;
                float middleY = (connection.EntryA.y + connection.EntryB.y) / 2f;

                Vector3 barrierPos = new Vector3((middleX + .5f) * 16, (middleY + .5f) * 16, 1);
                Quaternion barrierRotation = Quaternion.identity;

                if (connection.EntryA.x == 0 || connection.EntryA.x == connection.EntryB.x - 1 || connection.EntryA.x == connection.EntryB.x + 1)
                {
                    barrierRotation = Quaternion.Euler(0, 0, 90);
                }

                GameObject barrierObj = Instantiate(barrierPrefab, barrierPos, barrierRotation);

                barrierObj.name = "Barrier" + connectionID;
                barrierObj.SetActive(false);

                barrierIndex.Add(connectionID, barrierObj);
                waveManager.AddBarrierToRoom(connection.RoomAID, barrierObj);
                waveManager.AddBarrierToRoom(connection.RoomBID, barrierObj);
            }
        }
    }

    public void InstantiateObjectList(List<Coord> objList, Coord origin, GameObject instantiable, GameObject parentTo = null)
    {
        foreach (Coord objCoord in objList)
        {
            Vector3 position = CoordToVect(objCoord, origin);

            Instantiate(instantiable, position, Quaternion.identity, parentTo.transform);
        }
    }

    public void InstantiateEnemyList(List<Tuple<EnemyType, Coord>> enemyList, Coord origin, bool summoning = false, GameObject parentTo = null)
    {
        float summoningDelay = .3f;

        foreach (Tuple<EnemyType, Coord> enemy in enemyList)
        {
            Vector3 position = CoordToVect(enemy.Item2, origin);

            GameObject enemyObj = Instantiate(gameManager.EnemyDictionary[enemy.Item1].Prefab, position, Quaternion.identity, parentTo.transform);

            if (summoning)
            {
                Summoner summoner = Instantiate(summonPrefab, position, Quaternion.identity).GetComponent<Summoner>();
                summoner.Initialize(enemyObj, summoningDelay, .5f);
            }

            summoningDelay += .2f;
        }
    }

    public void InstantiateLights(RoomNode room, GameObject parentTo = null)
    {
        Vector3 position = new Vector3(
            (room.Position.x + room.Width / 2f) * 16f,
            (room.Position.y - room.Height / 2f) * 16f, 1);

        GameObject lightObj = Instantiate(lightPrefab, position, Quaternion.identity, parentTo.transform);

        lightObj.transform.localScale = new Vector3(room.Width * 1.25f, room.Height * 1.25f);
    }

    void AdvanceLevel()
    {
        //Realizar transición y avanzar nivel

        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Movement>().enabled = false;
        player.GetComponent<AttackModule>().enabled = false;
        player.enabled = false;

        camManager.Zoom(10, 5);

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
        return new Vector3((relativeTo.x + (c.x + .5f)) * tileSize, (relativeTo.y - (c.y + .5f)) * tileSize, 1);
    }

    public static LevelManager GetInstance()
    {
        return instance;
    }

    [Serializable]
    public struct DropObject{
        public GameObject Object;
        public DropType Type;
    }

    [Serializable]
    public struct ItemObject
    {
        public ItemID ID;
        public ItemData Data;
    }
}
