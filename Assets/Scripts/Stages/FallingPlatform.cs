/* ===================================================
 * スクリプト名 : FallingPlatform.cs
 * Version : Ver0.03
 * 用途 : リフト全般の制御
 * 拡張 : スポナーからの自動移動（AutoMove）対応と、道連れ消滅の完全防止
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingPlatform : MonoBehaviour {
    // ▼【追加】AutoMove（自動移動リフト）モードを追加
    public enum PlatformType { SimpleFall, TimedMoveAndFall, AutoMove }

    [Header("モード設定")]
    public PlatformType type = PlatformType.SimpleFall;

    [Header("共通設定")]
    public float fallDelay = 1.0f;

    [Header("303モード専用（移動）")]
    public float moveSpeedX = 5f;

    private Rigidbody2D rb;
    private bool isTriggered = false;
    private float timer;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        timer = fallDelay;
    }

    private void OnCollisionEnter2D(Collision2D collision){
        // ▼【追加】AutoMoveモードの場合は、乗っても勝手に落下タイマーを起動しない
        if (type == PlatformType.AutoMove) return;

        if (collision.gameObject.CompareTag("Player") && !isTriggered){
            if (collision.transform.position.y > transform.position.y){
                isTriggered = true;

                if (type == PlatformType.SimpleFall){
                    Invoke("StartFalling", fallDelay);
                }
            }
        }
    }

    void Update(){
        if (!isTriggered) return;

        if (type == PlatformType.TimedMoveAndFall){
            timer -= Time.deltaTime;
            if (timer > 0){
                rb.linearVelocity = new Vector2(moveSpeedX, 0);
            }else{
                StartFalling();
            }
        }
    }

    private void StartFalling(){
        transform.DetachChildren();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        Destroy(gameObject, 3f);
    }

    private void OnCollisionStay2D(Collision2D collision){
        if (rb.bodyType == RigidbodyType2D.Dynamic) return;

        if (collision.gameObject.CompareTag("Player")){
            collision.transform.SetParent(transform);

            // ▼【修正】TimedMoveAndFallだけでなく、AutoMoveの場合もプレイヤーに速度を渡す
            if (type == PlatformType.TimedMoveAndFall || type == PlatformType.AutoMove){
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null){
                    // スポナー（ObjectSpawner）から与えられた初速を、プレイヤーの足元にも伝える
                    pc.platformVelocity = rb.linearVelocity;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            collision.transform.SetParent(null);
        }
    }

    // 道連れ消滅の完全防止
    private void OnDestroy(){
        // ObjectSpawnerの寿命などでこのリフトが強制的に消滅させられる瞬間、
        // プレイヤーが子オブジェクトになったままだとプレイヤーも一緒に消滅（エラー/即死）してしまう！
        // それを防ぐために、消滅の直前に必ず子オブジェクトを切り離して空中に放り出す。
        transform.DetachChildren();
    }
}