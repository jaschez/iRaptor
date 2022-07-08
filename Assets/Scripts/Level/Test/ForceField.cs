using System.Collections;
using UnityEngine;

public class ForceField : MonoBehaviour
{

    public GameObject prefab;

    public Camera cam;

    GameObject[,] forceObjs;

    public TextMeshModifier textNotifier;

    public AudioClip start;
    public AudioSource loopAudio;

    int[] xOffset;
    int[] yOffset;

    public int offset = 20;

    public int size = 15;
    public float factor = 96f;

    float weirdValue = 0;

    bool canFade = false;

    float glitchTime = 0;

    private void OnEnable()
    {
        GameManagerModule.OnLoadingSceneFade += PreFade;
    }

    private void OnDisable()
    {
        GameManagerModule.OnLoadingSceneFade -= PreFade;
    }

    // Start is called before the first frame update
    void Start()
    {
        TransitionSystem.GetInstance().Clear();

        forceObjs = new GameObject[size, size];
        xOffset = new int[size];
        yOffset = new int[size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                forceObjs[i, j] = Instantiate(prefab, new Vector2(i * offset, j * offset), Quaternion.identity);
            }
        }

        //Super weird calculation
        weirdValue = 1653617004L / (Screen.width * Screen.height) * Screen.dpi / factor;

        cam.orthographicSize = 1;

        Invoke("ShowMessage", 1f);

        glitchTime = Time.time + Random.Range(.5f, 4f);
    }

    // Update is called once per frame
    void Update()
    {
        weirdValue = 1653617004L / (Screen.width * Screen.height) * Screen.dpi / factor;
        cam.orthographicSize = Mathf.Clamp(Mathf.Lerp(cam.orthographicSize, weirdValue + 5, Time.deltaTime * 3.4f), 0, weirdValue);

        if(canFade){
            loopAudio.volume = Mathf.Lerp(loopAudio.volume, 0, Time.deltaTime * 7);
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float value = Mathf.Sin(-Time.time * 4 + Vector2.Distance(new Vector2(i, j), new Vector2(size / 2, size / 2)) * .15f);

                forceObjs[i, j].transform.rotation = Quaternion.Euler(0, 0, value * 90);
                forceObjs[i, j].transform.position = new Vector2(i * offset + xOffset[i], j * offset + yOffset[j]);
                forceObjs[i, j].transform.localScale = Vector2.Lerp(forceObjs[i, j].transform.localScale, Vector2.one * .1f, Time.deltaTime * 8);
            }
        }

        if (glitchTime < Time.time)
        {
            glitchTime = Time.time + Random.Range(.1f, .3f);
            StartCoroutine(Glitch());
        }
    }

    void ShowMessage()
    {
        textNotifier.NotifyMessage("LOADING...", true);
    }

    void PreFade()
    {
        StartFade();
    }

    void StartFade()
    {
        canFade = true;
    }

    IEnumerator Glitch()
    {

        int movement = Random.Range(70, 150) * Random.Range(0, 10)>5?1:-1;
        int range = Random.Range(4, 7);
        int startLine = Random.Range(0, size);

        int axis = Random.Range(0, 2);

        float stillTime = Random.Range(.05f, .2f);

        for (int i = startLine; i < size && i - startLine < range; i++)
        {
            if (axis == 0)
            {
                xOffset[i] = movement;
            }
            else
            {
                yOffset[i] = movement;
            }
        }

        yield return new WaitForSeconds(stillTime);

        for (int i = startLine; i < size && i - startLine < range; i++)
        {
            if (axis == 0)
            {
                xOffset[i] = 0;
            }
            else
            {
                yOffset[i] = 0;
            }
        }

        yield return null;

    }
}
