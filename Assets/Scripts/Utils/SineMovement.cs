using UnityEngine;

public class SineMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float amplitude = 1f;

    [SerializeField] private bool _horizontal = true;
    [SerializeField] private bool _vertical = false;

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        float x = initialPosition.x + Mathf.Sin(Time.time * speed) * amplitude;
        float y = initialPosition.y + Mathf.Sin(Time.time * speed) * amplitude;

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
