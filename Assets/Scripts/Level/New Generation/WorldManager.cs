using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager
{
    System.Random random;

    public int[] LevelSeeds;

    public readonly int Levels = 4;
    public readonly int Seed;

    float loreSpawnChance = 0.4f;
    float extraLoopSpawnChance = 0.5f;

    public WorldManager()
    {
        Seed = new System.Random().Next();
        random = new System.Random(Seed);

        LevelSeeds = new int[Levels];
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

        foreach (var property in keyRooms.GetType().GetProperties())
        {
            totalKeyRooms += (int)property.GetValue(keyRooms, null);
        }

        if (level > 2 && random.NextDouble() % 1 < extraLoopSpawnChance)
        {
            loops++;
        }

        rooms = level + random.Next(0, 3) + 15;
        leaves = totalKeyRooms +  loops + level;

        minLoopLength = 2 + level;
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
}
