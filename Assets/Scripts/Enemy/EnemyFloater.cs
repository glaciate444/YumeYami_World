/* ===================================================
 * スクリプト名 : EnemyFloater.cs
 * Version : Ver0.01
 * Since : 2026/05/14
 * Update : 2026/05/14
 * 用途 : 空中を上下（または左右）にフワフワ移動する敵
 * =================================================== */
using UnityEngine;

// 前回作った基底クラスを継承します！
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFloater : EnemyMovement {
    [Header("フワフワ設定")]
    public float speed = 2f;      // 動く速さ
    public float distance = 2f;   // 動く幅（振幅）
    public bool moveHorizontal = false; // チェックを入れると左右移動になります

    private Vector2 startPos;
    private float timer;
    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position; // 最初の位置を記憶
        
        // 重力で落ちないように、キネマティック（物理演算を無視してスクリプトで動かすモード）にする
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    void FixedUpdate(){
        // 親クラス（EnemyMovement）の機能により、ダメージ硬直中などは this.enabled が false になり、ここで止まります
        if (!this.enabled) return;

        timer += Time.fixedDeltaTime * speed;

        // Sin波を使って、-1 ～ 1 の間を滑らかに往復する数値を作る
        float wave = Mathf.Sin(timer) * distance;

        Vector2 newPos = startPos;
        if (moveHorizontal){
            newPos.x = startPos.x + wave;
        }else{
            newPos.y = startPos.y + wave;
        }

        rb.MovePosition(newPos);
    }
}