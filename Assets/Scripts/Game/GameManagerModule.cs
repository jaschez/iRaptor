using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManagerModule : MonoBehaviour
{
    static GameManagerModule instance;

    public ItemDataDictionary itemDictionary;

    public List<ItemData> AvailableItems { get; private set; }

    public EnemyData[] GameEnemies;
    public EnemyData[] Enemies { get; private set; }

    public EnemyRoomSettings[] enemyRoomSettings;

    public Dictionary<EnemyType, EnemyData> EnemyDictionary { get; private set; }
    public Dictionary<EnemyType, float>[] SpawnRateDictionaries { get; private set; }

    System.Random random;

    LevelManager levelManager;
    WorldManager worldManager;
    WorldGenerator generator;

    PlayerState storedPlayerState;

    public delegate void LoadingSceneFade();
    public static event LoadingSceneFade OnLoadingSceneFade;

    public int[] LevelSeeds { get; private set; }
    public int UniversalSeed;
    public int CurrentLevel { get; private set; }

    public int TotalLevels { get; private set; } = 4;

    public bool RandomSeed;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if(instance != this)
        {
            Destroy(this);
        }

        if (RandomSeed)
        {
            UniversalSeed = new System.Random().Next();
        }

        if (!SavingSystem.Loaded())
        {
            SavingSystem.Initialize();
        }

        random = new System.Random(UniversalSeed);

        CurrentLevel = 0;

        CreateSeeds();
        GenerateEnemyData();
    }

    void OnEnable()
    {
        //Start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;

    }

    void OnDisable()
    {
        //Stop listening for a scene change as soon as this script is disabled.
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;

    }

    void GenerateItems()
    {
        List<ItemID> unlockedIDs = SavingSystem.CurrentState.UnlockedItems;
        AvailableItems = itemDictionary.GetItemsFromIDs(unlockedIDs);
    }

    void GenerateEnemyData()
    {
        //Create enemy dictionary
        EnemyDictionary = new Dictionary<EnemyType, EnemyData>();

        foreach (EnemyData enemy in GameEnemies)
        {
            if (!EnemyDictionary.ContainsKey(enemy.Type))
            {
                EnemyDictionary.Add(enemy.Type, enemy);
            }
        }

        //Create enemy spawn rate dictionary
        SpawnRateDictionaries = new Dictionary<EnemyType, float>[enemyRoomSettings.Length];

        int levelIndex = 0;

        foreach (EnemyRoomSettings spawnRate in enemyRoomSettings)
        {
            SpawnRateDictionaries[levelIndex] = new Dictionary<EnemyType, float>();

            foreach (EnemyRoomSettings.SpawnRate enemyRate in spawnRate.Enemies)
            {
                if (!SpawnRateDictionaries[levelIndex].ContainsKey(enemyRate.Type))
                {
                    SpawnRateDictionaries[levelIndex].Add(enemyRate.Type, enemyRate.Rate);
                }
            }

            levelIndex++;
        }

        //Order enemies by difficulty
        Enemies = GameEnemies.OrderBy(x => x.DifficultyPoints).ToArray();
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Loading":
                OnLoadingSceneFade();
                Invoke("StartGeneration", 3f);
                //StartGeneration();
                break;

            case "Game":
                StartLevel();
                break;

            default:
                break;
        }
    }

    public string GetGameSeed()
    {
        return UniversalSeed.ToString();
    }

    async void StartGeneration()
    {
        worldManager = new WorldManager();

        GenerateItems();

        var generationTask = await worldManager.GenerateLevelAsync(CurrentLevel, LevelSeeds[CurrentLevel]);

        FinishLevelGeneration(generationTask);
    }

    void StartLevel()
    {
        StartCoroutine(InitializeLevelManager());
    }

    void FinishLevelGeneration(WorldGenerator output)
    {
        generator = output;

        //Do something

        //////////////
        
        SceneManager.LoadScene("Game");
    }

    public void FinishLevel(PlayerState state)
    {
        levelManager = null;
        storedPlayerState = state;
        CurrentLevel++;

        SceneManager.LoadScene("Loading");
    }

    void CreateSeeds()
    {
        LevelSeeds = new int[TotalLevels];

        for (int i = 0; i < TotalLevels; i++)
        {
            LevelSeeds[i] = random.Next();
        }
    }

    public static GameManagerModule GetInstance()
    {
        return instance;
    }

    IEnumerator InitializeLevelManager()
    {
        while (levelManager == null)
        {
            levelManager = LevelManager.GetInstance();
            yield return null;
        }

        levelManager.Initialize(generator);
    }
}
