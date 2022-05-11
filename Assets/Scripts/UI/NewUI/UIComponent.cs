using UnityEngine;

public abstract class UIComponent : MonoBehaviour
{
    public abstract void Initialize();
    public abstract void HookEvents();
}
