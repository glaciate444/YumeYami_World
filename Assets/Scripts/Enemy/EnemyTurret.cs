/* ===================================================
 * スクリプト名 : 敵の弾スクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : 弾のスクリプト、味方敵共通
 * =================================================== */
using UnityEngine;

public class EnemyTurret : MonoBehaviour{
    [Header("射撃設定")]
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float fireInterval = 2f;

    private Transform player; // プレイヤーの場所を記憶する変数
    private float timer;

    void Start(){
        // ゲーム開始時に、タグを使ってプレイヤーを自動的に探し出して記憶する
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null){
            player = p.transform;
        }
    }

    void Update(){
        // プレイヤーが見つからない（やられた等）場合は撃たない
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer >= fireInterval){
            Shoot();
            timer = 0f;
        }
    }

    private void Shoot(){
        if (enemyBulletPrefab != null && firePoint != null){
            // 1. プレイヤーへの方向ベクトルを計算（目的地の座標 - 現在の座標）
            Vector2 shootDir = (player.position - firePoint.position).normalized;

            // 2. 弾を生成
            GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

            // 3. 【オマケ演出】弾の画像自体をプレイヤーの方向へ回転させる
            // Mathf.Atan2という数学の関数を使って、ベクトルから角度を割り出します
            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 4. 弾を発射
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null){
                bulletScript.Initialize(shootDir);
            }
        }
    }
}