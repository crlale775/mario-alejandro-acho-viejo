using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerSpriteRenderer smallRenderer;
    public PlayerSpriteRenderer bigRenderer;
    private PlayerSpriteRenderer activeRenderer;

    public CapsuleCollider2D capsuleCollider { get; private set; }
    public DeathAnimation deathAnimation { get; private set; }

    public bool big => bigRenderer.enabled;
    public bool dead => deathAnimation.enabled;
    public bool starpower { get; private set; }
    public bool invulnerable { get; private set; }

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        deathAnimation = GetComponent<DeathAnimation>();
        activeRenderer = smallRenderer;
    }

    public void Hit()
    {
        if (dead || starpower || invulnerable) {
            return;
        }

        if (big) {
            Shrink();
        } else {
            Death();
        }
    }

    /* Muerte inmediata (lava, caidas, tiempo agotado). */
    public void Kill()
    {
        if (dead) {
            return;
        }

        Death();
    }

    public void Death()
    {
        smallRenderer.enabled = false;
        bigRenderer.enabled = false;
        deathAnimation.enabled = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Death);
        }

        LevelCamera camera = Camera.main != null ? Camera.main.GetComponent<LevelCamera>() : null;

        if (camera != null) {
            camera.Shake(0.25f, 0.6f);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.ResetLevel(3f);
        }
    }

    public void Grow()
    {
        smallRenderer.enabled = false;
        bigRenderer.enabled = true;
        activeRenderer = bigRenderer;

        capsuleCollider.size = new Vector2(1f, 2f);
        capsuleCollider.offset = new Vector2(0f, 0.5f);

        StartCoroutine(ScaleAnimation());
    }

    public void Shrink()
    {
        smallRenderer.enabled = true;
        bigRenderer.enabled = false;
        activeRenderer = smallRenderer;

        capsuleCollider.size = new Vector2(1f, 1f);
        capsuleCollider.offset = new Vector2(0f, 0f);

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Damage);
        }

        StopAllCoroutines();
        StartCoroutine(ScaleAnimation());
        StartCoroutine(InvulnerabilityAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 4 == 0)
            {
                smallRenderer.enabled = !smallRenderer.enabled;
                bigRenderer.enabled = !smallRenderer.enabled;
            }

            yield return null;
        }

        smallRenderer.enabled = false;
        bigRenderer.enabled = false;
        activeRenderer.enabled = true;
    }

    /* Parpadeo posterior al dano para evitar perder dos vidas seguidas. */
    private IEnumerator InvulnerabilityAnimation()
    {
        invulnerable = true;

        float elapsed = 0f;
        float duration = 1.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 6 == 0) {
                activeRenderer.spriteRenderer.enabled = !activeRenderer.spriteRenderer.enabled;
            }

            yield return null;
        }

        activeRenderer.spriteRenderer.enabled = true;
        invulnerable = false;
    }

    public void Starpower()
    {
        StartCoroutine(StarpowerAnimation());
    }

    private IEnumerator StarpowerAnimation()
    {
        starpower = true;

        float elapsed = 0f;
        float duration = 10f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 4 == 0) {
                activeRenderer.spriteRenderer.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            }

            yield return null;
        }

        activeRenderer.spriteRenderer.color = Color.white;
        starpower = false;
    }

}
