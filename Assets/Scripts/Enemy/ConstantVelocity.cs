/* ===================================================
 * スクリプト名 : ConstantVelocity.cs
 * 用途 : キラー等、何かにぶつかっても絶対に減速しない移動
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ConstantVelocity : MonoBehaviour {
    private Rigidbody2D rb;
    private Vector2 fixedVelocity;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // ObjectSpawnerからポイッと投げられた時の「初速」を記憶する
        fixedVelocity = rb.linearVelocity;
    }

    void FixedUpdate(){
        // 記憶した速度を毎フレーム強制的に上書きし続ける
        // （これにより、衝突して速度が落ちても一瞬で元の速度に戻る）
        if (fixedVelocity != Vector2.zero){
            rb.linearVelocity = fixedVelocity;
        }
    }
}