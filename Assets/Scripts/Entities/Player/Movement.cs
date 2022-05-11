using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    static Movement instance;

    [HideInInspector]
    public Rigidbody2D rb;

    [SerializeField] GameObject interactSign;

    public ParticleSystem trailParticle;

    Interactable objectInteractable;
    Interactable selectedInteractable;
    int selectedInteractableID = -1;

    Vector2 playerOrientation;
    Vector2 lastPlayerOrientation;
    Vector2 joystickOrientation;
    Vector2 movementOrientation;
    Vector3 mousePos;

    float playerVelocity = 250;
    float acceleration = 4;//5
    float orientationAngle;
    float interactionCooldown = 1f;
    float interactionTime = 0;

    bool locked = false;

    Camera cam;

    AudioSource engineSound;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        engineSound = SoundManager.Play(Sounds.EngineLoop, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform, false, false);
        engineSound.loop = true;

        Axis joystickAxis = new Axis(
            KeyCode.W,
            KeyCode.S,
            KeyCode.A,
            KeyCode.D
            );

        KeyCode move = KeyCode.Z;
        KeyCode attack = KeyCode.X;
        KeyCode dash = KeyCode.O;
        KeyCode interact = KeyCode.E;
        KeyCode aim = KeyCode.Escape;

        /* Inicializa los controles una vez comienza el juego (basado en un archivo externo para evitar
         * que un jugador pueda modificar los controles desde el programa y solo tengan acceso a ello
         * los propietarios)
         */

        Controls.SetupControls(joystickAxis, move, attack, dash, aim, interact);

        playerVelocity = acceleration * 50;
    }

    public void Update()
    {
        Pitch();
        Move();
        Interaction();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out objectInteractable))
        {
            selectedInteractable = objectInteractable;
            selectedInteractableID = collider.GetHashCode();
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (selectedInteractable != null)
        {
            if (collider.GetHashCode() == selectedInteractableID)
            {
                selectedInteractable = null;
                selectedInteractableID = -1;

                interactSign.SetActive(false);
            }
        }
    }

    //Gestiona las interacciones con los objetos
    void Interaction()
    {
        if (selectedInteractable != null)
        {
            if (!locked)
            {
                interactSign.transform.position = transform.position + Vector3.up * 12;

                if (interactionTime < Time.time)
                {
                    interactSign.SetActive(true);

                    if (Controls.GetInteractKeyDown())
                    {
                        interactionTime = Time.time + interactionCooldown;
                        selectedInteractable.Interact();
                        interactSign.SetActive(false);
                    }
                }
            }
        }
    }

    //Controla la orientacion del jugador
    void Pitch()
    {
        if (!locked)
        {
            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            joystickOrientation = mousePos - transform.position;

            if (!JoystickDropped())
            {
                playerOrientation = joystickOrientation.normalized;

                orientationAngle = Vector2.SignedAngle(Vector2.up, playerOrientation);

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, orientationAngle), Time.deltaTime * 15);
            }
        }
    }

    //Activa el movimiento si detecta pulsación de tecla
    void Move()
    {
        if (Controls.GetMoveKeyDown() && !locked)
        {
            CamManager.GetInstance().ShakeQuake(5f, 1.5f, false);

            SoundManager.Play(Sounds.EngineStart, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
            engineSound.Play();
            trailParticle.Play();
        }

        if (Controls.GetMoveKey() && !locked)
        {
            movementOrientation.x = -Controls.GetJoystick2X();
            movementOrientation.y = Controls.GetJoystick2Y();

            rb.velocity = Vector2.Lerp(rb.velocity, movementOrientation * playerVelocity, Time.deltaTime * acceleration);
        }
        else
        {
            if (!locked)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * acceleration);
            }
            else
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 20);
            }

            if (!trailParticle.isStopped)
            {
                trailParticle.Stop();
            }
        }

        if (Controls.GetMoveKeyUp() || locked)
        {
            if (engineSound.isPlaying)
            {
                SoundManager.Play(Sounds.EngineOff, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);
                engineSound.Stop();
            }
        }


        rb.velocity = Vector2.ClampMagnitude(rb.velocity, playerVelocity);
    }

    public void Recoil()
    {
        transform.Rotate(0, 0, 30 * Mathf.Sign(Random.Range(-1f,1f)));
    }

    public void SetStartMode()
    {
        playerOrientation = Vector2.up;
        lastPlayerOrientation = playerOrientation;

        rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    public bool IsLocked()
    {
        return locked;
    }

    public bool OnGround()
    {
        return Physics2D.OverlapCircle((Vector2)transform.position + Vector2.down * 8f, .3f);
    }

    public static Movement GetInstance()
    {
        return instance;
    }

    public Vector2 GetDirection()
    {
        return movementOrientation;
    }

    public Vector2 GetPlayerOrientationByMovement()
    {
        if (JoystickDropped())
        {
            return lastPlayerOrientation;
        }

        return playerOrientation;
    }

    public Vector2 GetPlayerOrientation()
    {
        return playerOrientation;  
    }

    public Vector2 GetPlayerInertiaOrientation()
    {
        return lastPlayerOrientation;
    }

    public bool JoystickDropped()
    {
        return joystickOrientation == Vector2.zero;
    }

    public float GetPlayerVelocity()
    {
        return playerVelocity;
    }
}
