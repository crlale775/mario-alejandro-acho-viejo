using System.Collections;
using UnityEngine;

/* Tuberia de salida: al pulsar abajo el jugador se introduce y termina el nivel. */
public class LevelExitPipe : MonoBehaviour
{
    private bool used;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) {
            return;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            used = true;
            StartCoroutine(Enter(other.gameObject));
        }
    }

    private IEnumerator Enter(GameObject playerObject)
    {
        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();
        Player player = playerObject.GetComponent<Player>();

        if (movement != null) {
            movement.enabled = false;
        }

        if (player != null && player.dead) {
            yield break;
        }

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.PowerUp);
        }

        Vector3 start = playerObject.transform.position;
        Vector3 end = transform.position + Vector3.down * 0.5f;
        Vector3 startScale = playerObject.transform.localScale;

        float elapsed = 0f;
        float duration = 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            playerObject.transform.position = Vector3.Lerp(start, end, t);
            playerObject.transform.localScale = Vector3.Lerp(startScale, new Vector3(0.4f, 0.4f, 1f), t);

            yield return null;
        }

        playerObject.SetActive(false);

        if (GameManager.Instance != null) {
            GameManager.Instance.CompleteLevel();
        }
    }

}
