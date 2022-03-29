using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    static WaveManager instance;

    RoomNode currentRoom;

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
        
    }

    public void UpdateRoom(RoomNode room)
    {
        if (currentRoom == null)
        {
            currentRoom = room;
        }

        if (room.ID != currentRoom.ID) {
            currentRoom = room;

            if (room.Type == RoomType.Normal) {
                //Activate room
                if (!beatenRooms.Contains(currentRoom.ID)) {
                    totalWaves = room.Enemies.Count;
                    beatenEnemies = 0;
                    currentWave = 0;

                    if (totalWaves > 0) {
                        BlockDoors(true);
                        StartNextWave();
                    }
                    else
                    {
                        FinishWave();
                    }
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
        beatenEnemies = 0;

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        List<Tuple<EnemyType, Coord>> enemies = currentRoom.Enemies[currentWave];
        LevelManager.GetInstance().InstantiateEnemyList(enemies, currentRoom.Position, new GameObject("enemy"));
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

        if (roomBarriers.ContainsKey(currentRoom.ID)) {
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
    }

    public static WaveManager GetInstance()
    {
        return instance;
    }
}
