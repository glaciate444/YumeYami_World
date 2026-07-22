/* ===================================================
 * スクリプト名 : EnemyFloater.cs
 * Version : Ver0.02
 * Since : 2026/05/14
 * Update : 2026/07/22
 * 用途 : 空中をフワフワ移動し、近づくと突撃する敵
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFloater : EnemyMovement {
    [Header("フワフワ設定")]
    public float speed = 2f;
    public float distance = 2f;
    public bool moveHorizontal = false;

    // ▼▼▼ 新規追加：突撃設定 ▼▼▼
    [Header("突撃設定")]
    [Tooltip("チェックを入れると、プレイヤーが近づいた時に突撃します")]
    public bool canCharge = false;  // ← 【新規追加】突撃するかどうかのスイッチ

    public float detectRadius = 5f; // プレイヤーを感知する距離
    public float chargeSpeed = 6f;  // 突撃時の速さ

    private bool isCharging = false;   // 突撃中かどうかのフラグ
    private Transform playerTransform; // プレイヤーの座標
    private Vector2 chargeDirection;   // 突撃する方向
    // ▲▲▲ 新規追加ここまで ▲▲▲

    private Vector2 startPos;
    private float timer;
    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        rb.bodyType = RigidbodyType2D.Kinematic;

        // ▼【新規追加】シーン内のプレイヤーを探して記憶しておく
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null){
            playerTransform = player.transform;
        }
    }

    void FixedUpdate(){
        if (!this.enabled) return;

        // ▼▼▼ 新規追加：突撃中の処理 ▼▼▼
        if (isCharging){
            // 突撃中は、決まった方向へ一直線に移動し続ける
            rb.MovePosition(rb.position + chargeDirection * chargeSpeed * Time.fixedDeltaTime);
            return; // 突撃中は下のフワフワ移動を行わない
        }

        if (canCharge && playerTransform != null){

            float dist = Vector2.Distance(transform.position, playerTransform.position);

            if (dist <= detectRadius){
                // 感知範囲に入ったら突撃開始！
                isCharging = true;
                // プレイヤーのいる方向を計算して記憶する（正規化して長さを1にする）
                chargeDirection = (playerTransform.position - transform.position).normalized;

                // ※突撃と同時に絵の向き（左右）を変えたい場合は以下をコメントアウト解除
                // float facingDir = Mathf.Sign(chargeDirection.x);
                // transform.localScale = new Vector3(facingDir, 1, 1);
            }
        }

        // --- 以下、既存のフワフワ移動処理 ---
        timer += Time.fixedDeltaTime * speed;

        float wave = Mathf.Sin(timer) * distance;

        Vector2 newPos = startPos;
        if (moveHorizontal){
            newPos.x = startPos.x + wave;
        }else{
            newPos.y = startPos.y + wave;
        }

        rb.MovePosition(newPos);
    }

    // ▼▼▼ 新規追加：エディタ用の補助機能 ▼▼▼
    // Unityエディタ上で、この敵を選択した時に「感知範囲」を赤い円で表示します
    private void OnDrawGizmosSelected(){
        // 突撃タイプの時だけ感知範囲を赤い円で表示する
        if (canCharge){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectRadius);
        }
    }
}