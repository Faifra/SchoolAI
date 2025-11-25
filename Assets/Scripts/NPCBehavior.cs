using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    private Node rootNode;
    private Transform player;
    public Transform pointA;
    public Transform pointB;

    private bool movingToPointA = true;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    // Idle state variables
    private bool isIdle = false;
    private float idleTimer = 0f;
    public float idleDuration = 2f; // how long to stand still

    void Awake()
    {
        // Find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found in scene!");
        }
    }

    void Start()
    {
        rootNode = CreateBehaviorTree();
    }

    void Update()
    {
        // Handle idle pause before resuming patrol
        if (isIdle)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdle = false; // finished idling, resume patrol
            }
            return; // stand still during idle
        }

        rootNode.Execute();
    }

    private Node CreateBehaviorTree()
    {
        var chasePlayer = new ActionNode(ChasePlayer);
        var isPlayerInRange = new ConditionNode(() =>
            player != null && Vector3.Distance(transform.position, player.position) < 5f
        );
        var patrol = new ActionNode(Patrol);

        var chaseSequence = new SequenceNode();
        chaseSequence.AddNode(isPlayerInRange);
        chaseSequence.AddNode(chasePlayer);

        var patrolSelector = new SelectorNode();
        patrolSelector.AddNode(chaseSequence);
        patrolSelector.AddNode(patrol);

        var losePlayer = new ActionNode(LosePlayer);

        var losePlayerSequence = new SequenceNode();
        losePlayerSequence.AddNode(isPlayerInRange);
        losePlayerSequence.AddNode(losePlayer);

        return patrolSelector;
    }

    private void Patrol()
    {
        Transform targetPoint = movingToPointA ? pointA : pointB;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint.position,
            patrolSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            movingToPointA = !movingToPointA;
        }
    }

    private void ChasePlayer()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );
    }

    private void LosePlayer()
    {
        player = null;
        isIdle = true;              // enter idle state
        idleTimer = idleDuration;   // reset idle timer
    }
}