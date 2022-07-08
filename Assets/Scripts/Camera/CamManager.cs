using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CamManager : MonoBehaviour
{

    static CamManager instance;

    Movement movManager;

    Camera camComponent;

    Transform target;
    Transform cameraTransform;

    public Animator flashAnimator;
    public RectTransform crosshair;

    Vector2 lastPos;
    Vector2 screenOffset;
    Vector2 camPausePos;

    bool shocking = false;

    float sightOffset = 10;
    float recoilOffset = 10;
    float shakeAmount = 0;
    float endTime;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        movManager = Movement.GetInstance();

        target = GameObject.FindGameObjectWithTag("Player").transform;
        cameraTransform = transform.GetChild(0);
        camComponent = cameraTransform.GetComponent<Camera>();

        lastPos = cameraTransform.localPosition;
        screenOffset = Vector2.zero;

        camComponent.orthographicSize = 30;

        Zoom(80, 2);
    }

    void LateUpdate()
    {
        FollowingCalculations();
        ShakeCalculations();
    }

    void FollowingCalculations()
    {
        if (!movManager.IsLocked()) {
            screenOffset = GetMousePosition() * 15;
            transform.position = (Vector2)target.position + screenOffset;
        }
    }

    public void SetCamPos(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void ShakeCalculations()
    {
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, lastPos, Time.deltaTime * 5);
        cameraTransform.localPosition = Vector3.ClampMagnitude(cameraTransform.localPosition, 60);
    }

    public void ShakeSingle(float amt)
    {
        shakeAmount = amt;
        cameraTransform.localPosition += (Vector3)Random.insideUnitCircle * shakeAmount;
    }

    public void Flash()
    {
        flashAnimator.SetTrigger("flash");
    }

    public void Zoom(float target, float velocity)
    {
        StartCoroutine(ZoomTransition(target, velocity));
    }

    public void ZoomAnimation(float targetSize, float duration)
    {
        camComponent.DOOrthoSize(targetSize, duration).SetEase(Ease.OutQuad);
    }

    public void ShakeAnimation(float strength, float duration, int vibrato = 10, bool fadeOut = true)
    {
        camComponent.DOShakePosition(duration, strength, vibrato, 90, fadeOut);
    }

    public void ShockGame(float time)
    {
        if (!shocking)
        {
            StartCoroutine(Shock(time));
        }
    }

    IEnumerator Shake(float amount, float duration, bool end)
    {
        endTime = Time.time + duration;
        float amt = amount;
        float origSize = camComponent.orthographicSize;

        while (Time.time < endTime)
        {
            amt = Mathf.Lerp(amt, 0, Time.deltaTime * 30 / duration);

            if (end)
            {
                camComponent.orthographicSize = Mathf.Lerp(camComponent.orthographicSize, origSize + 1f, Time.deltaTime * 10);
            }

            ShakeSingle(amt);
            yield return new WaitForSeconds(.04f);

        }
    }

    IEnumerator ZoomTransition(float targetSize, float velocity)
    {
        int sign = camComponent.orthographicSize < targetSize? 1 : -1;
        float range = .3f;

        while (Mathf.Abs(camComponent.orthographicSize - targetSize) > range)
        {
            camComponent.orthographicSize = Mathf.Lerp(camComponent.orthographicSize, targetSize + range * sign, Time.deltaTime * velocity);

            yield return null;
        }

        camComponent.orthographicSize = targetSize;

        yield return null;

    }

    IEnumerator Shock(float time){
        shocking = true;

        Time.timeScale = 0.2f;

        yield return new WaitForSecondsRealtime(time);

        Time.timeScale = 1f;

        shocking = false;
    }

    public void ShakeQuake(float amt, float duration, bool end)
    {
        //StopAllCoroutines();
        StartCoroutine(Shake(amt, duration, end));
    }

    public void Recoil()
    {
        Vector2 orientation;
        float magnitude = recoilOffset * 4;
        float bounds = recoilOffset * (Controls.GetMoveKey() ? 5 : 3);

        orientation = movManager.GetPlayerOrientationByMovement();

        screenOffset = Vector2.ClampMagnitude(screenOffset + orientation * magnitude, bounds);
    }

    Vector2 GetMousePosition()
    {
        Vector2 mousePosition;
        Vector2 normalizedPosition;

        mousePosition = Input.mousePosition;
        normalizedPosition = Vector2.zero;

        normalizedPosition.x = (mousePosition.x - Screen.width / 2) / (Screen.width / 2);
        normalizedPosition.y = (mousePosition.y - Screen.height / 2) / (Screen.height / 2);

        return normalizedPosition;
    }

    public float GetCameraSize()
    {
        return camComponent.orthographicSize;
    }

    public static CamManager GetInstance()
    {
        return instance;
    }
}
