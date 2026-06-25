/* ===================================================
 * スクリプト名 : BossHoverMove.cs
 * 用途 : ボス専用のサイン波（波打ち）ホバリング移動
 * 修正 : エディタの初期状態に依存せず、FirePointを必ず正面に配置する絶対値計算を追加
 * =================================================== */
using UnityEngine;

public class BossHoverMove : MonoBehaviour {
    [Header("左右の移動設定")]
    public float horizontalSpeed = 2.0f; 
    public float horizontalRange = 5.0f; 

    [Header("上下の波打ち設定（サイン波）")]
    public float verticalSpeed = 3.0f;   
    public float verticalRange = 1.0f;   

    [Header("向き（スプライト反転）の設定")]
    public bool isDefaultFacingRight = true; 
    public bool alwaysLookAtPlayer = false;
    public bool pinDirection = false;
    public bool initialFlipX = false;

    [Header("子オブジェクトの連動設定")]
    public Transform firePoint;

    private Vector3 startPos;
    private int direction = -1; 
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private Vector3 initialFirePointLocalPos; 

    void Start() {
        startPos = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerTransform = player.transform;
        }

        if (firePoint != null) {
            initialFirePointLocalPos = firePoint.localPosition;
        }
    }

    void Update() {
        // 1. 移動計算
        float currentX = transform.position.x;
        currentX += direction * horizontalSpeed * Time.deltaTime;

        if (currentX < startPos.x - horizontalRange) {
            currentX = startPos.x - horizontalRange;
            direction = 1; 
        } else if (currentX > startPos.x + horizontalRange) {
            currentX = startPos.x + horizontalRange;
            direction = -1; 
        }

        float currentY = startPos.y + Mathf.Sin(Time.time * verticalSpeed) * verticalRange;
        transform.position = new Vector3(currentX, currentY, transform.position.z);

        // 2. 向きの制御ロジック
        if (spriteRenderer != null) {
            bool shouldFlip = false; 

            if (alwaysLookAtPlayer && playerTransform != null) {
                bool isPlayerOnRight = playerTransform.position.x > transform.position.x;
                shouldFlip = isDefaultFacingRight ? !isPlayerOnRight : isPlayerOnRight;
            } 
            else if (pinDirection) {
                shouldFlip = initialFlipX;
            } 
            else {
                shouldFlip = isDefaultFacingRight ? (direction == -1) : (direction == 1);
            }

            // ▼ 画像の反転を適用
            spriteRenderer.flipX = shouldFlip;

            // ▼【超安全修正】FirePointの位置を「見た目の向き」に合わせて強制的に修正する
            if (firePoint != null) {
                // ① 現在「見た目上」右を向いているかどうかの判定
                bool isVisuallyFacingRight = isDefaultFacingRight ? !shouldFlip : shouldFlip;

                // ② X座標の「絶対値（中心からの距離）」だけを取得する（マイナスを無効化）
                float absoluteX = Mathf.Abs(initialFirePointLocalPos.x);

                // ③ 見た目が右向きならプラス（右側）、左向きならマイナス（左側）に強制セット！
                float currentFirePointX = isVisuallyFacingRight ? absoluteX : -absoluteX;
                
                firePoint.localPosition = new Vector3(currentFirePointX, initialFirePointLocalPos.y, initialFirePointLocalPos.z);
            }
        }
    }
}