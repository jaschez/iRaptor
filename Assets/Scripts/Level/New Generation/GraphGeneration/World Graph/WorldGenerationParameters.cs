using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WorldGenerationParameters
{
    public int Level;

    public GraphInput GraphParameters;
    public KeyRoomList SpecialRoomList;

    [Range(0, 1)]
    public float MinimumBossDepthFactor;// = .5f;
    [Range(0, 1)]
    public float MinimumKeyRoomDepthFactor;// = .2f;
    [Range(0, 1)]
    public float ChanceOfOneWayLoop;// = .5f;
    [Range(0, 1)]
    public float ChanceOfUnfairness;// = .2f;
}

[System.Serializable]
public struct KeyRoomList
{
    public int Rewards;
    public int Shops;
    public int Lore;
    public int Minibosses;
}
