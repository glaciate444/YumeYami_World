/* ===================================================
 * スクリプト名 : EnemyJumper.cs
 * Version : Ver0.03
 * Since : 2026/04/30
 * Update : 2026/05/27
 * 用途 : ぴょんぴょん跳ねて近づいてくる敵
 * 更新 : 基底クラス実装
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))] // 画面外判定に必要
public class EnemyJumper : EnemyMovement{
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
    private Animator anim; // アニメーターを操作するための変数

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // ▼【追加】アタッチされているAnimatorを取得

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

        // ▼【重要・修正】上昇中（ジャンプ直後）は、強制的に接地判定をOFFにする！
        if (rb.linearVelocity.y > 0.1f){
            isGrounded = false;
        }else{
            // 落下中、または停止中の時だけ足元の判定を行う
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }

        // 現在の接地状態をAnimatorに毎フレーム教える
        if (anim != null){
            anim.SetBool("isGrounded", isGrounded);
        }

        // ▼【修正】重複していた2回目の接地判定は削除しました

        if (isGrounded){
            jumpTimer -= Time.deltaTime;

            // 飛んだ直後（タイマーがリセットされた直後）の数フレームは横移動を止めない！
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
    // ▼ スクリプトの最後（最後の } の手前）にこれを追加するだけ！ ▼
    private void OnDrawGizmos(){
        if (groundCheck != null){
            // エディタ上で、センサーの位置に赤い円を描画して見やすくする
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}