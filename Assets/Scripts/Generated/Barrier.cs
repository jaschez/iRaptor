using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    bool blocked = false;

    SpriteRenderer spriteRenderer;

    Color originalColor;
    Color blockedColor = Color.gray;

    protected void Start()
    {

        if (!TryGetComponent(out spriteRenderer))
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        originalColor = spriteRenderer.color;
    }

    public void Block(bool blocked)
    {
        this.blocked = blocked;

        if (blocked)
        {
            spriteRenderer.color = blockedColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Bullet"))
        {

            if (!blocked)
            {
                other.gameObject.SetActive(false);
                StartCoroutine(DamageFlash(.07f));
            }
            else
            {
                spriteRenderer.color = blockedColor;
                other.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator DamageFlash(float flashTime)
    {
        Material spriteMat = spriteRenderer.material;

        spriteRenderer.material = null;
        spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(flashTime);

        CamManager.GetInstance().ShakeQuake(4, 2.5f, false);
        SoundManager.Play(Sounds.Break, transform.position);

        spriteRenderer.material = spriteMat;
        spriteRenderer.color = originalColor;
        
        gameObject.SetActive(false);

    }
}
