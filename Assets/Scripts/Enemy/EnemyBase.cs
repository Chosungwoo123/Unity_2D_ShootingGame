using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageNumbersPro;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    #region 기본 스탯

    [Space(10)]
    [Header("기본 스탯")]
    [SerializeField] private float rotSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveMultiply;
    [SerializeField] private float maxHealth;

    [SerializeField] private int maxAdjustmentMoveSpeed;
    [SerializeField] private int minAdjustmentMoveSpeed;
    
    [Tooltip("플레이어 쪽을 바라보는지 체크하는 변수")]
    [SerializeField] private bool isRotation;

    [Tooltip("플레이어에 닿으면 자폭하는지 체크하는 변수")]
    [SerializeField] private bool isSelfDestruct;

    #endregion

    #region 이펙트 관련

    [Space(10)]
    [Header("이펙트 관련")]
    public GameObject[] dieEffects;
    public GameObject smokeEffect;

    #endregion

    #region 공격 관련 스탯

    [Space(10)]
    [Header("공격 관련 스탯")]
    [SerializeField] protected float attackRate;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected float contactDamage;

    #endregion

    #region 머테리얼 관련

    [Space(10)]
    [Header("머테리얼 관련")]
    public Material hitMaterial;
    public Material nomalMaterial;

    #endregion

    #region UI 관련

    [Space(10)]
    [Header("UI 관련")]
    [SerializeField] private DamageNumber damagePopup;
    [SerializeField] private DamageNumber scorePopup;

    #endregion

    #region 스코어

    [Space(10)]
    [Header("스코어")]
    [SerializeField] private int dieScore;

    #endregion

    protected bool isAttack;

    protected float attackTimer;

    private bool isDie;

    private float curHealth;

    private Rigidbody2D rigid;
    private SpriteRenderer sr;

    private WaitForSeconds hitDelay;

    private Vector2 moveVec;

    protected Vector3 targetPos;

    protected virtual void Start()
    {
        // 변수 초기화
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        moveSpeed += Random.Range(maxAdjustmentMoveSpeed, maxAdjustmentMoveSpeed);

        curHealth = maxHealth;

        hitDelay = new WaitForSeconds(0.05f);
    }

    protected virtual void Update()
    {
        if (isDie)
        {
            // 플레이어 쪽 말고 진행방향 쪽을 바라봄
            if (isRotation)
            {
                Vector2 dir = rigid.velocity.normalized;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
            
            return;
        }

        RotationUpdate();
        MoveUpdate();
        AttackUpdate();
        TargetUpdate();
    }

    private void RotationUpdate()
    {
        // 플레이어 방향 바라보기
        if (!isRotation)
        {
            return;
        }

        Vector2 playerDir = targetPos - transform.position;

        float angle = Mathf.Atan2(playerDir.y, playerDir.x) * Mathf.Rad2Deg;

        Quaternion dirRot = Quaternion.Euler(0, 0, angle - 90);

        this.transform.rotation = Quaternion.Slerp(transform.rotation, dirRot, Time.deltaTime * rotSpeed);
    }

    private void MoveUpdate()
    {
        if (isRotation)
        {
            moveVec.x = Mathf.Lerp(rigid.velocity.x, transform.up.x * moveSpeed, Time.deltaTime * moveMultiply);
            moveVec.y = Mathf.Lerp(rigid.velocity.y, transform.up.y * moveSpeed, Time.deltaTime * moveMultiply);
        }
        else
        {
            Vector2 dir = GameManager.Instance.curPlayer.transform.position - transform.position;

            dir.Normalize();

            moveVec.x = Mathf.Lerp(rigid.velocity.x, dir.x * moveSpeed, Time.deltaTime * moveMultiply);
            moveVec.y = Mathf.Lerp(rigid.velocity.y, dir.y * moveSpeed, Time.deltaTime * moveMultiply);
        }

        rigid.velocity = moveVec;
    }

    protected virtual void AttackUpdate()
    {
        if (isAttack)
        {
            return;
        }

        if (attackTimer >= attackRate && !isAttack)
        {
            ShootBullet();
            attackTimer = 0;
        }

        attackTimer += Time.deltaTime;
    }

    protected virtual void ShootBullet()
    {
        return;
    }

    protected virtual void TargetUpdate()
    {
        targetPos = GameManager.Instance.curPlayer.transform.position;
    }

    public void OnDamage(float damage)
    {
        if (isDie)
        {
            return;
        }

        curHealth -= damage;

        if (curHealth <= 0)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        StartCoroutine(HitRoutine(damage));
    }

    private IEnumerator DieRoutine()
    {
        smokeEffect.SetActive(true);
        gameObject.layer = 7;

        isDie = true;

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < dieEffects.Length; i++)
        {
            Instantiate(dieEffects[i], transform.position, Quaternion.identity);
        }

        GameManager.Instance.CameraShake(30, 0.1f);
        GameManager.Instance.PlusScore(dieScore);

        scorePopup.Spawn(transform.position, dieScore);

        Destroy(gameObject);
    }

    private IEnumerator HitRoutine(float damageAmount)
    {
        // 반짝 거리는 애니메이션
        sr.material = hitMaterial;

        // 데미지 팝업
        damagePopup.Spawn(transform.position + (Vector3)Random.insideUnitCircle, damageAmount);

        yield return hitDelay;

        sr.material = nomalMaterial;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isSelfDestruct)
        {
            // 죽는 로직
            for (int i = 0; i < dieEffects.Length; i++)
            {
                Instantiate(dieEffects[i], transform.position, Quaternion.identity);
            }

            // 플레이어 데미지 주기

            collision.GetComponent<Player>().OnDamage(contactDamage);

            Destroy(gameObject);
        }
    }
}