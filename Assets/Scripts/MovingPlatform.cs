/* ===================================================
 * スクリプト名 : 動く足場スクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : 動く足場
 * 📂 MovingPlatform_Setup (空オブジェクト)
 *   🟦 Platform (足場本体。BoxCollider2D を付け、以下のスクリプトをアタッチ)
 *   🔴 PointA (空オブジェクト：スタート地点)
 *   🔴 PointB (空オブジェクト：折り返し地点)
 * =================================================== */
using UnityEngine;

public class MovingPlatform : MonoBehaviour{
    public Transform pointA;
    public Transform pointB;
    public float speed = 3f;

    private Vector3 targetPos;

    void Start(){
        targetPos = pointB.position; // 最初はPointBに向かう
    }

    void FixedUpdate(){
        // ターゲットに向かって移動
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);

        // ターゲットに到着したら、目標をもう片方に切り替える
        if (Vector3.Distance(transform.position, targetPos) < 0.05f){
            targetPos = (targetPos == pointA.position) ? pointB.position : pointA.position;
        }
    }

    // --- ここから超重要：足場にプレイヤーを乗せる処理 ---

    private void OnCollisionEnter2D(Collision2D other){
        // プレイヤーが足場に乗ったら、プレイヤーを足場の「子オブジェクト」にする
        if (other.gameObject.CompareTag("Player")){
            other.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D other){
        // プレイヤーが足場から離れたら、子オブジェクトから解除（元の階層に戻す）する
        if (other.gameObject.CompareTag("Player")){
            other.transform.SetParent(null);
        }
    }
}