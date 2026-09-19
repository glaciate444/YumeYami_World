/* ===================================================
 * スクリプト名 : ConstantVelocity.cs
 * 用途 : キラー等、何かにぶつかっても絶対に減速しない移動
 * 修正 : EnemyMovementを継承させ、ノックバックでの自動停止に対応
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
// ▼ 変更：MonoBehaviour ではなく EnemyMovement を継承する
public class ConstantVelocity : EnemyMovement {
    private Rigidbody2D rb;
    private Vector2 fixedVelocity;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        fixedVelocity = rb.linearVelocity;
    }

    void FixedUpdate(){
        if (fixedVelocity != Vector2.zero){
            rb.linearVelocity = fixedVelocity;
        }
    }
}