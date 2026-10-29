using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DeathBarrier : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySfx(Sfx.Death);
            }

            other.gameObject.SetActive(false);
            GameManager.Instance.ResetLevel(2f);
        }
        else
        {
            Destroy(other.gameObject);
        }
    }

}
