using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    [Header("Stats & Health")]
    public int maxHealth = 100;
    private int currentHealth;
    public bool isDead = false;

    [Header("Movement & Chase Settings")]
    public float chaseSpeed = 3.8f;
    public float rageSpeed = 5.5f;
    public float stoppingDistance = 3.2f;       // รักษาระยะห่างตอนยืน ไม่ให้เดินเบียดเข้าหน้า
    public float updatePathInterval = 0.2f;
    private float nextPathUpdateTime = 0f;

    [Header("Combat Settings")]
    public float attackRange = 4.2f;            // ระยะง้างโจมตี (ระยะไกลสมส่วนกับแขนยาว)
    public float attackDuration = 1.6f;         // ระยะเวลาเล่นท่า Attack จนจบ (ล็อกให้อยู่กับที่)
    public float attackCooldown = 2.0f;
    private float nextAttackTime = 0f;

    [Header("Hit & Rage Durations")]
    public float hitDuration = 0.8f;            // ระยะเวลาสะดุ้ง GetHit จนจบ (หยุดเดินชะงัก)
    public float rageDuration = 1.8f;           // ระยะเวลาคำราม Rage จนจบ
    public float rageInterval = 14f;
    private float nextRageTime = 7f;

    [Header("Action Locks")]
    private bool isBusy = false;                // ป้องกันไม่ให้เดินหรือไถลขณะเล่นท่าใดๆ ค้างอยู่

    [Header("Strafe Movement")]
    public float strafeSwitchInterval = 3f;
    private float nextStrafeTime = 0f;
    private int currentMoveType = 0;            // 0 = Run, 1 = StrafeLeft, 2 = StrafeRight

    [Header("Target & Animation")]
    public string playerTag = "Player";
    public Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;

        FindPlayer();
    }

    void Update()
    {
        // ถ้าตาย หรือกำลังเล่นท่า (ตี, โดนยิง, คำราม) ให้หยุดอัปเดตการเดินทุกอย่าง
        if (isDead || isBusy) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // เมื่อเข้าใกล้ระยะตี และพร้อมโจมตี
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttackRoutine());
        }
        else
        {
            HandleChaseMovement();
            HandleRageBehavior();
            HandleStrafeBehavior();
        }

        UpdateAnimation();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
        }
    }

    void HandleChaseMovement()
    {
        if (Time.time >= nextPathUpdateTime)
        {
            nextPathUpdateTime = Time.time + updatePathInterval;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
    }

    // Coroutine สั่งล็อกตัวให้อยู่กับที่ตอนง้างตีจนเสร็จ
    IEnumerator PerformAttackRoutine()
    {
        isBusy = true;
        nextAttackTime = Time.time + attackCooldown;

        // สั่งหยุดเดิน นิ่งอยู่กับที่ทันที ไม่ให้ไถลตาม
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // หันหน้ามองผู้เล่นก่อนเริ่มฟาด
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Attack");
        }

        // รอจนกว่าท่าโจมตีจะเล่นจบ
        yield return new WaitForSeconds(attackDuration);

        if (agent.isOnNavMesh && !isDead)
        {
            agent.isStopped = false;
        }

        isBusy = false;
    }

    // Coroutine สั่งให้คำราม Rage จนจบ
    IEnumerator PerformRageRoutine()
    {
        isBusy = true;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Rage");
        }

        yield return new WaitForSeconds(rageDuration);

        agent.speed = rageSpeed;
        if (agent.isOnNavMesh && !isDead)
        {
            agent.isStopped = false;
        }

        isBusy = false;
    }

    // Coroutine สั่งชะงักหยุดเดินตอนโดนกระสุนยิง (GetHit)
    IEnumerator PerformGetHitRoutine()
    {
        isBusy = true;

        // หยุดชะงักทันทีที่โดนยิง
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("GetHit");
        }

        // รอจนกว่าจะสะดุ้งจบ
        yield return new WaitForSeconds(hitDuration);

        if (agent.isOnNavMesh && !isDead)
        {
            agent.isStopped = false;
        }

        isBusy = false;
    }

    void HandleRageBehavior()
    {
        if (Time.time >= nextRageTime)
        {
            nextRageTime = Time.time + rageInterval;
            StartCoroutine(PerformRageRoutine());
        }
    }

    void HandleStrafeBehavior()
    {
        if (Time.time >= nextStrafeTime)
        {
            nextStrafeTime = Time.time + strafeSwitchInterval;

            float roll = Random.value;
            if (roll < 0.7f) currentMoveType = 0;
            else if (roll < 0.85f) currentMoveType = 1;
            else currentMoveType = 2;

            if (animator != null)
            {
                animator.SetInteger("MoveType", currentMoveType);
                if (currentMoveType == 0)
                {
                    animator.SetInteger("RunIndex", Random.Range(0, 3));
                }
            }
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.2f;
        animator.SetBool("IsMoving", isMoving);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // หยุดการกระทำเดิม แล้วเล่นท่าสะดุ้งทันที
            StopAllCoroutines();
            StartCoroutine(PerformGetHitRoutine());
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
        agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 4f);
    }
}