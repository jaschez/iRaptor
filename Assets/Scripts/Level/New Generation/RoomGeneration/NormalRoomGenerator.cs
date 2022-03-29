using System;
using System.Collections.Generic;

public class NormalRoomGenerator : RoomGeneration
{
    int roomSize;

    int roomWidth;
    int roomHeight;

    int minRoomSide;

    int scaleFactor = 3;

    int totalWaves;
    int difficultyPoints;
    int difficultyVariety;
    int maxEnemyIndex;

    EnemyData[] enemies;
    EnemyRoomSettings roomSettings;

    Dictionary<EnemyType, float> enemyRates;

    GameManagerModule gameManager;

    public NormalRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        gameManager = GameManagerModule.GetInstance();

        enemies = gameManager.EnemiesOrderedByDifficulty;
        roomSettings = gameManager.enemyRoomSettings[level];
        enemyRates = gameManager.SpawnRateDictionaries[level];

        //Dimensions calculations
        roomSize = random.Next(roomSettings.MinimumRoomSize, roomSettings.MaximumRoomSize + 1);

        //Calculate the minimum length of a room side
        minRoomSide = (int)Math.Sqrt(roomSize);
        minRoomSide -= (int)(minRoomSide * .2f);

        //Adjust width and height to chosen room size
        roomWidth = random.Next(minRoomSide, roomSize / minRoomSide);
        roomHeight = roomSize / roomWidth;

        //Approximate width and height to multiple of scaleFactor
        roomWidth = (roomWidth / scaleFactor) * scaleFactor;
        roomHeight = (roomHeight / scaleFactor) * scaleFactor;

        totalWaves = random.Next(1, 3);
        difficultyPoints = roomSettings.MinimumDifficultyPoints;
        difficultyVariety = roomSettings.DifficultyVariety;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].Type == roomSettings.MaxEnemyDifficulty)
            {
                maxEnemyIndex = i;
                break;
            }
        }

        Initialize(RoomType.Normal, roomWidth, roomHeight, scaleFactor, .5f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Rock);
    }

    protected override void AdditionalGeneration()
    {
        GenerateEnemies();
    }

    void GenerateEnemies()
    {
        for (int currentWave = 0; currentWave < totalWaves; currentWave++)
        {
            List<Tuple<EnemyType, Coord>> enemyWave = new List<Tuple<EnemyType, Coord>>();

            int maximumPoints = difficultyPoints + random.Next(0, difficultyVariety);
            int totalPoints = 0;

            while (totalPoints < maximumPoints)
            {
                int maxProbability = 0;

                for (int i = 0; i <= maxEnemyIndex; i++)
                {
                    EnemyData enemy = enemies[i];
                    maxProbability += (int)(enemyRates[enemy.Type] * 100);
                }

                int chosenEnemyIndex = -1;
                int choice = random.Next(0, maxProbability + 1);
                int currentProbability = 0;

                bool elementFound = false;

                for (int i = 0; i <= maxEnemyIndex && !elementFound; i++)
                {
                    currentProbability += (int)(enemyRates[enemies[i].Type] * 100);
                    
                    if (choice <= currentProbability)
                    {
                        chosenEnemyIndex = i;
                        elementFound = true;
                    }
                }

                Tuple<EnemyType, Coord> currentEnemy;

                EnemyType chosenEnemy = enemies[chosenEnemyIndex].Type;
                Coord enemyCoord;

                //Calculate position (would depend on the floor density? type of enemy?)
                enemyCoord = FloorCoords[random.Next(0, FloorCoords.Count)];

                currentEnemy = new Tuple<EnemyType, Coord>(chosenEnemy, enemyCoord);

                totalPoints += gameManager.EnemyDictionary[chosenEnemy].DifficultyPoints;

                enemyWave.Add(currentEnemy);
            }

            AssociatedRoom.AddEnemyWave(enemyWave);
        }
    }
}
