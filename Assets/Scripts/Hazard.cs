using UnityEngine;

/* Zona de dano: lava, pinchos o agua hirviendo. */
public class Hazard : MonoBehaviour
{
    public bool instantKill = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Touch(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Touch(other);
    }

    private void Touch(Collider2D other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player == null || player.dead) {
            return;
        }

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Bump);
        }

        if (instantKill) {
            player.Kill();
        } else {
            player.Hit();
        }
    }

}
