using UnityEngine;

public class ItemOnGround : MonoBehaviour
{
    public float amplitude = 0.1f;  // How high it floats
    public float frequency = 2.5f;    // How fast it floats
    public float rotationSpeed = 50f; // Speed of rotation

    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0, yOffset, 0);

        // Optional: Rotate the item slightly for a more dynamic effect
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0); // Rotate around the Y-axis

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("onTriggerEnter called with: " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            // Logic to pick up the item can be added here
            Debug.Log("Item picked up: " + gameObject.name);
            Destroy(gameObject); // Remove the item from the scene
        }
        else
        {
            Debug.Log("Item collided with: " + other.gameObject.name);
        }
    }
}
