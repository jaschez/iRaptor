using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveParticles : MonoBehaviour
{

    public GameObject prefab;

    public Camera cam;

    GameObject[,] forceObjs;

    public AudioClip start;
    public AudioSource loopAudio;

    AsyncOperation asyncLoad;

    int[] xOffset;
    int[] yOffset;

    public int offset = 20;

    public int size = 15;
    float weirdValue = 0;

    bool canStart = false;
    bool canZoom = false;
    bool canClick = false;

    float glitchTime = 0;

    // Start is called before the first frame update
    void Start()
    {
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
        weirdValue = 1653617004L / (Screen.width * Screen.height) * Screen.dpi / 96f;

        cam.orthographicSize = 1;

        Invoke("ShowMessage", 1f);

        glitchTime = Time.time + Random.Range(.5f, 4f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canZoom)
        {
            //cam.orthographicSize = Mathf.Clamp(Mathf.Lerp(cam.orthographicSize, weirdValue + 5, Time.deltaTime * 3), 0, weirdValue);
        }
        else
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 1, Time.deltaTime * 15);
            loopAudio.volume = Mathf.Lerp(loopAudio.volume, 0, Time.deltaTime * 3);
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float value = Mathf.Sin(-Time.time * 4 + Vector2.Distance(new Vector2(i, j), new Vector2(size / 2, size / 2)) * .15f);

                forceObjs[i, j].transform.rotation = Quaternion.Euler(0, 0, value * 90);
                forceObjs[i, j].transform.position = new Vector2(i * offset + xOffset[i], j * offset + yOffset[j]);
                //forceObjs[i, j].transform.position = new Vector2(i * offset + valueC * 5, j * offset + value * 5) - (new Vector2(size / 2, size / 2) - new Vector2(i, j)).normalized * value * 8;//new Vector2(i * offset + valueC * 20, j * offset + value * 20);
                //forceObjs[i, j].transform.localScale = new Vector2(2 * Mathf.Abs(value) * 0.5f + .8f, 2 * Mathf.Abs(value) * 0.5f + .8f);

                if (canStart)
                {
                    forceObjs[i, j].transform.localScale = Vector2.Lerp(forceObjs[i, j].transform.localScale, Vector2.one * .1f, Time.deltaTime * 8);
                }
            }
        }

        if (glitchTime < Time.time)
        {
            glitchTime = Time.time + Random.Range(.1f, .3f);
            StartCoroutine(Glitch());
        }
    }

    void StartZoom()
    {
        canZoom = true;
    }

    void StartLoadingScene()
    {
        //StartCoroutine(LoadScene());
    }

    void StartScene()
    {
        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator Glitch()
    {

        int movement = Random.Range(70, 150) * Random.Range(0, 10) > 5 ? 1 : -1;
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

    IEnumerator LoadScene()
    {

        yield return null;

        //asyncLoad = SceneManager.LoadSceneAsync("Game");

        asyncLoad.allowSceneActivation = false;

        while (!canStart)
        {

            if (asyncLoad.progress >= .9f)
            {
                //textNotifier.StopLoop();

                if (!canStart)
                {
                    AudioSource.PlayClipAtPoint(start, cam.transform.position);
                    canStart = true;
                    //textObj.SetActive(false);
                    //endAnimObj.SetActive(true);
                    Invoke("StartZoom", .2f);
                    Invoke("StartScene", 1f);
                }
            }

            yield return null;
        }
    }
}

