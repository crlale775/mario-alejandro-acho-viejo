using UnityEngine;

/* Barra de fuego giratoria del castillo. */
public class FireBar : MonoBehaviour
{
    public int segments = 5;
    public float speed = 110f;
    public Sprite ballSprite;
    public float radius = 0.85f;

    private Transform[] balls;
    private Player player;
    private float angle;

    private void Start()
    {
        balls = new Transform[segments];

        for (int i = 0; i < segments; i++)
        {
            GameObject ball = new GameObject($"Fire {i}");
            ball.transform.SetParent(transform);
            ball.transform.localScale = Vector3.one * (1f - i * 0.06f);

            SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
            renderer.sprite = ballSprite;
            renderer.sortingOrder = 4;

            balls[i] = ball.transform;
        }

        player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        if (player == null) {
            player = FindFirstObjectByType<Player>();
        }

        angle += speed * Time.deltaTime;
        float radians = angle * Mathf.Deg2Rad;

        for (int i = 0; i < balls.Length; i++)
        {
            float distance = radius * (i + 1);
            Vector3 offset = new Vector3(Mathf.Cos(radians) * distance, Mathf.Sin(radians) * distance, 0f);
            balls[i].localPosition = offset;

            if (player != null && !player.dead)
            {
                if (Vector2.Distance(balls[i].position, player.transform.position) < 0.55f) {
                    player.Hit();
                }
            }
        }
    }

}
