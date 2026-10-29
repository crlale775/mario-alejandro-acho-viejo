using UnityEngine;

public class BowserFireball : MonoBehaviour
{
    private float life = 5f;
    private float rotationSpeed = 480f;

    private void Update()
    {
        life -= Time.deltaTime;

        if (life <= 0f) {
            Destroy(gameObject);
            return;
        }

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player != null) {
            player.Hit();
        }

        Destroy(gameObject);
    }

}
