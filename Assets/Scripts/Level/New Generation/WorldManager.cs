using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    System.Random random;

    TilemapGenerator tilemapGenerator;

    WorldGenerator generator;

    public int[] LevelSeeds { get; private set; }
    public int Seed;

    public readonly int Levels = 4;

    float loreSpawnChance = 0.4f;
    float extraLoopSpawnChance = 0.5f;

    public bool autoSeed = true;

    bool ready = false;

    void Start()
    {
        Init();
        CreateSeeds();
        GenerateLevelAsync(0);

        StartCoroutine(WaitForGeneration());

        Debug.Log("Seed: " + Seed);
    }

    void Init()
    {
        if (autoSeed) {
            Seed = new System.Random().Next();
        }

        random = new System.Random(Seed);

        tilemapGenerator = TilemapGenerator.GetInstance();
    }

    public void CreateSeeds()
    {
        LevelSeeds = new int[Levels];

        for (int i = 0; i < Levels; i++)
        {
            LevelSeeds[i] = random.Next();
        }
    }

    public async void GenerateLevelAsync(int level)
    {
        var result = await Task.Run(() =>
        {
            if (LevelSeeds != null)
            {
                WorldGenerationParameters parameters = CalculateParameters(level);

                generator = new WorldGenerator(parameters);
                generator.GenerateWorld();

                ready = true;

                Debug.Log(generator.ToString());
            }

            return 0;
        });
    }

    WorldGenerationParameters CalculateParameters(int level)
    {
        WorldGenerationParameters parameters = new WorldGenerationParameters();

        KeyRoomList keyRooms = CalculateKeyRooms(level);
        GraphInput graphInput = CalculateGraphParameters(level, keyRooms);

        float chanceOneWayLoop = .25f + ((float)level / Levels) * .4f;
        float chanceUnfairness = .1f + ((float)level / Levels) * .4f;

        parameters.Level = level;
        parameters.GraphParameters = graphInput;
        parameters.SpecialRoomList = keyRooms;
        parameters.ChanceOfOneWayLoop = chanceOneWayLoop;
        parameters.ChanceOfUnfairness = chanceUnfairness;
        parameters.MinimumBossDepthFactor = .5f;
        parameters.MinimumKeyRoomDepthFactor = .2f;

        return parameters;
    }

    GraphInput CalculateGraphParameters(int level, KeyRoomList keyRooms)
    {
        GraphInput graphParameters;

        int totalKeyRooms = 0;

        int rooms;
        int minLoopLength;
        int maxLoopLength;
        int leaves;
        int loops = 2;

        totalKeyRooms += keyRooms.Lore + keyRooms.Minibosses + keyRooms.Rewards + keyRooms.Shops;

        if (level > 2 && random.NextDouble() % 1 < extraLoopSpawnChance)
        {
            loops++;
        }

        rooms = level + random.Next(0, 3) + 15;
        leaves = totalKeyRooms + loops + level + 1;

        minLoopLength = 3 + level;
        maxLoopLength = 4 + level;

        graphParameters = new GraphInput(LevelSeeds[level], rooms, leaves, loops, minLoopLength, maxLoopLength);

        return graphParameters;
    }

    KeyRoomList CalculateKeyRooms(int level)
    {
        KeyRoomList keyRooms = new KeyRoomList();

        keyRooms.Rewards = 2;
        keyRooms.Shops = 1;
        keyRooms.Lore = 0;

        if (level % 2 != 0)
        {
            if (random.NextDouble() % 1 < loreSpawnChance)
            {
                keyRooms.Lore = 1;
            }
        }

        if (level <= 2)
        {
            keyRooms.Minibosses = 1;
        }
        else
        {
            keyRooms.Minibosses = 2;
        }

        return keyRooms;
    }

    IEnumerator WaitForGeneration()
    {

        while (!ready)
        {
            yield return null;
        }

        tilemapGenerator.LoadLevel(generator.roomComposites);
    }
}
