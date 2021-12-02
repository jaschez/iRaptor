using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryTrigger : MonoBehaviour
{
    public Room AssociatedRoom { get; private set; }

    public void Initialize(Room associated)
    {
        AssociatedRoom = associated;
    }

    //Deberá de activar a los enemigos, actualizar el minimapa y activar el resto de cosas relacionadas con la habitacion en cuestion
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.tag.CompareTo("Player") == 0)
        {
            Minimap.GetInstance().UpdateMapRegion(AssociatedRoom);
        }
    }
}
