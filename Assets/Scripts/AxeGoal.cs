using UnityEngine;

/* Hacha del castillo: al tocarla se derrumba el puente. */
public class AxeGoal : MonoBehaviour
{
    public BridgeCollapse bridge;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) {
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player != null && player.dead) {
            return;
        }

        used = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Axe);
        }

        if (bridge != null) {
            bridge.Collapse();
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        if (renderer != null) {
            renderer.enabled = false;
        }
    }

}
