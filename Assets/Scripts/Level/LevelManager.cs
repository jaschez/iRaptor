using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    static LevelManager instance;

    LevelGenerator generator;

    CamManager camManager;

    MapInfo mapInfo;

    int levelIndex;
    int width, height;
    int fillPercentage;
    int smoothness;
    int maxEnemyNumber;
    int maxLootNumber;

    int seed;

    int eggNumber;
    int beatenEnemies = 0;

    bool connectCaves;

    public delegate void LevelCleared();
    public delegate void LeftLevel();

    public static event LevelCleared OnLevelClear;
    public static event LeftLevel OnLevelLeft;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        ExitHole.OnExitEnter += ExitLevel;
    }

    private void OnDisable()
    {
        ExitHole.OnExitEnter -= ExitLevel;
    }

    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(int levelIndex, int width, int height, int fillPercentage, int smoothness, int seed, int maxEnemyNumber, int maxLootNumber, bool connectCaves)
    {
        this.levelIndex = levelIndex;
        this.width = width;
        this.height = height;
        this.fillPercentage = fillPercentage;
        this.smoothness = smoothness;
        this.seed = seed;
        this.maxEnemyNumber = maxEnemyNumber;
        this.maxLootNumber = maxLootNumber;
        this.connectCaves = connectCaves;

        generator = GetComponent<LevelGenerator>();

        camManager = CamManager.GetInstance();
    }

    public void Generate()
    {
        mapInfo = generator.GenerateLevel(width, height, fillPercentage, smoothness, seed, connectCaves, maxEnemyNumber, maxLootNumber);

        eggNumber = 3;//mapInfo.InitialEggs;
    }

    public void AddBeatenEgg()
    {
        beatenEnemies++;
        CheckAllBeatenEggs();
    }

    void CheckAllBeatenEggs()
    {
        if (beatenEnemies >= eggNumber)
        {
            //Stage Clear
            UIVisualizer.GetInstance().PopUpImportantMessage("STAGE CLEAR.\nADVANCE TO THE NEXT AREA.");
            //UIVisualizer.GetInstance().PopUp(PopUpType.Info, "Stage Clear!", PlayerModule.GetInstance().transform, 1.5f, 20);
            Debug.Log("Stage Clear!");

            camManager.ShakeQuake(10, 2.5f, false);
            camManager.Flash();

            FinishLevel();
        }
    }

    void FinishLevel()
    {
        //Invocar cofre de recompensa y mostrar salida
        OnLevelClear?.Invoke();
    }

    void ExitLevel()
    {
        OnLevelLeft?.Invoke();
    }

    public MapInfo GetMapInfo()
    {
        return mapInfo;
    }

    public static LevelManager GetInstance()
    {
        return instance;
    }
}
