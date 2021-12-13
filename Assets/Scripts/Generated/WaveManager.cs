using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    static WaveManager instance;

    Room currentRoom;

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
    }

    void Start()
    {
        
    }

    public void UpdateRoom(Room room)
    {
        if (room.ID != currentRoom.ID) {
            currentRoom = room;
            totalWaves = room.Enemies.Count;
            beatenEnemies = 0;

            if (room.Type == Room.RoomType.Normal) {
                StartNextWave();
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

            }
            else
            {
                StartNextWave();
            }
        }
    }

    void StartNextWave()
    {
        totalWaveEnemies = currentRoom.Enemies[currentWave].Count;
        SpawnEnemies();
    }

    void SpawnEnemies()
    {

    }

    void FinishWave()
    {
        //currentRoom.
    }

    public static WaveManager GetInstance()
    {
        return instance;
    }
}
