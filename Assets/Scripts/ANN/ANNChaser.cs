using UnityEngine;

public class ANNChaser : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    ANN brain;

    void Start()
    {
        brain = new ANN(2, 4, 2); // 2 inputs, 4 hidden, 2 outputs
    }

    void Update()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        float dy = player.position.y - transform.position.y;

        float[] inputs = new float[] { dx, dy };
        float[] outputs = brain.Forward(inputs);

        Vector2 move = new Vector2(outputs[0], outputs[1]).normalized;

        transform.position += (Vector3)move * speed * Time.deltaTime;
    }
}