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
    public float detectionRange = 5f; 
    public float loseRange = 6f;       
    public float closeRange = 1f;       // trigger attack if within this range

    // Lunge attack
    public float lungeDelay = 0.5f;     // idle before lunge
    public float lungeSpeed = 8f;    
    public float lungeDuration = 0.3f;  

    private bool isAttacking = false;
    private bool isPreparingLunge = false;
    private bool isLunging = false;
    private float delayTimer = 0f;
    private float lungeTimer = 0f;
    private Vector3 lungeDirection;

    // Projectile attack
    public Rigidbody2D bulletPrefab;
    public float bulletSpeed = 10f;
    public float projectileCooldown = 2f;
    private float projectileTimer = 0f;

    // Alt attack flag
    private bool useAltAttack = false;

    // Idle state
    private bool isIdle = false;
    private float idleTimer = 0f;
    public float idleDuration = 2f;

    // Runtime flags for transitions
    private bool isChasing = false;

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

        // Cooldownfor projectile
        if (projectileTimer > 0f)
            projectileTimer -= Time.deltaTime;
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

        // Attack branch
        var attackSequence = new SequenceNode();
        attackSequence.AddNode(isPlayerClose);
        attackSequence.AddNode(attack);

        // Chase branch
        var chaseSequence = new SequenceNode();
        chaseSequence.AddNode(isPlayerDetectable);
        chaseSequence.AddNode(markChasing);
        chaseSequence.AddNode(chase);

        // Lost branch
        var lostSequence = new SequenceNode();
        lostSequence.AddNode(isPlayerLostWhileChasing);
        lostSequence.AddNode(markNotChasing);
        lostSequence.AddNode(idle);

        // Root selector order
        var rootSelector = new SelectorNode();
        rootSelector.AddNode(attackSequence);
        rootSelector.AddNode(chaseSequence);
        rootSelector.AddNode(lostSequence);
        rootSelector.AddNode(patrol);

        return rootSelector;
    }

    // Action / condition nodes

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

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            movingToPointA = !movingToPointA;
        }

        if (PlayerInRange(detectionRange) || PlayerClose())
            return NodeState.Success;

        return NodeState.Running;
    }

    private NodeState ChasePlayer()
    {
        if (player == null) return NodeState.Failure;

        if (PlayerClose())
        {
            return NodeState.Success;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            chaseSpeed * Time.deltaTime
        );

        if (!PlayerInRange(loseRange))
        {
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    private NodeState AttackPlayer()
    {
        // Initialize attack
        if (!isAttacking)
        {
            isAttacking = true;
            isPreparingLunge = true;
            delayTimer = lungeDelay;
            lungeTimer = lungeDuration;

            // 50% chance to use projectile instead of lunge
            useAltAttack = (Random.value < 0.5f);

            if (!useAltAttack && player != null)
            {
                lungeDirection = (player.position - transform.position).normalized;
            }
        }

        // Idle wind-up
        if (isPreparingLunge)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isPreparingLunge = false;

                if (useAltAttack)
                {
                    // Alt attack: projectile
                    if (projectileTimer <= 0f && player != null)
                    {
                        Vector2 dir = (player.position - transform.position).normalized;
                        Rigidbody2D bullet = GameObject.Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                        bullet.linearVelocity = dir * bulletSpeed;

                        projectileTimer = projectileCooldown;
                    }

                    isAttacking = false;
                    return NodeState.Success;
                }
                else
                {
                    // Normal lunge
                    isLunging = true;
                }
            }
            return NodeState.Running;
        }

        // Lunge forward
        if (isLunging)
        {
            lungeTimer -= Time.deltaTime;
            transform.position += lungeDirection * lungeSpeed * Time.deltaTime;

            if (lungeTimer <= 0f)
            {
                isLunging = false;
                isAttacking = false;
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        // Safety reset
        isAttacking = false;
        isLunging = false;
        isPreparingLunge = false;
        return NodeState.Success;
    }

    private NodeState Idle()
    {
        if (!isIdle)
        {
            isIdle = true;
            idleTimer = idleDuration;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            isIdle = false;
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}