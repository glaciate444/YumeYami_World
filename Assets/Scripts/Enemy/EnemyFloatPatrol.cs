/* ===================================================
 * スクリプト名 : EnemyFloatPatrol.cs
 * Version : Ver0.02
 * 用途 : ふわりと落下し、着地後に巡回を開始する敵（アニメーション対応版）
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFloatPatrol : EnemyMovement{
    [Header("落下設定")]
    public float floatFallSpeed = 1.5f;

    [Header("移動設定")]
    public float moveSpeed = 2f;
    private bool movingRight = false;
    private bool isLanded = false;

    [Header("判定センサー")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform edgeCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim; // ▼ 新規：アニメーター用変数

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // ▼ アニメーターを取得
    }

    void FixedUpdate(){
        if (!isLanded){
            // 落下処理
            float currentVelocityY = rb.linearVelocity.y;
            if (currentVelocityY < -floatFallSpeed){
                currentVelocityY = -floatFallSpeed;
            }
            rb.linearVelocity = new Vector2(0f, currentVelocityY);

            // 着地判定
            if (groundCheck != null){
                isLanded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            }

            // 落下中は「歩いていない」状態にする
            if (anim != null){
                anim.SetBool("isWalking", false);
            }

        }else{
            // 巡回処理
            float currentSpeed = movingRight ? moveSpeed : -moveSpeed;
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

            bool isHittingWall = false;
            if (wallCheck != null){
                isHittingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);
            }

            bool isGroundAhead = true;
            if (edgeCheck != null){
                isGroundAhead = Physics2D.OverlapCircle(edgeCheck.position, checkRadius, groundLayer);
            }

            if (isHittingWall || !isGroundAhead){
                Flip();
            }

            // 着地したら「歩いている」状態にする
            if (anim != null){
                anim.SetBool("isWalking", true);
            }
        }
    }

    private void Flip(){
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmos(){
        if (wallCheck != null){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }
        if (edgeCheck != null){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(edgeCheck.position, checkRadius);
        }
        if (groundCheck != null){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}