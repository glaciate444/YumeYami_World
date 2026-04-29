using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : MonoBehaviour{
    public enum PlatformType { SimpleFall, TimedMoveAndFall }

    [Header("モード設定")]
    public PlatformType type = PlatformType.SimpleFall;

    [Header("共通設定")]
    public float fallDelay = 1.0f; // 乗ってから何秒後に（または0になってから）落ちるか

    [Header("303モード専用（移動）")]
    public float moveSpeedX = 5f;  // 横に動く速度（マイナスで左へ）

    private Rigidbody2D rb;
    private bool isTriggered = false;
    private float timer;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // 最初は物理の影響を受けないようにしておく
        rb.bodyType = RigidbodyType2D.Kinematic;

        timer = fallDelay;
    }

    private void OnCollisionEnter2D(Collision2D collision){
        // プレイヤーが「上から」乗った時だけ作動させる
        if (collision.gameObject.CompareTag("Player") && !isTriggered){
            // 足元が自分より上にあるか一応チェック
            if (collision.transform.position.y > transform.position.y){
                isTriggered = true;

                // 302（すぐ落ちるタイプ）の場合は、少し待ってから物理をONにする
                if (type == PlatformType.SimpleFall){
                    Invoke("StartFalling", fallDelay);
                }
            }
        }
    }

    void Update(){
        if (!isTriggered) return;

        // 303モード：カウントダウンしながら横移動
        if (type == PlatformType.TimedMoveAndFall){
            timer -= Time.deltaTime;

            if (timer > 0){
                // 横にスライド移動（Kinematicのまま移動させる）
                rb.linearVelocity = new Vector2(moveSpeedX, 0);
            }else{
                // 時間切れ：物理（Dynamic）に切り替えて落下開始
                StartFalling();
            }
        }
    }

    private void StartFalling(){
        // ▼【重要・追加】落下開始時に、プレイヤーをリフトから強制的に引き剥がす！
        // これがないと、両方の重力計算が喧嘩してプレイヤーが謎のバウンドを起こします。
        transform.DetachChildren();

        // 物理（Dynamic）に切り替えて落下開始
        rb.bodyType = RigidbodyType2D.Dynamic;

        // ▼【重要・追加】プレイヤーの足元からスムーズに離れるよう、初速を下向きに少しつける
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);

        Destroy(gameObject, 3f);
    }

    // プレイヤーを床と一緒に動かすための処理
    private void OnCollisionStay2D(Collision2D collision){
        // 落下中（Dynamic）の時は親子関係を作らない
        if (rb.bodyType == RigidbodyType2D.Dynamic) return;

        if (collision.gameObject.CompareTag("Player")){
            // 親子関係による縦のズレ防止
            collision.transform.SetParent(transform);

            // ▼【追加】横移動している場合、プレイヤーに自分の速度をコピーして渡す ▼
            if (type == PlatformType.TimedMoveAndFall){
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null){
                    pc.platformVelocity = rb.linearVelocity;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            // 床から離れたら親子関係を解消する
            collision.transform.SetParent(null);
        }
    }
}