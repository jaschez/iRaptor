using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    static LevelManager instance;

    System.Random random;

    WorldGenerator output;

    TilemapGenerator tilemapGenerator;

    CamManager camManager;

    UIVisualizer uiVisualizer;

    Movement movManager;

    PlayerModule player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Initialize(WorldGenerator output)
    {
        this.output = output;

        player = (PlayerModule)PlayerModule.GetInstance();
        camManager = CamManager.GetInstance();
        movManager = Movement.GetInstance();
        uiVisualizer = UIVisualizer.GetInstance();
        tilemapGenerator = TilemapGenerator.GetInstance();

        random = new System.Random(new System.Random(output.GraphInfo.GraphInfo.Seed).Next());

        //LoadPlayerState(playerState);
        Generate(output.RoomComposites);

        movManager.SetStartMode();
        camManager.SetCamPos(movManager.gameObject.transform.position);
        uiVisualizer.InitUI();
    }

    void Generate(List<List<RoomNode>> rooms)
    {
        tilemapGenerator.LoadLevel(rooms);

        //Place Player
        RoomNode entranceRoom = null;
        Coord playerCoord;

        foreach (List<RoomNode> composite in rooms)
        {
            foreach (RoomNode room in composite)
            {
                if (room.Type == RoomType.Entrance)
                {
                    entranceRoom = room;
                    break;
                }
            }
        }

        playerCoord = entranceRoom.Floor[random.Next(0, entranceRoom.Floor.Count)];

        player.transform.position = CoordToVect(playerCoord, entranceRoom.Position);
    }

    void AdvanceLevel()
    {
        //Realizar transición y avanzar nivel

        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Movement>().enabled = false;
        player.GetComponent<AttackModule>().enabled = false;
        player.enabled = false;

        camManager.Zoom(10, 5);

        uiVisualizer.TransitionScene();

        Invoke("FinishLevel", 1f);
    }

    void FinishLevel()
    {
        PlayerState playerState = SavePlayerState();
        GameManagerModule.GetInstance().FinishLevel(playerState);
    }

    void ClearLevel()
    {
        //Stage Clear
        UIVisualizer.GetInstance().PopUpImportantMessage("STAGE CLEAR.\nADVANCE TO THE NEXT AREA.");
        //UIVisualizer.GetInstance().PopUp(PopUpType.Info, "Stage Clear!", PlayerModule.GetInstance().transform, 1.5f, 20);
        Debug.Log("Stage Clear!");

        camManager.ShakeQuake(10, 2.5f, false);
        camManager.Flash();
    }

    PlayerState SavePlayerState()
    {
        return player.SavePlayerState();
    }

    void LoadPlayerState(PlayerState state)
    {
        player.LoadPlayerState(state);
    }

    public List<List<RoomNode>> GetMapInfo()
    {
        return output.RoomComposites;
    }

    Vector3 CoordToVect(Coord c, Coord relativeTo, int tileSize = 16)
    {
        return new Vector3(relativeTo.x + (c.x + .5f) * tileSize, relativeTo.y - (c.y + .5f) * tileSize, 1);
    }

    public static LevelManager GetInstance()
    {
        return instance;
    }
}
