using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Chest : Entity
{
    PlayerModule player;

    [SerializeField] GameObject particles;

    [SerializeField] GameObject priceText;

    GameObject priceInstance;

    public int Price { get; protected set; } = 3;

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Chest);
        InitHealth(1);

        player = (PlayerModule)PlayerModule.GetInstance();
    }

    public void SetPrice(int price)
    {
        Price = price;
    }

    protected override void Start()
    {
        base.Start();

        transform.Rotate(Vector3.forward, Random.Range(-180, 180));
        priceInstance = Instantiate(priceText, transform.position + Vector3.down * 10, Quaternion.identity);
        priceInstance.GetComponent<TextMeshPro>().text = "Price: " + Price + " C";
    }

    private void FixedUpdate()
    {
        if (priceInstance != null)
        {
            priceInstance.transform.position = transform.position + Vector3.down * 10;
        }
    }

    bool CanPurchase()
    {
        return player.GetCarbonUnits() >= Price;
    }

    protected override void Die()
    {
        if (CanPurchase())
        {
            SoundManager.Play(Sound.Break, transform.position);
            base.Die();
            player.SpendCarbonUnits(Price);

            Instantiate(particles, player.transform.position, Quaternion.identity);

            priceInstance.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Vector2 direction = other.transform.rotation * Vector2.up;

            GetComponent<Rigidbody2D>().AddForce(direction.normalized * 200, ForceMode2D.Impulse);
        }
    }
}

public enum ChestType
{
    Module,
    Unit,
    Weapon
}