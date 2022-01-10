using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    static WaveManager instance;

    GameObject enemy;

    Room currentRoom;

    List<int> beatenRooms;
    List<GameObject> brokenRoomBarriers;

    Dictionary<int, List<GameObject>> roomBarriers; 

    int totalWaveEnemies;
    int totalWaves;
    int beatenEnemies;
    int currentWave;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        beatenRooms = new List<int>();
        brokenRoomBarriers = new List<GameObject>();
        roomBarriers = new Dictionary<int, List<GameObject>>();
    }

    void Start()
    {
        enemy = LevelGenerator.GetInstance().enemyPrefab;
    }

    public void UpdateRoom(Room room)
    {
        if (room.ID != currentRoom.ID) {
            currentRoom = room;

            if (room.Type == Room.RoomType.Normal) {
                if (!beatenRooms.Contains(currentRoom.ID)) {
                    totalWaves = room.Enemies.Count;
                    beatenEnemies = 0;
                    currentWave = 0;
                    
                    BlockDoors(true);
                    StartNextWave();
                }
            }
        }
    }

    public void AddBeatenEnemy()
    {
        beatenEnemies++;
        if (beatenEnemies == totalWaveEnemies)
        {
            currentWave++;

            if (currentWave == totalWaves) {
                FinishWave();
            }
            else
            {
                StartNextWave();
            }
        }
    }

    public void AddBarrierToRoom(int roomID, GameObject barrier)
    {
        if (!roomBarriers.ContainsKey(roomID))
        {
            roomBarriers.Add(roomID, new List<GameObject>());
        }

        roomBarriers[roomID].Add(barrier);
    }

    void StartNextWave()
    {
        totalWaveEnemies = currentRoom.Enemies[currentWave].Count;
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        List<Coord> enemies = currentRoom.Enemies[currentWave];
        LevelGenerator.GetInstance().InstantiateObjectList(enemies, currentRoom.Position, enemy, new GameObject("enemy"));
    }

    void FinishWave()
    {
        //We add the room id to a registry, so we find its value when we check if the room is beaten or not
        //(We also open the doors after they get closed)
        BlockDoors(false);
        beatenRooms.Add(currentRoom.ID);
    }

    void BlockDoors(bool blocked)
    {
        if (blocked)
        {
            brokenRoomBarriers.Clear();
        }

        foreach (GameObject barrierObj in roomBarriers[currentRoom.ID])
        {
            Barrier barrier = barrierObj.GetComponent<Barrier>();

            if (blocked)
            {
                if (!barrierObj.activeSelf)
                {
                    brokenRoomBarriers.Add(barrierObj);
                    barrierObj.SetActive(true);
                }
            }
            else
            {
                if (brokenRoomBarriers.Contains(barrierObj)) {
                    barrierObj.SetActive(false);
                }
            }

            barrier.Block(blocked);
        }
    }

    public static WaveManager GetInstance()
    {
        return instance;
    }
}
