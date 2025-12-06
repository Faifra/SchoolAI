using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    private Node rootNode;
    private Transform player;
    public Transform pointA;
    public Transform pointB;
    private bool movingToPointA = true;

    // Speeds
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    private bool facingRight = true;

    // Ranges
    public float detectionRange = 5f;   // begin chasing if within this range
    public float loseRange = 6f;        // consider player lost if beyond this range


    // Attacks
    public float closeRange = 1f;

    private bool isAttacking = false;

    private float attackTimer = 0f;

    public float attackDuration = 2f; 

    // Idle state
    private bool isIdle = false;
    private float idleTimer = 0f;
    public float idleDuration = 2f;


    // Runtime flags for transitions
    private bool isChasing = false;     // true only while actively chasing

    // Track last position for movement direction
    private Vector3 lastPosition;

    void Awake()
    {
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
        lastPosition = transform.position;
    }

    void Update()
    {
        rootNode.Execute();

        // Flip the enemy based on horizontal movement
        Vector3 movement = transform.position - lastPosition;
        if (movement.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (movement.x < 0 && facingRight)
        {
            Flip();
        }

        lastPosition = transform.position;
    }

    private Node CreateBehaviorTree()
    {
        var patrol = new ActionNode(Patrol);
        var chase = new ActionNode(ChasePlayer);
        var idle = new ActionNode(Idle);
        var markChasing = new ActionNode(() => { isChasing = true; return NodeState.Success; });
        var markNotChasing = new ActionNode(() => { isChasing = false; return NodeState.Success; });

        var isPlayerDetectable = new ConditionNode(() => PlayerInRange(detectionRange));
        var isPlayerClose = new ConditionNode(() => PlayerClose());
        var isPlayerLostWhileChasing = new ConditionNode(() => isChasing && !PlayerInRange(loseRange));

        var attack = new ActionNode(AttackPlayer);

        // Attack branch: if player is close -> attack
        var attackSequence = new SequenceNode();
        attackSequence.AddNode(isPlayerClose);
        attackSequence.AddNode(attack);

        // Chase branch: if detectable -> markChasing -> chase
        var chaseSequence = new SequenceNode();
        chaseSequence.AddNode(isPlayerDetectable);
        chaseSequence.AddNode(markChasing);
        chaseSequence.AddNode(chase);

        // Lost branch: if was chasing and now lost -> markNotChasing -> idle -> success
        var lostSequence = new SequenceNode();
        lostSequence.AddNode(isPlayerLostWhileChasing);
        lostSequence.AddNode(markNotChasing);
        lostSequence.AddNode(idle);

        // Root selector order:
        // 1) Attack if close
        // 2) Chase if player detected
        // 3) If player was lost: idle
        // 4) Otherwise patrol
        var rootSelector = new SelectorNode();
        rootSelector.AddNode(attackSequence);
        rootSelector.AddNode(chaseSequence);
        rootSelector.AddNode(lostSequence);
        rootSelector.AddNode(patrol);

        return rootSelector;
    }

    // -------------------
    // Action / condition nodes
    // -------------------

    private bool PlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    private bool PlayerClose()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= closeRange;
    }

private NodeState Patrol()
{
    Transform targetPoint = movingToPointA ? pointA : pointB;
    if (targetPoint == null) return NodeState.Failure;

    transform.position = Vector3.MoveTowards(
        transform.position,
        targetPoint.position,
        patrolSpeed * Time.deltaTime
    );

    // Flip target when reaching the point
    if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
    {
        movingToPointA = !movingToPointA;
    }

    // If player is detectable, allow selector to preempt
    if (PlayerInRange(detectionRange) || PlayerClose())
        return NodeState.Success;

    return NodeState.Running;
}

    private NodeState ChasePlayer()
    {
        if (player == null) return NodeState.Failure;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );

        // Keep running while chasing; the selector will preempt if out of range
        return NodeState.Running;
    }

private NodeState AttackPlayer()
{
    // Initialize attack only once when entering the node
    if (!isAttacking)
    {
        isAttacking = true;
        attackTimer = attackDuration;
    }

    attackTimer -= Time.deltaTime;

    if (attackTimer <= 0f)
    {
        isAttacking = false;
        return NodeState.Success; // attack complete; selector falls back to patrol/chase
    }

    // Stop moving as a first version of attack
    return NodeState.Running;
}

    private NodeState Idle()
    {
        // Initialize idle only once when entering the node
        if (!isIdle)
        {
            isIdle = true;
            idleTimer = idleDuration;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            isIdle = false;
            return NodeState.Success; // idle complete; selector falls back to patrol
        }

        return NodeState.Running;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Flip the X axis
        transform.localScale = scale;
    }
}
