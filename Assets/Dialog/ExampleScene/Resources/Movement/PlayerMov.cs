using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMov : MonoBehaviour
{
    DialogManager dialogManager;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dialogManager = DialogManager.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dialogManager.IsInDialog()) {
            rb.velocity = (Input.GetAxis("Horizontal") * Vector2.right + Input.GetAxis("Vertical") * Vector2.up) * Time.deltaTime * 500;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
