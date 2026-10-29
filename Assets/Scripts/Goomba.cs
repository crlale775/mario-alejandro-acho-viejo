using UnityEngine;

public class Goomba : MonoBehaviour
{
    public Sprite flatSprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (player.starpower) {
                Hit(true);
            } else if (collision.transform.DotTest(transform, Vector2.down)) {
                Flatten();
            } else {
                player.Hit();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shell")) {
            Hit(true);
        }
    }

    private void Flatten()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f);

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Stomp);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.AddScore(100);
        }
    }

    private void Hit(bool scored)
    {
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Stomp);
        }

        if (scored && GameManager.Instance != null) {
            GameManager.Instance.AddScore(200);
        }
    }

}
