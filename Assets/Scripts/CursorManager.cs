using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTex;

    static CursorManager instance;

    void Awake() {

        if (instance == null)
        {
            instance = this;
        }else if (instance != this)
        {
            Destroy(this);
        }

        Cursor.SetCursor(cursorTex, Vector2.one*32, CursorMode.ForceSoftware);
    }
}
