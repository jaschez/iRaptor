public class EntranceRoomGenerator : RoomGeneration
{
    public EntranceRoomGenerator(RoomNode room, int seed, int level) : base(room, seed, level)
    {
        Initialize(RoomType.Entrance, random.Next(3, 5) * 3, random.Next(3, 5) * 3, 3, .4f);
    }

    protected override void GenerateMap()
    {
        DefaultMapGeneration();
    }

    protected override void GenerateTileMap()
    {
        DefaultTilemapGeneration(TileSkin.Floor_Rock, TileSkin.Wall_Rock);
    }
}
