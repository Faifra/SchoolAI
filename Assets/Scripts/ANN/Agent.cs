using UnityEngine;

public class Agent : MonoBehaviour
{
    public ANN brain;
    public Transform player;
    public float speed = 3f;

    float totalDistance = 0f;
    int steps = 0;

    public void Init(ANN brain, Transform player)
    {
        this.brain = brain;
        this.player = player;
    }

    public void Step()
    {
        float dx = player.position.x - transform.position.x;
        float dy = player.position.y - transform.position.y;

        float[] inputs = new float[] { dx, dy };
        float[] outputs = brain.Forward(inputs);

        Vector2 move = new Vector2(outputs[0], outputs[1]).normalized;
        transform.position += (Vector3)move * speed * Time.deltaTime;

        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        totalDistance += dist;
        steps++;
    }

    public float GetFitness()
    {
        return -(totalDistance / steps);
    }
}