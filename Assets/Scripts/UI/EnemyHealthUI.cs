using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    EnemyModule target;

    Slider healthIndicator;

    SpriteRenderer sr;

    Vector3 offset;

    float height;

    public void AttachTarget(EnemyModule target)
    {
        healthIndicator = GetComponent<Slider>();
        this.target = target;

        target.OnEntityEvent += OnChangedHealth;

        if (!target.TryGetComponent(out sr))
        {
            //Cuidado para cuando haya mas de un sprite asociado a un enemigo
            sr = target.GetComponentsInChildren<SpriteRenderer>(true)[0];
        }

        height = sr.bounds.size.y;
        offset = Vector3.up * height;

        healthIndicator.value = target.GetHP() / (float)target.GetMaxHP();
    }

    void DetachTarget()
    {
        target.OnEntityEvent -= OnChangedHealth;
        target = null;
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.transform.position + offset;
        }
    }

    void OnChangedHealth(Entity sender, Entity.EntityEvent eventType, object param)
    {
        if (eventType == Entity.EntityEvent.Damage)
        {
            healthIndicator.value = target.GetHP() / (float)target.GetMaxHP();
        }

        if (eventType == Entity.EntityEvent.Death)
        {
            DetachTarget();
            Destroy(gameObject);
        }
    }
}
