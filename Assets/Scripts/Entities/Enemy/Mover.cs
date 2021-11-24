using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover:MonoBehaviour
{

    Rigidbody2D rb;
    float velocity;
    
    public void InitMover(float vel)
    {
        velocity = vel;
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 dir)
    {
        rb.velocity = Vector2.Lerp(rb.velocity, dir*velocity, Time.deltaTime * 5);
    }

    public void MoveTo(Vector2 pos)
    {
        StopAllCoroutines();
        StartCoroutine(MovingTo(pos));
    }

    IEnumerator MovingTo(Vector2 pos)
    {
        Vector2 dir = ((Vector2)transform.position - pos).normalized;

        while (Vector2.Distance(transform.position, pos) > 1f)
        {
            Move(dir);
            yield return null;
        }
    }
}
