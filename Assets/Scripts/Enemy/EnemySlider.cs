/* ===================================================
 * スクリプト名 : EnemySlider.cs
 * Version : Ver0.01
 * Since : 2026/05/14
 * Update : 2026/05/14
 * 用途 : 氷の上を滑るように突撃してくる敵（ペンギンなど）
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemySlider : EnemyMovement {
    [Header("滑り・突撃設定")]
    public float chargeSpeed = 12f;     // 突撃時の最初の猛スピード
    [Range(0.8f, 0.99f)]
    public float slideFriction = 0.96f; // 滑り具合（1に近いほどよく滑り、小さいとすぐ止まる）
    public float chargeInterval = 3f;   // 次の突撃までの待機時間

    [Header("壁の判定")]
    public Transform wallCheck;         // 目の前の壁判定用オブジェクト
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;       // 壁（Ground）のレイヤー

    private Rigidbody2D rb;
    private Transform player;
    private float timer;
    private bool isSliding = false;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        
        // プレイヤーを探して記憶する
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        timer = chargeInterval; // 最初は少し待ってから突撃開始
    }

    void FixedUpdate() {
        // 親クラス（EnemyMovement）の機能により、ダメージ硬直中などは this.enabled が false になり停止します
        if (!this.enabled) return;

        // ▼ 壁にぶつかったかどうかの判定 ▼
        if (wallCheck != null) {
            bool isHittingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);
            
            // 滑っている最中に壁に激突したら、強制的にストップして振り向く
            if (isHittingWall && isSliding) {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                isSliding = false;
                timer = chargeInterval; // 再び待機モードへ
            }
        }

        // ▼ 滑り中の処理 ▼
        if (isSliding) {
            // 毎フレーム、現在のX速度に「slideFriction（0.96など）」を掛けて、少しずつスピードを落とす（氷の摩擦表現）
            float currentVelocityX = rb.linearVelocity.x * slideFriction;
            rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

            // スピードが十分に落ちたら完全に停止し、待機モードへ移行する
            if (Mathf.Abs(currentVelocityX) < 0.5f) {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                isSliding = false;
                timer = chargeInterval; 
            }
        } 
        // ▼ 待機中の処理 ▼
        else {
            timer -= Time.fixedDeltaTime;
            
            // タイマーがゼロになったらプレイヤーに向けて突撃！
            if (timer <= 0f && player != null) {
                ChargeTowardsPlayer();
            }
        }
    }

    private void ChargeTowardsPlayer() {
        isSliding = true;

        // プレイヤーがいる方向（右なら1、左なら-1）を計算
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // 敵の画像をプレイヤーの方向へ向かせる（※画像が左向きで作られている場合は -direction にしてください）
        transform.localScale = new Vector3(direction, 1, 1);

        // 突撃の初速（猛スピード）を一気に与える！
        rb.linearVelocity = new Vector2(direction * chargeSpeed, rb.linearVelocity.y);
    }
}