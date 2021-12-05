using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, Interactable
{
    int cost = 20;

    PlayerModule player;

    // Start is called before the first frame update
    void Start()
    {
        player = (PlayerModule)PlayerModule.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        bool result = player.SpendCarbonUnits(cost);

        if (result) {
            //Dar objeto a jugador
            gameObject.SetActive(false);
        }
    }
}
