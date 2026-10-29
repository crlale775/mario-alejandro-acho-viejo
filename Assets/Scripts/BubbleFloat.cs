using UnityEngine;

/* Burbuja decorativa del nivel acuatico. */
public class BubbleFloat : MonoBehaviour
{
    public float minX = 0f;
    public float maxX = 100f;
    public float minY = 0f;
    public float maxY = 10f;
    public bool randomizeStart = true;

    private float speed;
    private float wobble;
    private float seed;

    private void Start()
    {
        speed = Random.Range(0.6f, 1.6f);
        wobble = Random.Range(0.4f, 1.1f);
        seed = Random.value * 10f;

        if (randomizeStart) {
            transform.position = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
        }

        float scale = Random.Range(0.6f, 1.2f);
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void Update()
    {
        Vector3 position = transform.position;
        position.y += speed * Time.deltaTime;
        position.x += Mathf.Sin(Time.time * wobble + seed) * 0.4f * Time.deltaTime;

        if (position.y > maxY) {
            position.y = minY;
            position.x = Random.Range(minX, maxX);
        }

        transform.position = position;
    }

}
