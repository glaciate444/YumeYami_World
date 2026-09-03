/* ===================================================
 * スクリプト名 : MerryGoRound.cs
 * Version : Ver1.01 (真の完全版)
 * 用途 : メリーゴーランドギミック
 * 解決 : 動く床(MovingPlatform.cs)と完全に同じ物理演算ロジックを適用
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 自動でRigidbody2Dを追加
public class MerryGoRound : MonoBehaviour{
    [Header("移動設定")]
    public float moveSpeedX = 2f;
    public Transform parentPole;

    [Header("馬の上下運動")]
    public float upDownSpeed = 2f;
    public float upDownHeight = 1f;
    public float timeOffset = 0f;

    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ▼ 最重要：物理エンジン同士が喧嘩しないよう、ゲーム開始時にポールから切り離す
        if (transform.parent != null){
            transform.SetParent(null);
        }
    }

    void FixedUpdate(){
        // 1. ポールの移動（制限なく、各馬が自分のポールを動かす）
        if (parentPole != null){
            parentPole.Translate(Vector3.right * moveSpeedX * Time.fixedDeltaTime);
        }

        // 2. 馬自身の移動（MovingPlatform.csと全く同じ、速度を直接与える方式）
        // サイン波の速度（微分）を計算して、物理エンジンにY軸の動きを任せる
        float velY = Mathf.Cos((Time.time + timeOffset) * upDownSpeed) * upDownHeight * upDownSpeed;
        rb.linearVelocity = new Vector2(moveSpeedX, velY);
    }

    private void OnCollisionStay2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){
            // ▼ 動く床と同じ王道の手法（Y軸の同期 ＋ X軸の同期）
            other.transform.SetParent(transform);

            PlayerController pc = other.gameObject.GetComponent<PlayerController>();
            if (pc != null){
                pc.platformVelocity = new Vector2(moveSpeedX, 0f);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){
            other.transform.SetParent(null);

            PlayerController pc = other.gameObject.GetComponent<PlayerController>();
            if (pc != null){
                pc.platformVelocity = Vector2.zero;
            }
        }
    }
}