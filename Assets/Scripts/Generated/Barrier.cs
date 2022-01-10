using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : Entity
{
    bool blocked = false;

    SpriteRenderer spriteRenderer;

    Color originalColor;
    Color blockedColor = Color.gray;

    protected override void InitEntity()
    {
        SetEntityType(EntityType.Barrier);
        InitHealth(1);

        units = 0;
    }

    protected override void Start()
    {
        base.Start();

        if (!TryGetComponent(out spriteRenderer))
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        originalColor = spriteRenderer.color;

        transform.Rotate(Vector3.forward, Random.Range(-10, 10));
    }

    protected override void Die()
    {
        if (!blocked) {
            CamManager.GetInstance().ShakeQuake(4, 2.5f, false);
            SoundManager.Play(Sounds.Break, transform.position);
            base.Die();
        }
        else
        {
            spriteRenderer.color = blockedColor;
        }
    }

    public void Block(bool blocked)
    {
        this.blocked = blocked;

        if (blocked)
        {
            spriteRenderer.color = blockedColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
