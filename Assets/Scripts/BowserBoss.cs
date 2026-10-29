using UnityEngine;

/* Jefe final: Bowser patrulla el puente del castillo y lanza bolas de fuego. */
public class BowserBoss : MonoBehaviour
{
    public float speed = 1.8f;
    public float minX = 155f;
    public float maxX = 170f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Sprite fireballSprite;
    private Transform player;
    private float direction = -1f;
    private float shootTimer = 2.4f;
    private float time;
    private bool defeated;
    private float baseY;

    public void Initialize(Sprite bowserSprite, Sprite fireSprite, Transform[] planks)
    {
        fireballSprite = fireSprite;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = bowserSprite;
        spriteRenderer.sortingOrder = 3;
    }

    public void SetPatrol(float left, float right)
    {
        minX = left;
        maxX = right;
    }

    private void Start()
    {
        baseY = transform.position.y;

        body = GetComponent<Rigidbody2D>();

        if (body == null) {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        if (collider == null) {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.size = new Vector2(1.7f, 1.9f);
        collider.offset = new Vector2(0f, 0.05f);

        player = FindFirstObjectByType<Player>()?.transform;
    }

    private void Update()
    {
        if (defeated) {
            return;
        }

        if (player == null)
        {
            Player found = FindFirstObjectByType<Player>();
            player = found != null ? found.transform : null;

            if (player == null) {
                return;
            }
        }

        time += Time.deltaTime;

        float dx = player.position.x - transform.position.x;
        bool seesPlayer = Mathf.Abs(dx) < 18f;

        if (seesPlayer && Mathf.Abs(dx) > 2.2f) {
            direction = Mathf.Sign(dx);
        }

        float x = Mathf.Clamp(transform.position.x + direction * speed * Time.deltaTime, minX, maxX);
        float y = baseY + Mathf.Abs(Mathf.Sin(time * 5f)) * 0.14f;

        if (body != null) {
            body.MovePosition(new Vector2(x, y));
        } else {
            transform.position = new Vector2(x, y);
        }

        if (spriteRenderer != null) {
            spriteRenderer.flipX = direction > 0f;
        }

        shootTimer -= Time.deltaTime;

        if (seesPlayer && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = 2.1f + Random.value * 1.3f;
        }

        if (Mathf.Abs(dx) < 1.4f && Mathf.Abs(player.position.y - transform.position.y) < 1.6f)
        {
            Player hit = player.GetComponent<Player>();

            if (hit != null) {
                hit.Hit();
            }
        }
    }

    private void Shoot()
    {
        float fireDirection = spriteRenderer != null && spriteRenderer.flipX ? 1f : -1f;

        GameObject ball = new GameObject("Bowser Fireball");
        ball.transform.position = new Vector2(transform.position.x + fireDirection * 1.2f, transform.position.y + 0.45f);

        SpriteRenderer ballRenderer = ball.AddComponent<SpriteRenderer>();
        ballRenderer.sprite = fireballSprite;
        ballRenderer.sortingOrder = 5;

        Rigidbody2D ballBody = ball.AddComponent<Rigidbody2D>();
        ballBody.gravityScale = 0.4f;
        ballBody.linearVelocity = new Vector2(fireDirection * 6.5f, 1.6f);

        CircleCollider2D ballCollider = ball.AddComponent<CircleCollider2D>();
        ballCollider.isTrigger = true;
        ballCollider.radius = 0.38f;

        ball.AddComponent<BowserFireball>();

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Fireball);
        }
    }

    public void FallIntoLava()
    {
        if (defeated) {
            return;
        }

        defeated = true;

        foreach (Collider2D collider in GetComponents<Collider2D>()) {
            collider.enabled = false;
        }

        if (body == null) {
            body = gameObject.AddComponent<Rigidbody2D>();
        }

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 2.8f;
        body.linearVelocity = new Vector2(-1.2f, 3f);
        body.angularVelocity = -160f;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.BossHit);
        }

        Destroy(gameObject, 4f);
    }

}
