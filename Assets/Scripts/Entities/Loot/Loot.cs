using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Loot : Entity
{

    PlayerModule player;

    public GameObject particles;

    public GameObject priceText;

    GameObject priceInstance;

    int price = 3;

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Loot);
        InitHealth(1);

        dropType = DropType.Item;

        player = (PlayerModule)PlayerModule.GetInstance();
    }

    protected override void Start()
    {
        base.Start();

        transform.Rotate(Vector3.forward, Random.Range(-180, 180));
        priceInstance = Instantiate(priceText, transform.position + Vector3.down * 10, Quaternion.identity);
        priceInstance.GetComponent<TextMeshPro>().text = "Price: " + price + " C";
    }

    private void FixedUpdate()
    {
        if (priceInstance != null)
        {
            priceInstance.transform.position = transform.position + Vector3.down * 10;
        }
    }

    public void SetItem(ItemData item)
    {
        drops = LevelManager.GetInstance().Drops;
        actualDrops = new Drop[1];
        actualDrops[0] = Instantiate(drops[dropType], transform.position, Quaternion.identity).GetComponent<Drop>();
        actualDrops[0].gameObject.SetActive(false);
        ((ItemDrop)actualDrops[0]).SetItem(item);
    }

    bool CanPurchase()
    {
        return player.GetCarbonUnits() >= price;
    }

    protected override void Die()
    {
        if (CanPurchase())
        {
            SoundManager.Play(Sounds.Break, transform.position);
            base.Die();
            player.SpendCarbonUnits(price);

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
