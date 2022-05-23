using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryTrigger : MonoBehaviour
{
    public GameObject RoomObject { get; private set; }
    public RoomNode AssociatedRoom { get; private set; }

    private WaveManager waveManager;

    static Transform player;

    static Stack<int> collidingEntries = new Stack<int>();
    
    static int currentEntryCode = -1;

    int hash;

    bool entered = false;

    public void Initialize(RoomNode associated, GameObject obj)
    {
        AssociatedRoom = associated;
        RoomObject = obj;
        waveManager = WaveManager.GetInstance();

        hash = GetHashCode();

        RoomObject.SetActive(false);
    }

    void DiscoverRoom()
    {
        if (collidingEntries.Count == 0)
        {
            Minimap.GetInstance().UpdateMapRegion(AssociatedRoom);
            RoomObject.SetActive(true);

            waveManager.UpdateRoom(AssociatedRoom);

            entered = true;
        }
    }

    void CheckState()
    {
        if (currentEntryCode != hash)
        {
            entered = false;
        }
    }

    //Deberá de activar a los enemigos, actualizar el minimapa y activar el resto de cosas relacionadas con la habitacion en cuestion
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag.CompareTo("Player") == 0)
        {
            currentEntryCode = hash;

            if (player == null)
            {
                player = collider.gameObject.transform;
            }

            if (!collidingEntries.Contains(hash))
            {
                collidingEntries.Push(hash);
            }

            CheckState();

        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag.CompareTo("Player") == 0)
        {
            if (collidingEntries.Count > 0)
            {
                collidingEntries.Pop();

                if (collidingEntries.Count > 1)
                {
                    currentEntryCode = collidingEntries.Peek();
                }
            }

            if (currentEntryCode == hash)
            {
                if (!entered)
                {
                    DiscoverRoom();
                }
            }
        }
    }
}
