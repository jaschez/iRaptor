using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DashModule : MonoBehaviour
{
    public LayerMask collisionMask;

    Movement movement;

    float duration = .15f;
    float finishCooldownTime = 0f;
    float cooldown = .3f;

    float distance = 60;

    bool locked = false;

    private void Start()
    {
        movement = GetComponent<Movement>();
        cooldown = .4f;
    }

    private void Update()
    {
        if (Controls.GetDashKeyDown() && !locked) {
            if(Time.time > finishCooldownTime)
            {
                Use();
            }
        }
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    private void Use()
    {
        if (movement.GetPlayerOrientation() != Vector2.zero) {
            finishCooldownTime = Time.time + cooldown;

            SoundManager.Play(Sound.Dash, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

            DashProcess();
        }
    }

    Vector3 CalculateDashPoint(float maxDist)
    {
        Vector2 orientation = movement.GetDirection().normalized;

        Vector3 auxPos = transform.position + (Vector3)(orientation * maxDist);
        Vector3 finalPos;

        float radio = 10f;

        RaycastHit2D ray = Physics2D.Raycast(transform.position, orientation, maxDist, collisionMask);

        if (ray.point != Vector2.zero)
        {
            finalPos = ray.point + ray.normal * radio;
        }
        else
        {
            finalPos = auxPos;
        }

        finalPos.z = 1;

        return finalPos;
    }

    private void DashProcess()
    {

        float dashFinishedTime = Time.time + duration;
        float finalDistance;

        Vector3 dashPoint = CalculateDashPoint(distance);

        Vector3[] path = new Vector3[] {
            transform.position, dashPoint
        };

        finalDistance = Vector2.Distance(transform.position, dashPoint);

        movement.enabled = false;
        movement.rb.bodyType = RigidbodyType2D.Kinematic;

        transform.DOPath(path, duration * (finalDistance / distance), PathType.Linear, PathMode.TopDown2D).SetEase(Ease.Linear)
            .OnComplete(()=>
            {
                movement.enabled = true;
                movement.rb.bodyType = RigidbodyType2D.Dynamic;

                transform.DOScaleX(.4f, .05f).OnComplete(() =>
                {
                    transform.DOScaleX(1f, .2f).SetEase(Ease.OutElastic);
                });
                transform.DOScaleY(1.5f, .08f).OnComplete(() =>
                {
                    transform.DOScaleY(1f, .3f).SetEase(Ease.OutElastic);
                });

            });
    }
}
