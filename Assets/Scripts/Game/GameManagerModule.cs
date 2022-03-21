using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManagerModule : MonoBehaviour
{
    static GameManagerModule instance;

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

    public int TotalLevels { get; private set; } = 3;

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

        random = new System.Random(UniversalSeed);

        CurrentLevel = 0;

        CreateSeeds();
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

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Loading":
                OnLoadingSceneFade();
                Invoke("StartGeneration", 3f);
                break;

            case "Game":
                StartLevel();
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
