/* ===================================================
 * スクリプト名 : EnemyTurret.cs
 * Version : Ver0.05
 * Update : 2026/06/09
 * 用途 : アクエディ風の高性能な敵弾発射システム
 * 修正 : SpriteRendererのFlipXによる左右反転に対応
 * =================================================== */
using UnityEngine;
using System.Collections; // コルーチンに必要

public class EnemyTurret : MonoBehaviour{
    // ▼【追加】アクエディの「方向・対象」に相当する設定
    public enum AimType {
        Forward,         // 前方（今のキャラクターが向いている方向）
        AimAtPlayer,     // ターゲット（プレイヤー）を狙う
        RandomDirection, // ランダムな方向（全方位）
        Up,               // ▼真上
        Down             // ▼追加：真下
    }

    [Header("基本設定")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireInterval = 2f;
    public bool notRotateAngle = false; // 回転させない

    [Header("発射フォーメーション")]
    public AimType aimType = AimType.AimAtPlayer;
    public int bulletCount = 1;       // 発射数
    public float spreadAngle = 15f;   // 分散角度
    [Tooltip("0なら同時発射。0より大きければマシンガンのように連射します")]
    public float burstInterval = 0f;  // 間隔（秒数）

    [Header("ズレ（ゆらぎ）設定")]
    public float angleRandomness = 0f; // 角度のズレ
    public Vector2 positionOffset;     // 発射位置のズレX, Y
    public Vector2 positionRandomness; // ランダムな位置のズレ（散布界）

    private Transform player;
    private float timer;
    private Animator anim;

    [Header("ターゲット補正")]
    [Tooltip("プレイヤーの足元ではなく、中心や頭を狙うためのズレ（Yを0.5などに設定）")]
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
                // アニメーションが無い場合は直接コルーチンの開始メソッドを呼ぶ
                Shoot();
            }
        }
    }

    // アニメーションイベントからはこのメソッドを呼ぶ
    public void Shoot(){
        // 間隔（burstInterval）に対応するため、発射処理をコルーチンに任せる
        StartCoroutine(ShootRoutine());
    }

private IEnumerator ShootRoutine(){
        if (enemyBulletPrefab == null || firePoint == null) yield break;

        // ▼【超重要・修正】向きの判定を「Scale」と「FlipX」の両方に対応させる ▼
        // まずスケールでの反転状況を取得（基本は 1 か -1）
        float facingDirection = Mathf.Sign(transform.lossyScale.x);
        
        // 次に、自分自身や子オブジェクトにある SpriteRenderer を探す
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        
        // もし SpriteRenderer があって、かつ FlipX にチェックが入っていたら向きを反転させる！
        if (sr != null && sr.flipX){
            facingDirection *= -1f; 
        }

        Vector2 baseDir = Vector2.right; 

        if (aimType == AimType.AimAtPlayer && player != null){
            Vector2 targetPos = (Vector2)player.position + targetOffset;
            baseDir = (targetPos - (Vector2)firePoint.position).normalized;
        }
        else if (aimType == AimType.Forward){
            // 修正した facingDirection を適用して、正しい方向へ発射！
            baseDir = new Vector2(facingDirection, 0).normalized;
        }else if (aimType == AimType.RandomDirection){
            float randomAngle = Random.Range(0f, 360f);
            baseDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        }else if (aimType == AimType.Up){
            baseDir = Vector2.up; 
        }else if (aimType == AimType.Down){
            baseDir = Vector2.down; 
        }

        for (int i = 0; i < bulletCount; i++){
            

            float offsetAngle = 0f;
            if (bulletCount > 1) {
                offsetAngle = (i - (bulletCount - 1) / 2f) * spreadAngle;
            }
            offsetAngle += Random.Range(-angleRandomness, angleRandomness);
            Vector2 finalDir = RotateVector(baseDir, offsetAngle);

            Vector2 randomPosOffset = new Vector2(Random.Range(-positionRandomness.x, positionRandomness.x), Random.Range(-positionRandomness.y, positionRandomness.y));
            Vector2 finalPos = (Vector2)firePoint.position 
                             + new Vector2(positionOffset.x * facingDirection, positionOffset.y) // ▼ 弾の出現位置のズレにも対応
                             + randomPosOffset;

            GameObject bullet = Instantiate(enemyBulletPrefab, finalPos, Quaternion.identity);

            float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
            //回転させない場合
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