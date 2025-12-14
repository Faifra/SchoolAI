using UnityEngine;

public class DTNPC : MonoBehaviour
{
    private DTNodeBase rootDTNode;
    private Transform dt_player;

    [Header("Ranges")]
    public float dt_projectileRange = 5f;   // shoot if within this range
    public float dt_visionRange = 7f;       // chase range

    [Header("Patrol Settings")]
    public float dt_patrolSpeed = 2f;
    public float dt_patrolDistance = 3f;

    [Header("Projectile Attack")]
    public Rigidbody2D dt_bulletPrefab;
    public float dt_bulletSpeed = 10f;
    public float dt_projectileCooldown = 2f;

    private float dt_projectileTimer = 0f;

    private Vector3 dt_patrolStartPos;
    private Vector3 dt_patrolTarget;

    void Awake()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
            dt_player = foundPlayer.transform;
        else
            Debug.LogError("DTNPC: Player with tag 'Player' not found!");
    }

    void Start()
    {
        dt_patrolStartPos = transform.position;
        dt_patrolTarget = dt_patrolStartPos + Vector3.right * dt_patrolDistance;

        var projectileNode = new DTActionNode(DoProjectileAttack);
        var chaseNode = new DTActionNode(DoChaseAction);
        var patrolNode = new DTActionNode(DoPatrolAction);

 
        var chaseDecision = new DTDecisionNode(
            () => IsPlayerWithinRange(dt_visionRange),
            chaseNode,
            patrolNode
        );

        var attackDecision = new DTDecisionNode(
            () => IsPlayerWithinRange(dt_projectileRange),
            projectileNode,
            chaseDecision
        );

        rootDTNode = attackDecision;
    }

    void Update()
    {
        rootDTNode.Execute();

        if (dt_projectileTimer > 0f)
            dt_projectileTimer -= Time.deltaTime;
    }

    private bool IsPlayerWithinRange(float range)
    {
        if (dt_player == null) return false;
        return Vector3.Distance(transform.position, dt_player.position) <= range;
    }

    private void DoProjectileAttack()
    {
        Debug.Log("DTNPC: Projectile Attack");

        if (dt_player == null || dt_projectileTimer > 0f)
            return;

        Vector2 dir = (dt_player.position - transform.position).normalized;

        Rigidbody2D bullet =
            Instantiate(dt_bulletPrefab, transform.position, Quaternion.identity);

        bullet.linearVelocity = dir * dt_bulletSpeed;

        dt_projectileTimer = dt_projectileCooldown;
    }

    private void DoChaseAction()
    {
        Debug.Log("DTNPC: Chase");

        if (dt_player == null) return;

        Vector3 direction = (dt_player.position - transform.position).normalized;
        transform.Translate(direction * 3f * Time.deltaTime, Space.World);
    }

    private void DoPatrolAction()
    {
        Debug.Log("DTNPC: Patrol");

        transform.position = Vector3.MoveTowards(
            transform.position,
            dt_patrolTarget,
            dt_patrolSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, dt_patrolTarget) < 0.05f)
        {
            dt_patrolTarget =
                (dt_patrolTarget == dt_patrolStartPos)
                ? dt_patrolStartPos + Vector3.right * dt_patrolDistance
                : dt_patrolStartPos;
        }
    }
}