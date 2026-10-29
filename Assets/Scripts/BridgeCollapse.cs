using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Puente del castillo: al golpear el hacha los tablones caen y Bowser termina en la lava. */
public class BridgeCollapse : MonoBehaviour
{
    public readonly List<GameObject> planks = new List<GameObject>();
    public BowserBoss bowser;
    public LevelCamera levelCamera;

    private bool collapsed;

    public void AddPlank(GameObject plank)
    {
        if (plank != null) {
            planks.Add(plank);
        }
    }

    public void Collapse()
    {
        if (collapsed) {
            return;
        }

        collapsed = true;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        for (int i = planks.Count - 1; i >= 0; i--)
        {
            GameObject plank = planks[i];

            if (plank == null) {
                continue;
            }

            Rigidbody2D body = plank.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 2.6f;
            body.linearVelocity = new Vector2(Random.Range(-0.8f, 0.8f), 1.2f);
            body.angularVelocity = Random.Range(-160f, 160f);

            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySfx(Sfx.Bump);
            }

            yield return new WaitForSeconds(0.055f);
        }

        if (bowser != null) {
            bowser.FallIntoLava();
        }

        if (levelCamera != null) {
            levelCamera.Shake(0.6f, 1.6f);
        }

        yield return new WaitForSeconds(2.6f);

        if (GameManager.Instance != null) {
            GameManager.Instance.Victory();
        }
    }

}
