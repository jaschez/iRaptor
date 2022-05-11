using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIOrbs : UIComponent
{
    TextMeshProUGUI orbsTxt;

    public override void HookEvents()
    {
        LobbyManager.UpdateOrbs += SetValue;
    }

    public override void Initialize()
    {
        orbsTxt = GetComponent<TextMeshProUGUI>();
    }

    void SetValue(int value)
    {
        orbsTxt.text = value.ToString();
    }
}
