/* ===================================================
 * スクリプト名 : EnemyJumper.cs
 * Version : Ver0.01
 * Since : 2026/04/30
 * Update : 2026/04/30
 * 用途 : ぴょんぴょん跳ねて近づいてくる敵
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))] // 画面外判定に必要
public class EnemyJumper : MonoBehaviour{
    [Header("ジャンプ設定")]
    public float jumpForceX = 3f;  // 横に飛ぶ力
    public float jumpForceY = 7f;  // 上に飛ぶ力
    public float jumpInterval = 2f;// 何秒に1回ジャンプするか

    [Header("接地判定")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Transform player;
    private float jumpTimer;
    private bool isVisible = false; // 画面に映っているかどうかのフラグ
    private bool isGrounded;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // プレイヤーを探して記憶する（Playerタグが付いている前提）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null){
            player = playerObj.transform;
        }

        // 最初はすぐにジャンプできるようにタイマーをセット
        jumpTimer = jumpInterval;
    }

    // ▼ 超便利機能：カメラ（画面）にこのキャラが映った瞬間に呼ばれる ▼
    private void OnBecameVisible(){
        isVisible = true;
    }

    // ▼ 超便利機能：カメラ（画面）からこのキャラが消えた瞬間に呼ばれる ▼
    private void OnBecameInvisible(){
        isVisible = false;
        // 画面外に出たら、空中で飛んでいかないようにピタッと止める（お好みで）
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Update(){
        // 画面外にいる時、またはプレイヤーがいない時は何もしない
        if (!isVisible || player == null) return;

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded){
            jumpTimer -= Time.deltaTime;

            // ▼【修正】飛んだ直後（タイマーがリセットされた直後）の数フレームは横移動を止めない！
            // タイマーが少し減って「完全に着地して待機している状態」の時だけピタッと止める
            if (jumpTimer < jumpInterval - 0.1f){
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }

            // タイマーが0になったらジャンプ
            if (jumpTimer <= 0f){
                JumpTowardsPlayer();
                jumpTimer = jumpInterval; // タイマーをリセット
            }
        }
    }

    private void JumpTowardsPlayer(){
        // プレイヤーが自分の「右(1)」にいるか「左(-1)」にいるかを計算
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // キャラクターの向きをプレイヤーの方へ反転させる
        // （※元画像が右向きならそのまま、左向きなら -direction に調整してください）
        transform.localScale = new Vector3(-direction, 1, 1);

        // 斜め上に向かって力を加えてジャンプ！
        rb.linearVelocity = new Vector2(direction * jumpForceX, jumpForceY);
    }
}