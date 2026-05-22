/* ===================================================
 * スクリプト名 : ConveyorBelt.cs
 * Version : Ver0.01
 * Since : 2026/05/22
 * Update : 2026/05/22
 * 用途 : プレイヤー等をSurfaceEffector2Dに合わせて流す
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(SurfaceEffector2D))]
public class ConveyorBelt : MonoBehaviour {

    private SurfaceEffector2D effector;

    void Start(){
        // 同じオブジェクトに付いている SurfaceEffector2D を取得
        effector = GetComponent<SurfaceEffector2D>();
    }

    private void OnCollisionStay2D(Collision2D collision){
        // 1. プレイヤーだった場合、専用の変数（platformVelocity）に速度を渡す
        if (collision.gameObject.CompareTag("Player")){
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null){
                // エフェクターの速度をそのままX軸の移動力として与える
                pc.platformVelocity = new Vector2(effector.speed, 0f);
            }
        }

        // （※もし EnemyPatrol などもコンベアで流したい場合は、ここに敵用の処理を追記することも可能です）
    }
}