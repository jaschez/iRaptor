using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpDrop : Drop
{

    PowerUpType powerUpType;

    protected override void InitDrop()
    {
        dropType = DropType.PowerUp;

        powerUpType = (PowerUpType)Random.Range(0, GetTotalPowerups());
    }

    protected override void ActivateDrop()
    {
        Debug.Log(powerUpType.ToString());
        StartCoroutine(ActivateAnimation());

        //playerModule.ActivateBuff((int)powerUpType);
    }

    public void SetPowerUpType(PowerUpType type)
    {
        powerUpType = type;
    }

    public static int GetTotalPowerups() {
        return (int)PowerUpType.Count;
    }

    public enum PowerUpType
    {
        Explosive = 0,
        Toxic = 1,
        Drill = 2,
        Count = 3
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

        playerModule.ActivateBuff((int)powerUpType);

        CamManager.GetInstance().ShakeQuake(4, 2f, false);

        chargeSound.Stop();
        SoundManager.Play(Sounds.ActivatePU, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        gameObject.SetActive(false);
        yield return null;
    }
}
