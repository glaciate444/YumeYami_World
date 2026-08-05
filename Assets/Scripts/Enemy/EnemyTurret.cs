/* ===================================================
 * スクリプト名 : EnemyTurret.cs
 * Version : Ver0.06
 * Update : 2026/08/05
 * 用途 : アクエディ風の高性能な敵弾発射システム
 * 修正 : 放物線（山なり）に投げる AimType を追加
 * =================================================== */
using UnityEngine;
using System.Collections;

public class EnemyTurret : MonoBehaviour {
    public enum AimType {
        Forward,
        AimAtPlayer,
        RandomDirection,
        Up,
        Down,
        ParabolaForward,  // 前方へ放物線（山なり）で投げる
        ParabolaAtPlayer  // プレイヤーのいる方向へ放物線で投げる
    }

    [Header("識別用ID（同じ敵に複数付ける場合用）")]
    public int turretID = 0;

    [Header("基本設定")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireInterval = 2f;
    public bool notRotateAngle = false;

    [Header("発射フォーメーション")]
    public AimType aimType = AimType.AimAtPlayer;
    public int bulletCount = 1;
    public float spreadAngle = 15f;
    public float burstInterval = 0f;

    // ▼▼▼ 新規追加：放物線の設定 ▼▼▼
    [Header("放物線設定（AimTypeがParabolaの時）")]
    [Tooltip("上に投げる強さの比率。1で斜め45度、数値を大きくすると高く上がります")]
    public float parabolaUpForce = 1.0f;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("ズレ（ゆらぎ）設定")]
    public float angleRandomness = 0f;
    public Vector2 positionOffset;
    public Vector2 positionRandomness;

    private Transform player;
    private float timer;
    private Animator anim;

    [Header("ターゲット補正")]
    public Vector2 targetOffset = new Vector2(0f, 0.5f);

    void Start(){
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        anim = GetComponent<Animator>();
    }

    void Update(){
        timer += Time.deltaTime;
        if (timer >= fireInterval){
            timer = 0f;
            if (anim != null){
                anim.SetTrigger("Attack");
            }else{
                Shoot();
            }
        }
    }

    public void Shoot(){
        StartCoroutine(ShootRoutine());
    }

    public void ShootByID(int id){
        if (this.turretID == id){
            StartCoroutine(ShootRoutine());
        }
    }

    private IEnumerator ShootRoutine(){
        if (enemyBulletPrefab == null || firePoint == null) yield break;

        float facingDirection = Mathf.Sign(transform.lossyScale.x);
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.flipX){
            facingDirection *= -1f;
        }

        Vector2 baseDir = Vector2.right;

        if (aimType == AimType.AimAtPlayer && player != null){
            Vector2 targetPos = (Vector2)player.position + targetOffset;
            baseDir = (targetPos - (Vector2)firePoint.position).normalized;
        }else if (aimType == AimType.Forward){
            baseDir = new Vector2(facingDirection, 0).normalized;
        }else if (aimType == AimType.RandomDirection){
            float randomAngle = Random.Range(0f, 360f);
            baseDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        }else if (aimType == AimType.Up){
            baseDir = Vector2.up;
        }else if (aimType == AimType.Down){
            baseDir = Vector2.down;
        }
        // ▼▼▼ 新規追加：放物線（山なり）の軌道計算 ▼▼▼
        else if (aimType == AimType.ParabolaForward){
            // 自分の向いている方向（X）に、指定した上向きの力（Y）を合成する
            baseDir = new Vector2(facingDirection, parabolaUpForce).normalized;
        }else if (aimType == AimType.ParabolaAtPlayer && player != null){
            // プレイヤーが左右どちらにいるかを判定し、その方向へ山なりに投げる
            float dirX = Mathf.Sign(player.position.x - firePoint.position.x);
            baseDir = new Vector2(dirX, parabolaUpForce).normalized;
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        for (int i = 0; i < bulletCount; i++){
            float offsetAngle = 0f;
            if (bulletCount > 1){
                offsetAngle = (i - (bulletCount - 1) / 2f) * spreadAngle;
            }
            offsetAngle += Random.Range(-angleRandomness, angleRandomness);
            Vector2 finalDir = RotateVector(baseDir, offsetAngle);

            Vector2 randomPosOffset = new Vector2(Random.Range(-positionRandomness.x, positionRandomness.x), Random.Range(-positionRandomness.y, positionRandomness.y));
            Vector2 finalPos = (Vector2)firePoint.position
                             + new Vector2(positionOffset.x * facingDirection, positionOffset.y)
                             + randomPosOffset;

            GameObject bullet = Instantiate(enemyBulletPrefab, finalPos, Quaternion.identity);

            float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
            if (!notRotateAngle){
                bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null) b.Initialize(finalDir);

            if (burstInterval > 0f){
                yield return new WaitForSeconds(burstInterval);
            }
        }
    }

    private Vector2 RotateVector(Vector2 v, float degrees){
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}