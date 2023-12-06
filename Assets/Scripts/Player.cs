using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float rotSpeed;
    public float moveSpeed;
    public float moveMultiply;
    public float fireRate;
    public float dashCoolTime;
    public float dashPower;
    public float dashTime;

    public Transform shotPos;

    public PlayerBullet bulletPrefab;
    public GameObject dashEffectPrefab;

    public ParticleSystem trailParticle;

    private float fireTimer;
    private float dashTimer;

    private bool isMoveStop;

    private Rigidbody2D rigid;

    private WaitForSeconds dashTimeWaitForSeconds;

    private Vector2 moveVec;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();

        dashTimeWaitForSeconds = new WaitForSeconds(dashTime);
    }

    private void Update()
    {
        RotationUpdate();
        MoveUpdate();
        AttackUpdate();
        DashUpdate();
    }

    private void RotationUpdate()
    {
        Vector2 mouseDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

        float angle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

        Quaternion dirRot = Quaternion.Euler(0, 0, angle - 90);

        this.transform.rotation = Quaternion.Slerp(transform.rotation, dirRot, Time.deltaTime * rotSpeed);
    }

    private void MoveUpdate()
    {
        if (Input.GetKey(KeyCode.W) && !isMoveStop)
        {
            moveVec.x = Mathf.Lerp(rigid.velocity.x, transform.up.x * moveSpeed, Time.deltaTime * moveMultiply);
            moveVec.y = Mathf.Lerp(rigid.velocity.y, transform.up.y * moveSpeed, Time.deltaTime * moveMultiply);

            rigid.velocity = moveVec;

            trailParticle.Play();
            return;
        }
        else
        {
            trailParticle.Stop();
            return;
        }
    }

    private void AttackUpdate()
    {
        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            Instantiate(bulletPrefab, shotPos.position, transform.rotation);
            fireTimer = 0;
        }

        fireTimer += Time.deltaTime;
    }

    private void DashUpdate()
    {
        if (Input.GetMouseButtonDown(1) && dashTimer >= dashCoolTime)
        {
            StartCoroutine(DashRoutine());
        }

        dashTimer += Time.deltaTime;
    }

    private IEnumerator DashRoutine()
    {
        rigid.velocity = Vector2.zero;

        isMoveStop = true;
        Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);

        rigid.AddForce(transform.up * dashPower, ForceMode2D.Impulse);

        yield return dashTimeWaitForSeconds;

        isMoveStop = false;
        dashTimer = 0;
    }
}