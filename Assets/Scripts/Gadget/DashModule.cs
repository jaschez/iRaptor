using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DashModule : MonoBehaviour
{
    public LayerMask collisionMask;

    Movement movement;

    Transform renderObj;

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
        if (Controls.GetMoveKey()) {
            finishCooldownTime = Time.time + cooldown;

            SoundManager.Play(Sound.Dash, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

            DashProcess();
        }
    }

    Vector3 CalculateDashPoint(float maxDist, out bool hit, out Vector3 slide)
    {
        Vector2 orientation = movement.GetDirection().normalized;
        Vector3 destination = transform.position + (Vector3)(orientation * maxDist);
        Vector3 finalPos;

        float radio = 6f;

        RaycastHit2D ray = Physics2D.CircleCast(transform.position, radio, orientation, maxDist, collisionMask);

        if (ray.collider != null)
        {
            finalPos = ray.centroid;
            hit = true;

            Vector2 deltaSlide = destination - finalPos;
            Vector2 normal = new Vector2(Mathf.Abs(ray.normal.x), Mathf.Abs(ray.normal.y));
            Vector2 slidePosition = new Vector2(deltaSlide.x * normal.y, deltaSlide.y * normal.x);
            Vector2 slideDir = slidePosition.normalized;

            RaycastHit2D slideRay = Physics2D.CircleCast(finalPos, radio - .1f, slideDir, slidePosition.magnitude, collisionMask);

            if (slideRay.collider != null)
            {
                slide = slideRay.centroid;
            }
            else
            {
                slide = finalPos + (Vector3)slidePosition;
            }
        }
        else
        {
            hit = false;
            finalPos = destination;
            slide = Vector3.zero;
        }

        finalPos.z = 1;
        slide.z = 1;

        return finalPos;
    }

    private void DashProcess()
    {
        float finalDistance;
        float finalDuration = duration;

        Vector3 dashPoint = CalculateDashPoint(distance, out bool hit, out Vector3 slidePoint);

        Vector3[] path = new Vector3[hit? 3 : 2];

        path[0] = transform.position;

        if (!hit)
        {
            path[1] = dashPoint;

            finalDistance = Vector2.Distance(transform.position, dashPoint);
        }
        else
        {
            path[1] = dashPoint;
            path[2] = slidePoint;

            finalDistance = Vector2.Distance(transform.position, slidePoint) + Vector2.Distance(slidePoint, dashPoint);

            if (finalDistance < distance)
            {
                finalDuration = duration * (finalDistance / distance);
            }
        }

        if (renderObj == null) {
            renderObj = PlayerModule.GetInstance().GetSpriteRenderer().transform;
        }
        
        Vector2 direction = movement.GetDirection().normalized;
        Vector2 orientation = movement.GetPlayerOrientation().normalized;

        float angle = Vector2.SignedAngle(direction, orientation);
        float oppositeAngle = Vector2.SignedAngle(-direction, orientation);
        float result;

        if (Mathf.Abs(angle) < Mathf.Abs(oppositeAngle))
        {
            result = angle;
        }
        else
        {
            result = oppositeAngle;
        }

        renderObj.DOLocalRotate(new Vector3(result, 90, 0), .05f);

        transform.DOPath(path, finalDuration, PathType.Linear).SetEase(Ease.Linear)
            .OnComplete(()=>
            {
                renderObj.DOLocalRotate(Vector3.zero, .05f);

                renderObj.DOScaleX(.4f, .05f).OnComplete(() =>
                {
                    renderObj.DOScaleX(1f, .2f).SetEase(Ease.OutElastic);
                });
                renderObj.DOScaleY(1.5f, .08f).OnComplete(() =>
                {
                    renderObj.DOScaleY(1f, .3f).SetEase(Ease.OutElastic);
                });

            });
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.cyan;

        Vector3 dash = CalculateDashPoint(distance, out bool hit, out Vector3 slide);

        Gizmos.DrawWireSphere(dash, 6);

        if (hit)
        {
            Gizmos.DrawWireSphere(slide, 4);
        }*/
    }
}
