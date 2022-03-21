using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WorldManager
{
    System.Random random;

    WorldGenerator generator;

    public delegate void GenerationReady(WorldGenerator output);
    public static event GenerationReady OnGenerationReady;

    public int[] LevelSeeds { get; private set; }
    public int Seed;

    [Range(0,3)]
    public int CurrentLevel = 0;

    public readonly int Levels = 4;

    float loreSpawnChance = 0.4f;
    float extraLoopSpawnChance = 0.5f;

    bool ready = false;
    bool debug = false;

    public async Task<WorldGenerator> GenerateLevelAsync(int level, int seed)
    {
        random = new System.Random(seed);

        await Task.Run(() =>
        {
            WorldGenerationParameters parameters = CalculateParameters(level, seed);

            generator = new WorldGenerator(parameters);
            generator.GenerateWorld(debug);
        });

        return generator;
    }

    WorldGenerationParameters CalculateParameters(int level, int seed)
    {
        WorldGenerationParameters parameters = new WorldGenerationParameters();

        KeyRoomList keyRooms = CalculateKeyRooms(level);
        GraphInput graphInput = CalculateGraphParameters(level, seed, keyRooms);

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

    GraphInput CalculateGraphParameters(int level, int seed, KeyRoomList keyRooms)
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

        graphParameters = new GraphInput(seed, rooms, leaves, loops, minLoopLength, maxLoopLength);

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

        OnGenerationReady?.Invoke(generator);
    }
}
