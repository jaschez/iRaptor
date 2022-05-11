using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WorkbenchParticles : MonoBehaviour, Interactable
{
    public GameObject particlePrefab;

    public Animator effectAnimator;

    public GameObject backButton;

    public float amplitude = 3;
    public float velocity = 1;

    int totalAvailable;

    float originalCamSize;

    float deltaOffset = 0;

    float lastVelocity;

    bool interacted = false;

    SpriteMask mask;

    List<ItemData> unlockableItems;

    List<GameObject> particles;
    List<LobbyItem> items;

    void Start()
    {
        lastVelocity = velocity;
    }

    public void Initialize(List<ItemData> _unlockableItems)
    {
        particles = new List<GameObject>();
        items = new List<LobbyItem>();
        mask = gameObject.GetComponentInChildren<SpriteMask>();

        unlockableItems = _unlockableItems;

        totalAvailable = unlockableItems.Count;

        for (int i = 0; i < unlockableItems.Count; i++)
        {
            GameObject particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
            LobbyItem item = particleInstance.GetComponent<LobbyItem>();

            item.Initialize(unlockableItems[i], this);
            item.Hide();

            items.Add(item);

            particles.Add(particleInstance);
        }

        backButton.SetActive(false);
    }

    void FixedUpdate()
    {
        if (velocity != lastVelocity)
        {
            deltaOffset = deltaOffset + (lastVelocity - velocity) * Time.time;
        }

        if (particles != null)
        {
            float increasedOffset = 0;
            float offset = 2 * Mathf.PI / particles.Count;
            float argument;

            foreach (GameObject particle in particles)
            {
                argument = Time.time * velocity + increasedOffset + deltaOffset;

                Vector3 position = new Vector3(Mathf.Sin(argument) * amplitude, Mathf.Cos(argument) * amplitude, 1);

                particle.transform.position = transform.position + position;

                increasedOffset += offset;
            }
        }


        lastVelocity = velocity;
    }

    public void CloseWorkbench()
    {
        if (interacted)
        {
            StopAllCoroutines();
            StartCoroutine(CloseWorkbenchCoroutine());

            interacted = false;
            backButton.SetActive(false);
        }
    }

    IEnumerator OpenWorkbenchCoroutine()
    {
        float startTime = Time.time;
        
        originalCamSize = CamManager.GetInstance().GetCameraSize();

        Transform player = PlayerModule.GetInstance().gameObject.transform;

        CamManager.GetInstance().ZoomAnimation(50, .9f);
        Movement.GetInstance().Lock();

        player.DORotate(Vector3.zero, .7f).SetEase(Ease.OutQuint);
        player.DOMove(transform.position, .8f).SetEase(Ease.OutQuint);

        DOTween.To(() => velocity, x => velocity = x, 30, .8f).SetEase(Ease.OutQuart);
        DOTween.To(() => amplitude, x => amplitude = x, 3, .7f).SetEase(Ease.InOutQuad);

        CamManager.GetInstance().ShakeAnimation(1, .8f, 25, false);

        yield return new WaitForSeconds(.8f);

        CamManager.GetInstance().ShakeAnimation(2, .8f);

        items.ForEach(item => item.Show());

        DOTween.To(() => velocity, x => velocity = x, .1f, 1.5f).SetEase(Ease.OutQuart);
        DOTween.To(() => amplitude, x => amplitude = x, 30, 1f).SetEase(Ease.OutBack);

        effectAnimator?.SetTrigger("trigger");

        yield return new WaitForSeconds(.1f);

        CamManager.GetInstance().ZoomAnimation(60, .5f);
        DOTween.To(() => mask.alphaCutoff, x => mask.alphaCutoff = x, 0, 1f).SetEase(Ease.InOutQuint);

        yield return new WaitForSeconds(.5f);

        backButton.SetActive(true);

        interacted = true;
    }

    IEnumerator CloseWorkbenchCoroutine()
    {
        float startTime = Time.time;

        CamManager.GetInstance().ZoomAnimation(80, 1f);
        Movement.GetInstance().Unlock();

        DOTween.To(() => mask.alphaCutoff, x => mask.alphaCutoff = x, 1, 1f);

        foreach (LobbyItem item in items)
        {
            item.Hide();
        }

        while (Time.time - startTime < 3)
        {
            velocity = Mathf.Lerp(velocity, 2, Time.deltaTime * 4);
            amplitude = Mathf.Lerp(amplitude, 15, Time.deltaTime * 3);
            yield return null;
        }
    }

    public void Unlock(LobbyItem lobbyItem)
    {
        bool result = LobbyManager.GetInstance().UnlockItem(lobbyItem.itemData);
        
        if (result)
        {
            lobbyItem.gameObject.SetActive(false);
            totalAvailable--;
        }
    }

    public void Interact()
    {
        if (totalAvailable > 0) {
            StopAllCoroutines();
            StartCoroutine(OpenWorkbenchCoroutine());
        }
        else
        {
            Debug.Log("Cannot interact; all items were already purchased!");
        }
    }
}
