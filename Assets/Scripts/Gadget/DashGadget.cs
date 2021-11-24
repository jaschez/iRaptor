using System.Collections;
using UnityEngine;

public class DashGadget : Gadget
{

    public LayerMask collisionMask;

    public GameObject crosshair;
    GameObject crosshairGO;

    Movement movement;

    SpriteRenderer sr;

    float duration = .04f;
    float thrust = 70;

    float maxDistance = 70;

    Vector2 orientation;
    Vector3 destPosition;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (crosshairGO == null)
        {
            crosshairGO = Instantiate(crosshair, UIVisualizer.GetInstance().canvasParent);
            crosshairGO.transform.localPosition = Vector3.zero;
            crosshairGO.transform.position = transform.position;
        }

        SetUsesLeft(GetMaxUses());

        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();

        destPosition = transform.position;

        crosshairGO.transform.position = CalculateDashPoint(maxDistance);
    }

    // Update is called once per frame
    protected override void Update()
    {

        canUse = Time.time > finishCooldownTime;

        base.Update();
    }

    private void FixedUpdate()
    {

        destPosition = CalculateDashPoint(maxDistance);

        crosshairGO.transform.position = destPosition;

        //crosshairGO.transform.position = Vector3.Lerp(crosshairGO.transform.position, destPosition, Time.fixedDeltaTime * 18);
    }

    protected override void Use()
    {
        finishCooldownTime = Time.time + cooldown;
        SpendUse();

        SoundManager.Play(Sounds.Dash, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);


        if (Vector2.Distance(destPosition, transform.position) > maxDistance * 2)
        {
            destPosition = transform.position;
        }

        StartCoroutine(DashCoroutine());

            /*UIVisualizer.GetInstance().PopUp(PopUpType.Bad, "Unreachable", transform, Color.white, .8f, 15);
            CamManager.GetInstance().ShakeSingle(5f);*/
    }

    Vector3 CalculateDashPoint(float maxDist)
    {
        orientation = movement.GetPlayerOrientation();

        Vector3 auxPos = transform.position + (Vector3)(orientation * maxDist);
        Vector3 finalPos;

        if (Physics2D.OverlapCircle(auxPos, 3f))
        {
            RaycastHit2D ray = Physics2D.Raycast(transform.position, orientation, maxDist, collisionMask);

            if (ray.point != Vector2.zero)
            {
                finalPos = ray.point - orientation * 4; //Offset para no aparecer dentro de la pared
            }
            else
            {
                finalPos = auxPos;
            }
        }
        else
        {
            finalPos = auxPos;
        }

        finalPos.z = 1;

        return finalPos;
    }

    private IEnumerator DashCoroutine()
    {

        float dashFinishedTime = Time.time + duration;

        transform.position = destPosition;

        sr.enabled = false;

        movement.rb.simulated = false;

        movement.enabled = false;

        while (Time.time < dashFinishedTime)
        {
            yield return null;
        }

        sr.enabled = true;

        movement.enabled = true;
        movement.rb.simulated = true;
        movement.rb.velocity = orientation * movement.GetPlayerVelocity();

        yield return null;
    }

    bool CanDash(Vector3 pos)
    {
        return !Physics2D.OverlapCircle(pos, 3f);
    }

}
