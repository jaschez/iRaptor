using System;

public class RewardRoom : RoomNode
{
    public Tuple<RewardType, Coord> Reward { get; private set; }

    public RewardRoom(RoomNode room) : base(room)
    {

    }

    public override void Generate(RoomGeneration generator)
    {
        base.Generate(generator);
        Reward = ((RewardRoomGenerator)generator).Reward;
    }
}

public enum RewardType
{
    Weapon,
    Buff,
    Object
}
