/* ===================================================
 * スクリプト名 : BossHoverMove.cs
 * 用途 : ボス専用のサイン波（波打ち）ホバリング移動
 * 修正 : スプライトのデフォルトの向き（右向き/左向き）に対応
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
    [Tooltip("【重要】ボスの元の画像が『右向き』の場合はチェックを入れてください")]
    public bool isDefaultFacingRight = true; // ▼ これを追加しました！

    [Tooltip("【プレイヤー注視】チェックを入れると、常にプレイヤーのいる方を向きます")]
    public bool alwaysLookAtPlayer = false;

    [Tooltip("【向き固定】チェックを入れると、移動方向に関係なく向きを固定します")]
    public bool pinDirection = false;
    
    [Tooltip("固定時の反転状態（チェックを入れるとFlipXがTrueになります）")]
    public bool initialFlipX = false;

    private Vector3 startPos;
    private int direction = -1; // -1:左向き, 1:右向き
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

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
            if (alwaysLookAtPlayer && playerTransform != null) {
                // プレイヤーが自分より右側にいるか
                bool isPlayerOnRight = playerTransform.position.x > transform.position.x;
                
                // ▼【修正】元の画像が右向きか左向きかで、反転（FlipX）の計算を逆にする
                if (isDefaultFacingRight) {
                    spriteRenderer.flipX = !isPlayerOnRight; // 元が右向きなら、右にいる時は反転しない
                } else {
                    spriteRenderer.flipX = isPlayerOnRight;  // 元が左向きなら、右にいる時に反転する
                }
            } 
            else if (pinDirection) {
                spriteRenderer.flipX = initialFlipX;
            } 
            else {
                // 進行方向に向くモードも、元の向きに合わせて計算
                if (isDefaultFacingRight) {
                    spriteRenderer.flipX = (direction == -1); // 左に進む時に反転
                } else {
                    spriteRenderer.flipX = (direction == 1);  // 右に進む時に反転
                }
            }
        }
    }
}