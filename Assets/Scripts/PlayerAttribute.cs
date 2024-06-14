using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttribute : MonoBehaviour
{
    public int HP = 10;
    public int moveSpeed = 5;
    public int jumpSpeed = 16;
    public bool hasWon = false;

    public bool isInvincible = false;
    private float invincibilityDuration = 0.6f;
    private float flashInterval = 0.2f;
    private float knockbackForce = 3f;

    public SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = transform.Find("Body").GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        originalColor = spriteRenderer.color;
    }

    public void ChangeHP(int value, Vector2 knockBackDirection)
    {
        if (!isInvincible)
        {
            HP += value;
            if (HP <= 0)
            {
                HP = 0;
            }
            StartCoroutine(Flash());
            KnockBack(knockBackDirection);
        }

    }

    private IEnumerator Flash()
    {
        isInvincible = true;
        rb.velocity = Vector2.zero;

        for (float i = 0; i < invincibilityDuration; i += flashInterval)
        {
            Color newColor = spriteRenderer.color;
            newColor.a = newColor.a == 1f ? 0.2f : 1f; // Toggle between 100% and 20% opacity
            spriteRenderer.color = newColor;
            yield return new WaitForSeconds(flashInterval);
        }
        spriteRenderer.color = originalColor;  // Ensure sprite is fully visible after flashing

        isInvincible = false;
        rb.velocity = Vector2.zero;
    }

    private void KnockBack(Vector2 direction)
    {
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }
}
