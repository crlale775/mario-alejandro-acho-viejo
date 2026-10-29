using UnityEngine;

/* Pez enemigo del nivel acuatico: patrulla en horizontal y ondula en vertical. */
public class CheepCheep : MonoBehaviour
{
    public float speed = 2f;
    public float bobAmplitude = 0.9f;
    public float bobFrequency = 1.4f;

    private SpriteRenderer spriteRenderer;
    private Sprite frameA;
    private Sprite frameB;
    private Rigidbody2D body;
    private float minX;
    private float maxX;
    private float baseY;
    private float direction = -1f;
    private float time;
    private bool dead;

    public void Initialize(Sprite first, Sprite second)
    {
        frameA = first;
        frameB = second;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) {
            spriteRenderer.sprite = frameA;
        }
    }

    public void SetPatrol(float left, float right, float moveSpeed)
    {
        minX = left;
        maxX = right;
        speed = moveSpeed;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        baseY = transform.position.y;
    }

    private void FixedUpdate()
    {
        if (dead) {
            return;
        }

        if (maxX - minX < 0.1f)
        {
            minX = transform.position.x - 4f;
            maxX = transform.position.x + 4f;
        }

        float x = transform.position.x + direction * speed * Time.fixedDeltaTime;

        if (x < minX) {
            x = minX;
            direction = 1f;
        } else if (x > maxX) {
            x = maxX;
            direction = -1f;
        }

        time += Time.fixedDeltaTime;
        float y = baseY + Mathf.Sin(time * bobFrequency) * bobAmplitude;

        if (body != null) {
            body.MovePosition(new Vector2(x, y));
        } else {
            transform.position = new Vector2(x, y);
        }
    }

    private void Update()
    {
        if (spriteRenderer == null || frameA == null || frameB == null) {
            return;
        }

        spriteRenderer.sprite = Mathf.Repeat(time * 4f, 1f) < 0.5f ? frameA : frameB;
        spriteRenderer.flipX = direction > 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead || !collision.gameObject.CompareTag("Player")) {
            return;
        }

        Player player = collision.gameObject.GetComponent<Player>();

        if (player == null) {
            return;
        }

        if (player.starpower) {
            Hit(false, player.transform.position);
        } else if (collision.transform.DotTest(transform, Vector2.down)) {
            player.GetComponent<PlayerMovement>().Bounce();
            Hit(true, player.transform.position);
        } else {
            player.Hit();
        }
    }

    public void Hit(bool stomped, Vector3 from)
    {
        if (dead) {
            return;
        }

        dead = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(stomped ? Sfx.Stomp : Sfx.BossHit);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.AddScore(stomped ? 200 : 100);
        }

        if (GetComponent<Collider2D>() != null) {
            GetComponent<Collider2D>().enabled = false;
        }

        if (spriteRenderer != null) {
            spriteRenderer.flipY = true;
        }

        transform.localScale = Vector3.one * 0.6f;
        Destroy(gameObject, 0.5f);
    }

}
