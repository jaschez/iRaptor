using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerModule : MonoBehaviour
{

    static GameManagerModule instance;

    public AssetManager assetContainer;

    LevelManager levelManager;

    GameObject levelManagerObj;
    GameObject cursor;

    UIVisualizer uiVisualizer;

    CamManager camManager;

    Movement movManager;

    PlayerModule player;

    Camera camComponent;

    SceneState sceneState;

    PlayerState playerState;

    Vector3 cursorPosition;

    int[] levelSeeds;

    public int maxLevels = 4;

    public int currentLevel;
    public int width, height;
    public int fillPercentage;
    public int smoothness;
    public int enemyNumber;
    public int lootNumber;

    public int seed;

    int levelSeed;

    public bool randomSeed, connectCaves;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        sceneState = SceneState.Game;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        int randSeed = Random.Range(int.MinValue, int.MaxValue);

        if (randomSeed)
        {
            seed = randSeed;
            Debug.Log("Seed: " + seed);
        }

        Random.InitState(seed);

        levelSeeds = new int[maxLevels];

        for (int i = 0; i < maxLevels; i++)
        {
            levelSeeds[i] = Random.Range(int.MinValue, int.MaxValue);
        }
    }

    private void OnEnable()
    {
        LevelManager.OnLevelLeft += AdvanceLevel;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLeft -= AdvanceLevel;
    }

    void InitNextLevel()
    {

        levelSeed = Random.Range(int.MinValue, int.MaxValue);

        player = (PlayerModule)PlayerModule.GetInstance();
        camManager = CamManager.GetInstance();
        movManager = Movement.GetInstance();
        uiVisualizer = UIVisualizer.GetInstance();

        cursor = GameObject.FindGameObjectWithTag("cursor");
        levelManagerObj = GameObject.FindGameObjectWithTag("lvlmanager");
        camComponent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        levelManager = levelManagerObj.AddComponent<LevelManager>();
        levelManager.Initialize(currentLevel, width, height, fillPercentage, smoothness, levelSeed, enemyNumber, lootNumber, connectCaves);
        levelManager.Generate();

        movManager.SetStartMode();

        if (currentLevel != 0)
        {
            LoadPlayerState();
        }

        camManager.SetCamPos(movManager.gameObject.transform.position);

        uiVisualizer.InitUI();
    }

    void Update()
    {

        if (cursor && camComponent)
        {
            cursorPosition = camComponent.ScreenToWorldPoint(Input.mousePosition);
            cursorPosition.z = 1;
            cursor.transform.position = cursorPosition;
        }
        else
        {
            cursor = GameObject.FindGameObjectWithTag("cursor");
            camComponent = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        if (levelManagerObj == null && sceneState == SceneState.Game)
        {
            InitNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SavePlayerState();
            //currentLevel = -1;
            GenerateNewLevel();
        }
    }

    void AdvanceLevel()
    {
        //Realizar transición y avanzar nivel
        currentLevel++;

        SavePlayerState();

        cursor.SetActive(false);
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Movement>().enabled = false;
        player.GetComponent<AttackModule>().enabled = false;
        player.enabled = false;

        CamManager.GetInstance().Zoom(10, 5);

        uiVisualizer.TransitionScene();

        Invoke("LoadUpgradeScene", 1f);
    }

    void GenerateNewLevel()
    {
        sceneState = SceneState.Game;
        SceneManager.LoadScene("Game");
    }

    void LoadUpgradeScene()
    {
        //sceneState = SceneState.Upgrade;
        SceneManager.LoadScene(1);
    }

    public static GameManagerModule GetInstance()
    {
        return instance;
    }

    public void SavePlayerState()
    {
        playerState = player.SavePlayerState();
    }

    public void LoadPlayerState()
    {
        player.LoadPlayerState(playerState);
    }

    enum SceneState
    {
        Game,
        Upgrade
    }
}
