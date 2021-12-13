using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryTrigger : MonoBehaviour
{
    public GameObject RoomObject { get; private set; }
    public Room AssociatedRoom { get; private set; }

    private WaveManager waveManager;

    public void Initialize(Room associated, GameObject obj)
    {
        AssociatedRoom = associated;
        RoomObject = obj;
        waveManager = WaveManager.GetInstance();

        RoomObject.SetActive(false);
    }

    void DiscoverRoom()
    {
        Minimap.GetInstance().UpdateMapRegion(AssociatedRoom);
        RoomObject.SetActive(true);

        waveManager.UpdateRoom(AssociatedRoom);
    }

    //Deberá de activar a los enemigos, actualizar el minimapa y activar el resto de cosas relacionadas con la habitacion en cuestion
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag.CompareTo("Player") == 0)
        {
            DiscoverRoom();
        }
    }
}
