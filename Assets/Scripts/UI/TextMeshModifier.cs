using UnityEngine;
using System.Collections;
using TMPro;

public class TextMeshModifier : MonoBehaviour
{

    public float AngleMultiplier = 1.0f;
    public float SpeedMultiplier = 1.0f;
    public float ScaleMultiplier = 1.0f;
    public float RotationMultiplier = 1.0f;

    private TMP_Text m_TextComponent;
    private bool hasTextChanged;

    private bool looping = false;


    void Awake()
    {
        m_TextComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // Subscribe to event fired when text object has been regenerated.
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }


    public void NotifyMessage(string msg, bool loop = false)
    {

        StopAllCoroutines();
        //m_TextComponent.Rebuild(UnityEngine.UI.CanvasUpdate.PreRender);
        StartCoroutine(NotifyLoop(msg, loop));
    }


    void ON_TEXT_CHANGED(Object obj)
    {
        if(obj == m_TextComponent)
            hasTextChanged = true;
    }

    void ModifyColor(TMP_TextInfo textInfo, int currentCharacter, Color32 color)
    {
        Color32[] newVertexColors;

        int materialIndex = textInfo.characterInfo[currentCharacter].materialReferenceIndex;

        // Get the vertex colors of the mesh used by this text element (character or sprite).
        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

        // Get the index of the first vertex used by this text element.
        int vertexIndex = textInfo.characterInfo[currentCharacter].vertexIndex;

        // Only change the vertex color if the text element is visible.
        if (textInfo.characterInfo[currentCharacter].isVisible)
        {
            newVertexColors[vertexIndex + 0] = color;
            newVertexColors[vertexIndex + 1] = color;
            newVertexColors[vertexIndex + 2] = color;
            newVertexColors[vertexIndex + 3] = color;

            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
            m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

    /// <summary>
    /// Method to animate vertex colors of a TMP Text object.
    /// </summary>
    /// <returns></returns>
    IEnumerator NotifyLoop(string msg, bool loop = false)
    {
        m_TextComponent.text = msg;

        looping = loop;

        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        Color32 origColor = textInfo.characterInfo[0].color;
        Color32 invColor = origColor;
        invColor.a = 0;

        Vector3[][] copyOfVertices = new Vector3[0][];

        hasTextChanged = true;

        int[] charWaveCount = new int[textInfo.characterCount];
        bool[] charWaveFlag = new bool[textInfo.characterCount];

        float startTime = Time.time;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            ModifyColor(textInfo, i, invColor);
        }

        while (charWaveCount[charWaveCount.Length - 1] <= 0)
        {
            // Allocate new vertices 
            if (hasTextChanged)
            {
                if (copyOfVertices.Length < textInfo.meshInfo.Length)
                {
                    copyOfVertices = new Vector3[textInfo.meshInfo.Length][];

                    charWaveCount = new int[textInfo.characterCount];
                    charWaveFlag = new bool[textInfo.characterCount];
                }

                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    int length = textInfo.meshInfo[i].vertices.Length;
                    copyOfVertices[i] = new Vector3[length];
                }

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            Vector3 sinVector;
            float sinValue, waveValue;

            for (int j = 0; j < characterCount; j++)
            {

                sinValue = Mathf.Sin((Time.time - startTime) * -8 + j * .1f + 2f);

                if (sinValue < 0 && charWaveFlag[j])
                {
                    charWaveCount[j]++;
                    charWaveFlag[j] = false;

                    sinValue = 0;
                }

                if (sinValue > 0)
                {
                    if(!looping)
                        charWaveFlag[j] = true;
                }

                if (charWaveCount[j] > 0)
                {
                    sinValue = 0;
                }

                waveValue = (Mathf.Clamp(sinValue, .8f, 1) - .8f) * 5;

                sinVector = Vector3.up * Mathf.Abs(waveValue * 15);

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!textInfo.characterInfo[j].isVisible)
                    continue;

                if (waveValue > .9f)
                {
                    ModifyColor(textInfo, j, origColor);
                }

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[j].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[j].vertexIndex;

                // Get the vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

                // Need to translate all 4 vertices of each quad to aligned with center of character.
                // This is needed so the matrix TRS is applied at the origin for each character.
                copyOfVertices[materialIndex][vertexIndex + 0] = sourceVertices[vertexIndex + 0] + sinVector;
                copyOfVertices[materialIndex][vertexIndex + 1] = sourceVertices[vertexIndex + 1] + sinVector;
                copyOfVertices[materialIndex][vertexIndex + 2] = sourceVertices[vertexIndex + 2] + sinVector;
                copyOfVertices[materialIndex][vertexIndex + 3] = sourceVertices[vertexIndex + 3] + sinVector;
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = copyOfVertices[i];
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            yield return new WaitForFixedUpdate();
        }

        string origText = m_TextComponent.text;

        m_TextComponent.SetText(origText);

        yield return new WaitForSeconds(1f);

        int chars = textInfo.characterCount;

        for (int i = 0; i < chars; i++)
        {
            origText = origText.Substring(0 , origText.Length - 1);

            m_TextComponent.SetText(origText + "_");

            yield return new WaitForSeconds(.3f/chars);
        }

        int numOfBlinks = 2;

        for (int i = 0; i < numOfBlinks; i++)
        {
            m_TextComponent.SetText("_");
            yield return new WaitForSeconds(1 / (float)(numOfBlinks * 4));
            m_TextComponent.SetText("");
            yield return new WaitForSeconds(1 / (float)(numOfBlinks * 4));
        }
    }

    public void StopLoop()
    {
        looping = false;
    }
}