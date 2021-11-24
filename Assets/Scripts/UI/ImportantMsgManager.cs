using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImportantMsgManager : MonoBehaviour
{

    public GameObject charPrefab;

    public Transform pivot;

    public float spacing = 2f;
    public float lineWidth = 2f;

    float xOffset, yOffset;

    List<GameObject> characters;

    private void Start()
    {
        xOffset = pivot.position.x;
        yOffset = pivot.position.y;

        characters = new List<GameObject>();

        CreateCharacters("New module installed");
    }

    public void CreateMessage(string msg)
    {
        CreateCharacters(msg);
    }

    void CreateCharacters(string msg)
    {

        char[] msgArray = msg.ToCharArray();
        int firstLineChar = 0;
        int line = 0;
        int lineCharLimit = 12;
        int currentLineChars = 0;
        float totalLineSize = 0;

        for (int i = 0; i < msgArray.Length; i++)
        {
            if (msgArray[i] == '\n' || (msgArray[i] == ' ' && currentLineChars >= lineCharLimit))
            {
                line++;
                firstLineChar = i;
                currentLineChars = 0;
                totalLineSize = 0;
            }
            else if (msgArray[i] != ' ')
            {

                int charIndex = i - firstLineChar;
                
                GameObject go = Instantiate(charPrefab, pivot);

                go.GetComponent<Text>().text = msgArray[i].ToString();
                go.GetComponent<Text>().SetAllDirty();
                Canvas.ForceUpdateCanvases();
                totalLineSize += go.GetComponent<RectTransform>().sizeDelta.x /* spacing*/;

                Vector3 charPos = new Vector3(totalLineSize, line * lineWidth, 0);
                go.transform.localPosition = charPos;

                //go.SetActive(false);
                characters.Add(go);

                currentLineChars++;
            }
        }
    }

}
