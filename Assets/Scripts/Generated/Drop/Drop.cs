using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Drop : MonoBehaviour
{

    protected DropType dropType;

    protected Rigidbody2D rb;

    float power = 400;
    float attrModule = 800;

    int acceleration = 8;

    private Vector2 attrForce;

    bool activated = false;

    protected PlayerModule playerModule;
    protected Transform player;

    protected abstract void InitDrop();
    protected abstract void ActivateDrop();

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        playerModule = (PlayerModule)PlayerModule.GetInstance();
        player = playerModule.transform;

        rb.AddForce(Random.insideUnitCircle * power, ForceMode2D.Impulse);
        rb.AddTorque(10, ForceMode2D.Impulse);

        InitDrop();
    }

    void Update()
    {
        if (!activated)
        {
            if (Vector2.Distance(transform.position, player.position) < 60)
            {
                attrForce = (player.position - transform.position).normalized * attrModule * .7f;

                rb.velocity = Vector2.Lerp(rb.velocity, attrForce, Time.deltaTime * acceleration);
            }
            else
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * acceleration * 3);
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Entity>()?.GetEntityType() == EntityType.Player)
        {
            ActivateDrop();

            CamManager.GetInstance().ShakeQuake(4f, 1.5f, false);
            SoundManager.Play(Sounds.Drop, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

            if (transform.childCount > 0) {
                Transform trail = transform.GetChild(0);

                trail.parent = player.transform;
                trail.localPosition = Vector2.zero;
                trail.GetComponent<TrailRenderer>().emitting = false;

                Destroy(trail.gameObject, .5f);
            }

            if(dropType !=DropType.Item)
            gameObject.SetActive(false);
        }
    }

    public DropType GetDropType()
    {
        return dropType;
    }


}

public enum DropType
{
    HP,
    CarbonUnit,
    Item,
    DashUnit
}
