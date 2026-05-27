/* ===================================================
 * スクリプト名 : ジャンプ台スクリプト
 * Version : Ver0.02
 * Since : 2026/04/09
 * Update : 2026/05/27
* 用途 : ジャンプ台（横からの誤爆防止 ＆ 画像切り替え付き）
 * =================================================== */
using UnityEngine;
using System.Collections; // コルーチン（IEnumerator）を使うために必要

public class Spring : MonoBehaviour {
    [Header("跳ね返る力")]
    public float bounceForce = 20f;

    [Header("グラフィック設定")]
    [Tooltip("踏まれた瞬間に切り替わる画像（縮んだバネなど）")]
    public Sprite activeSprite;
    [Tooltip("画像を元に戻すまでの時間（秒）")]
    public float resetTime = 0.2f;

    private SpriteRenderer sr;
    private Sprite defaultSprite; // 元の画像を記憶しておく用

    void Start(){
        // 自分のについている SpriteRenderer を取得し、最初の画像を記憶しておく
        sr = GetComponent<SpriteRenderer>();
        if (sr != null){
            defaultSprite = sr.sprite;
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){

            // ▼【重要】衝突した「角度」を調べる
            // contacts[0].normal は「ぶつかった面の向き」を表します。
            // プレイヤーが上から落ちてきて床にぶつかった場合、床から見ると「上向き（yが1に近い数値）」の衝撃になります。
            // 横からぶつかった場合は y が 0 に近くなるため、これで「上から踏んだ時だけ」を正確に見分けられます。
            if (other.contacts[0].normal.y < -0.5f){

                Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
                if (rb != null){
                    // 現在の落下速度を完全に無視して、上方向へ強制的に速度を上書きする
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);

                    Debug.Log("大ジャンプ！");

                    // ▼ 画像を切り替えてボヨーンとさせる演出をスタート
                    StartCoroutine(AnimateSpring());
                }
            }
        }
    }

    // 画像を一定時間だけ切り替えて、また元に戻す処理
    private IEnumerator AnimateSpring(){
        if (sr != null && activeSprite != null){
            // 縮んだ画像に切り替え
            sr.sprite = activeSprite;

            // 指定した時間だけ待機
            yield return new WaitForSeconds(resetTime);

            // 元の画像に戻す
            sr.sprite = defaultSprite;
        }
    }
}