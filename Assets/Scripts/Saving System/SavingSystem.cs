public static class SavingSystem
{
    public static GameState CurrentState { get; private set; } = null;

    private readonly static string DATAFILE = "slot.sav";

    public static void Initialize()
    {
        Load();
    }

    public static void NewSave()
    {
        CurrentState = new GameState();

        CurrentState.AddStockItem(ItemID.Drill);
        CurrentState.AddStockItem(ItemID.Molotovic);

        CurrentState.AddOrbs(10);

        Save();
    }

    public static bool Loaded()
    {
        return CurrentState != null;
    }

    public static void Load()
    {
        CurrentState = Serializator<GameState>.Load(DATAFILE);
    }

    public static void Save()
    {
        Serializator<GameState>.Save(CurrentState, DATAFILE);
    }
}
