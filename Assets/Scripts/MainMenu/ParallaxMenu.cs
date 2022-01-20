using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMenu : MonoBehaviour
{
    public float overallSensitivity = 0.125f;

    [SerializeField]
    ParallaxLayer[] layers;

    Vector3 mousePos;

    bool enabledParallax = false;

    private void Start()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].startPos = layers[i].layerObj.transform.localPosition;
        }
    }

    void Update()
    {
        if (enabledParallax)
        {
            mousePos.x = Input.mousePosition.x - Screen.width / 2;
            mousePos.y = Input.mousePosition.y - Screen.height / 2;

            mousePos *= overallSensitivity / 100f;

            foreach (ParallaxLayer layer in layers)
            {
                layer.layerObj.transform.localPosition = Vector3.Lerp(layer.layerObj.transform.localPosition, layer.startPos + mousePos * layer.sensitivity, Time.deltaTime * 5);
            }
        }
    }

    public void EnableParallax(bool enabled)
    {
        enabledParallax = enabled;
    }

    [System.Serializable]
    struct ParallaxLayer
    {
        public GameObject layerObj;
        public float sensitivity;

        [HideInInspector]
        public Vector3 startPos;
    }
}
