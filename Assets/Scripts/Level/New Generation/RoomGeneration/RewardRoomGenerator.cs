using System;

public class RewardRoomGenerator : RoomGeneration
{
    public Tuple<RewardType, Coord> Reward { get; private set; }

    public RewardRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        Initialize(RoomType.Reward, random.Next(150, 200), 3, .7f);
    }

    protected override void GenerateMap()
    {
        AddEntry(new Coord(Width / 2, Height / 2));
        DefaultMapGeneration();
    }

    protected override void AdditionalGeneration()
    {
        GenerateReward();
    }

    void GenerateReward()
    {
        Reward = new Tuple<RewardType, Coord>(RewardType.Buff, new Coord(Width / 2, Height / 2));
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Floor_Reward);
    }
}

