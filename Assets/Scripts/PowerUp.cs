using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type
    {
        Coin,
        ExtraLife,
        MagicMushroom,
        Starpower,
    }

    public Type type;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            Collect(other.gameObject);
        }
    }

    private void Collect(GameObject player)
    {
        switch (type)
        {
            case Type.Coin:
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySfx(Sfx.Coin);
                }

                GameManager.Instance.AddCoin();
                break;

            case Type.ExtraLife:
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySfx(Sfx.OneUp);
                }

                GameManager.Instance.AddLife();
                break;

            case Type.MagicMushroom:
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySfx(Sfx.PowerUp);
                }

                GameManager.Instance.AddScore(1000);
                player.GetComponent<Player>().Grow();
                break;

            case Type.Starpower:
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.PlaySfx(Sfx.PowerUp);
                }

                GameManager.Instance.AddScore(1000);
                player.GetComponent<Player>().Starpower();
                break;
        }

        Destroy(gameObject);
    }

}
