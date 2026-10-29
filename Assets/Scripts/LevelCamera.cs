using UnityEngine;

/* Camara lateral con seguimiento suave, adelanto de mirada y limites de nivel. */
[RequireComponent(typeof(Camera))]
public class LevelCamera : MonoBehaviour
{
    public Transform target;
    public float levelStart = 0f;
    public float levelEnd = 200f;
    public float fixedHeight = 6.5f;
    public float lookAhead = 2.5f;
    public float smoothTime = 0.18f;

    private new Camera camera;
    private Vector3 currentVelocity;
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeTimer;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Start()
    {
        SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (target == null) {
            return;
        }

        float halfWidth = HalfWidth();
        float x = Mathf.Clamp(target.position.x + lookAhead, levelStart + halfWidth, levelEnd - halfWidth);
        transform.position = new Vector3(x, fixedHeight, -10f);
    }

    private float HalfWidth()
    {
        return camera == null ? 12.5f : camera.orthographicSize * camera.aspect;
    }

    private void LateUpdate()
    {
        if (target == null) {
            return;
        }

        float halfWidth = HalfWidth();
        float minX = levelStart + halfWidth;
        float maxX = levelEnd - halfWidth;

        if (maxX < minX) {
            maxX = minX;
        }

        float desiredX = Mathf.Clamp(target.position.x + lookAhead, minX, maxX);
        Vector3 desired = new Vector3(desiredX, fixedHeight, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref currentVelocity, smoothTime);

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float falloff = Mathf.Clamp01(shakeTimer / Mathf.Max(0.01f, shakeDuration));
            Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * falloff);
            transform.position += new Vector3(offset.x, offset.y, 0f);
        }
    }

    public void Shake(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        shakeDuration = duration;
        shakeTimer = duration;
    }

}
