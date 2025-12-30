using UnityEngine;
using System.Collections.Generic;

public class Trainer : MonoBehaviour
{
    public Transform player;          
    public Agent agentPrefab;         
    public int populationSize = 30;
    public float episodeTime = 5f;

    List<ANN> population = new List<ANN>();
    List<Agent> agents = new List<Agent>();

    float timer = 0f;

    void Start()
    {
        // Create initial population
        for (int i = 0; i < populationSize; i++)
            population.Add(new ANN(2, 4, 2));

        StartEpisode();
    }

    void StartEpisode()
    {
        timer = 0f;

        // Destroy old agents
        foreach (var a in agents)
            Destroy(a.gameObject);
        agents.Clear();

        Transform p = player;

        // Spawn agents
        for (int i = 0; i < populationSize; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                0
            );

            Agent agent = Instantiate(agentPrefab, pos, Quaternion.identity);
            agent.Init(population[i], p);
            agents.Add(agent);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Step all agents
        foreach (var agent in agents)
            agent.Step();

        // Episode finished
        if (timer >= episodeTime)
        {
            EvolvePopulation();
            StartEpisode();
        }
    }

    void EvolvePopulation()
    {
        // Score fitness
        List<(ANN brain, float fitness)> scored = new List<(ANN, float)>();

        for (int i = 0; i < populationSize; i++)
            scored.Add((population[i], agents[i].GetFitness()));

        // Sort by fitness (higher is better)
        scored.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        // Keep top 20%
        int survivors = populationSize / 5;
        List<ANN> newPop = new List<ANN>();

        for (int i = 0; i < survivors; i++)
            newPop.Add(scored[i].brain);

        // Fill rest with mutated children
        while (newPop.Count < populationSize)
        {
            ANN parent = newPop[Random.Range(0, survivors)];
            ANN child = Mutate(parent);
            newPop.Add(child);
        }

        population = newPop;
    }

    ANN Mutate(ANN parent)
    {
        ANN child = new ANN(parent.inputCount, parent.hiddenCount, parent.outputCount);

        // Mutate w1
        for (int i = 0; i < parent.inputCount; i++)
            for (int j = 0; j < parent.hiddenCount; j++)
                child.w1[i, j] = parent.w1[i, j] + Random.Range(-0.2f, 0.2f);

        // Mutate w2
        for (int i = 0; i < parent.hiddenCount; i++)
            for (int j = 0; j < parent.outputCount; j++)
                child.w2[i, j] = parent.w2[i, j] + Random.Range(-0.2f, 0.2f);

        return child;
    }
}