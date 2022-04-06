using System.Collections;
using UnityEngine;

public class ItemDrop : Drop
{
    ItemData item;

    protected override void InitDrop()
    {
        dropType = DropType.Item;
    }

    protected override void ActivateDrop()
    {
        StartCoroutine(ActivateAnimation());
    }

    public void SetItem(ItemData item)
    {
        this.item = item;
    }

    IEnumerator ActivateAnimation()
    {
        float origin = Time.time;
        float sinValue = 0;
        float lastValue;

        Vector3 offset;

        Collider2D[] comps = GetComponents<Collider2D>();

        foreach (Collider2D c in comps)
        {
            c.enabled = false;
        }

        AudioSource chargeSound = SoundManager.Play(Sounds.ChargePowerup, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        while (sinValue >= 0) {

            lastValue = sinValue;
            sinValue = Mathf.Sin((Time.time - origin) * 5) * 20f;

            offset = Vector2.up * sinValue;
            offset.z = 0;

            transform.Rotate(0,0,Mathf.Abs(sinValue - lastValue) * 20);

            transform.position = player.position + offset;

            yield return new WaitForFixedUpdate();
        }

        playerModule.AddItem(item);

        CamManager.GetInstance().ShakeQuake(4, 2f, false);

        chargeSound.Stop();
        SoundManager.Play(Sounds.ActivatePU, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        gameObject.SetActive(false);
        yield return null;
    }
}
