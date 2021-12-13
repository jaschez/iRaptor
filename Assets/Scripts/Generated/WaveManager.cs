using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    static WaveManager instance;

    Room currentRoom;

    int totalEnemies;
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
        if (room != currentRoom) {
            currentRoom = room;
            beatenEnemies = 0;
            totalEnemies = room.enemyCoords.Count;

            if (room.enemiesRemaining) {
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
