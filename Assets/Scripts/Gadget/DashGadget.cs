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
    }

    protected override void Use()
    {
        finishCooldownTime = Time.time + cooldown;
        rechargeCooldownTime = finishCooldownTime + rechargeCooldown;

        SpendUse();

        SoundManager.Play(Sounds.Dash, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

        if (GetUsesLeft() == 0)
        {
            exhausted = true;
        }

        StartCoroutine(DashCoroutine());
    }

    Vector3 CalculateDashPoint(float maxDist)
    {
        orientation = movement.GetPlayerOrientation();

        Vector3 auxPos = transform.position + (Vector3)(orientation * maxDist);
        Vector3 finalPos;

        //Si resulta que en el lugar de reaparición existe un obstáculo o no es una coordenada válida,
        //se deberá buscar el último punto de colisión válido para reaparecer ahí
        //if (Physics2D.OverlapCircle(auxPos, 3f))

        //Usaremos esta opción en caso de que favorezca a la movilidad:

        // - Encontrar todos los puntos de colisión
        // - Iterar hasta encontrar el punto más lejano que se encuentre dentro de una cueva conocida
        /*RaycastHit2D[] rays = Physics2D.RaycastAll(transform.position, orientation, maxDist, collisionMask);
        System.Array.Sort(rays, (x, y) => x.distance.CompareTo(y.distance));*/

        //Habrá que tener esto en cuenta
        // - (Si la cueva mencionada es conocida, habrá que activar su trigger de entrada o salida, ya que
        // reaparecer ahí (teletransportarse hasta dicha cueva) es como si hubiera entrado en la cueva)

        RaycastHit2D ray = Physics2D.Raycast(transform.position, orientation, maxDist, collisionMask);

        if (ray.point != Vector2.zero)
        {
            //Necesitamos que exista cierto offset, ya que no podemos situar el punto de reaparición
            //justo donde el rayo choca, ya que existe la posibilidad de que el jugador se encuentre
            //en una esquina y, por accidente, la atraviese debido a la escasez de pared entre dos muros.
            finalPos = ray.point - orientation * 8;
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
