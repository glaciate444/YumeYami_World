/* ===================================================
 * スクリプト名 : 動く足場スクリプト
 * Version : Ver0.02
 * Since : 2026/04/09
 * Update : 2026/05/22
 * 用途 : 動く足場
 * 更新内容 : 改修
 * 📂 MovingPlatform_Setup (空オブジェクト)
 * ├🟦 Platform (足場本体。BoxCollider2D を付け、以下のスクリプトをアタッチ)
 * ├🔴 PointA (空オブジェクト：スタート地点)
 * └🔴 PointB (空オブジェクト：折り返し地点)
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour {
    [Header("移動設定")]
    public Transform[] waypoints; // 移動する目標地点のリスト
    public float speed = 3f;      // 移動スピード
    public float waitTime = 1f;   // 目的地に着いた時の待機時間

    private Rigidbody2D rb;
    private int currentPointIndex = 0;
    private float waitTimer;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        
        // 重力やプレイヤーの重さに負けないよう、Kinematicに設定
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        waitTimer = waitTime;
    }

    void FixedUpdate() {
        // 目標地点が設定されていない場合は何もしない
        if (waypoints.Length == 0) return;

        Transform targetPoint = waypoints[currentPointIndex];
        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetPoint.position;

        // 目的地にほぼ到着したかどうかの判定
        if (Vector2.Distance(currentPos, targetPos) < 0.1f) {
            // 到着：ピタッと止まって待機時間を減らす
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.fixedDeltaTime;

            // 待機時間がゼロになったら次のポイントへ
            if (waitTimer <= 0f) {
                currentPointIndex = (currentPointIndex + 1) % waypoints.Length;
                waitTimer = waitTime;
            }
        } else {
            // 移動中：目的地に向かって進む
            Vector2 direction = (targetPos - currentPos).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    // プレイヤーを床と一緒に動かすための処理（FallingPlatformの応用）
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            // 縦のズレを防止するための親子関係
            collision.transform.SetParent(transform);

            // 横移動のガクつきを無くすため、プレイヤーに床の速度を教える
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null) {
                pc.platformVelocity = rb.linearVelocity;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            // 床から離れたら親子関係を解除
            collision.transform.SetParent(null);

            // 速度の伝達もストップする
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null) {
                pc.platformVelocity = Vector2.zero;
            }
        }
    }
}