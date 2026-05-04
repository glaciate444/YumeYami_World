/* ===================================================
 * スクリプト名 : HiddenBlock.cs
 * Version : Ver0.01
 * Since : 2026/05/03
 * Update : 2026/05/03
 * 用途 : 隠れブロック
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class HiddenBlock : MonoBehaviour{
    [Header("ブロック設定")]
    [Tooltip("叩かれた後に表示するブロックの画像")]
    public Sprite visibleSprite;

    [Tooltip("出すアイテムのプレハブ（空でもOK）")]
    public GameObject itemPrefab;

    [Tooltip("実体化した後のレイヤー名（Groundなど）")]
    public string groundLayerName = "Ground";

    [Header("演出設定")]
    public float bounceHeight = 0.2f; // 叩いた時に少し跳ねる高さ
    public float bounceSpeed = 4f;    // 跳ねるスピード

    private bool isRevealed = false;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Vector3 originalPos;

    void Start(){
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        // 【初期設定】透明にし、すり抜け可能（トリガー）にする
        sr.color = new Color(1, 1, 1, 0);
        col.isTrigger = true;

        originalPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        // すでに出現済みなら何もしない
        if (isRevealed) return;

        // 触れたのがプレイヤーかどうか確認
        if (collision.CompareTag("Player")){
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            // 【超重要】プレイヤーが「上昇中（ジャンプ中）」かつ「ブロックより下から当たった」場合のみ反応させる
            if (playerRb != null && playerRb.linearVelocity.y > 0 && collision.transform.position.y < transform.position.y){
                RevealBlock(playerRb);
            }
        }
    }

    private void RevealBlock(Rigidbody2D playerRb){
        isRevealed = true;

        // 1. プレイヤーのジャンプの勢いを殺して跳ね返す（頭をぶつけたリアルな挙動）
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);

        // 2. ブロックを実体化させる
        sr.color = new Color(1, 1, 1, 1);
        if (visibleSprite != null){
            sr.sprite = visibleSprite;
        }

        // 3. 当たり判定を「すり抜け不可の壁」にする
        col.isTrigger = false;

        // 4. レイヤーをGroundなどに変更し、プレイヤーが上に乗ってジャンプできるようにする
        gameObject.layer = LayerMask.NameToLayer(groundLayerName);

        // 5. アイテムを出す（設定されていれば、ブロックの上にポンッと出す）
        if (itemPrefab != null){
            Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);
        }

        // 6. ブロックがポンッと跳ねるアニメーションを開始
        StartCoroutine(BounceRoutine());
    }

    // ブロックが叩かれた時に少し上下に動くアニメーション
    private IEnumerator BounceRoutine(){
        Vector3 targetPos = originalPos + new Vector3(0, bounceHeight, 0);

        // 上に上がる
        while (transform.position.y < targetPos.y){
            transform.position = Vector3.MoveTowards(transform.position, targetPos, bounceSpeed * Time.deltaTime);
            yield return null;
        }

        // 下に下がる（元の位置へピッタリ戻る）
        while (transform.position.y > originalPos.y){
            transform.position = Vector3.MoveTowards(transform.position, originalPos, bounceSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = originalPos; // ズレ防止
    }
}