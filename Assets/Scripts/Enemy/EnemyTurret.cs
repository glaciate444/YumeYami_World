/* ===================================================
 * スクリプト名 : 敵の弾スクリプト
 * Version : Ver0.02
 * Update : 2026/04/09
 * 用途 : 弾のスクリプト、味方敵共通
 * 変更点 : 追尾ON、OFF切り替え
 * =================================================== */
using UnityEngine;

public class EnemyTurret : MonoBehaviour{
    [Header("基本設定")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireInterval = 2f;

    [Header("攻撃の拡張")]
    public bool isHoming = true;      // trueで自機狙い、falseで正面（右向き）
    public int bulletCount = 1;       // 弾の数（1, 3, 5...）
    public float spreadAngle = 15f;   // 弾ごとの角度差

    private Transform player;
    private float timer;

    void Start(){
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update(){
        timer += Time.deltaTime;
        if (timer >= fireInterval){
            Shoot();
            timer = 0f;
        }
    }

    private void Shoot(){
        if (enemyBulletPrefab == null || firePoint == null) return;

        // 1. ベースとなる方向を決定
        Vector2 baseDir;
        if (isHoming && player != null){
            baseDir = (player.position - firePoint.position).normalized;
        }else{
            // オブジェクトのスケール（向き）を見て方向を決定する
            // 親オブジェクトの反転も考慮して lossyScale を使用します
            float facingDirection = Mathf.Sign(transform.lossyScale.x);
            baseDir = new Vector2(facingDirection, 0).normalized;
        }

        // 2. 弾の数だけループして発射
        for (int i = 0; i < bulletCount; i++){
            // 拡散させるための角度計算
            // i=0なら中心、それ以降は左右に振り分ける計算
            float offset = (i - (bulletCount - 1) / 2f) * spreadAngle;
            Vector2 finalDir = RotateVector(baseDir, offset);

            // 弾の生成
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

            // 弾の向きを合わせる（演出）
            float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 発射
            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null) b.Initialize(finalDir);
        }
    }

    // ベクトルを指定した角度(degree)だけ回転させる補助関数
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