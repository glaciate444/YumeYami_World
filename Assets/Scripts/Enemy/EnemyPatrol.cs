using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour{
    [Header("移動設定")]
    public float moveSpeed = 2f;
    private bool movingRight = true;

    [Header("壁・崖の判定")]
    public Transform wallCheck;    // 目の前に配置する空オブジェクト
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

        // 2. 目の前に壁があるか判定
        bool isHittingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        // 壁にぶつかったら振り向く
        if (isHittingWall){
            Flip();
        }
    }

    private void Flip(){
        // 向きフラグを反転
        movingRight = !movingRight;

        // スケールのXを反転させて見た目と判定を裏返す（プレイヤーと同じ手法）
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}