using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ShipMovement : MonoBehaviour
{

    public Vector3 targetPos;

    Vector3 offset;
    Vector3 shiftMov;

    Animator animator;

    bool waving = false;

    float sinOrigin;

    void Start()
    {
        animator = GetComponent<Animator>();

        shiftMov = Vector3.zero;

        Invoke("OpenShip", .3f);
        Invoke("Wave", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!waving)
        {
            offset = Vector3.zero;
        }
        else
        {
            offset = -Vector3.up * Mathf.Sin((Time.time - sinOrigin) * 4) * 2 + shiftMov;
        }

        transform.position = Vector3.Lerp(transform.position, targetPos + offset, Time.deltaTime * 6);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            animator.SetTrigger("close");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SceneManager.LoadScene("Upgrade");
        }
    }

    void OpenShip()
    {
        animator.SetTrigger("open");
    }

    void Wave()
    {
        shiftMov = Vector3.up * 12;
        sinOrigin = Time.time - 1;
        waving = true;
    }
}
