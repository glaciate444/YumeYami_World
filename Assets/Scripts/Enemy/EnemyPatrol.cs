/* ===================================================
 * スクリプト名 : EnemyPatrol.cs
 * 用途 : 敵の巡回移動（壁と崖を検知して引き返す）
 * 更新 : 崖判定（edgeCheck）とGizmosの追加
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : EnemyMovement{
    [Header("移動設定")]
    public float moveSpeed = 2f;
    private bool movingRight = false;

    [Header("壁・崖の判定")]
    public Transform wallCheck;    // 目の前に配置する空オブジェクト
    public Transform edgeCheck;    // ▼【追加】足元の少し前に配置する崖センサー
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;  // Groundレイヤーを指定

    private Rigidbody2D rb;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate(){
        // 1. 移動処理
        float currentSpeed = movingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // 2. 目の前に壁があるか判定（壁があれば true）
        bool isHittingWall = false;
        if (wallCheck != null) {
            isHittingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);
        }

        // 3. ▼【追加】目の前の足元に地面があるか判定（地面がなければ崖なので false になる）
        bool isGroundAhead = true;
        if (edgeCheck != null) {
            isGroundAhead = Physics2D.OverlapCircle(edgeCheck.position, checkRadius, groundLayer);
        }

        // 4. 「壁にぶつかる」または「目の前に地面がない（崖）」なら振り向く
        if (isHittingWall || !isGroundAhead){
            Flip();
        }
    }

    private void Flip(){
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ▼【追加】エディタ上でセンサーの位置を可視化する ▼
    private void OnDrawGizmos() {
        if (wallCheck != null) {
            Gizmos.color = Color.blue; // 壁センサーは青色の円
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }
        if (edgeCheck != null) {
            Gizmos.color = Color.red;  // 崖センサーは赤色の円
            Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
        }
    }
}