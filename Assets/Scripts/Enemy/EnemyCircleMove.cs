/* ===================================================
 * スクリプト名 : EnemyCircleMove.cs
 * Version : Ver0.01
 * Since : 2026/05/14
 * Update : 2026/05/14
 * 用途 : 中心点を基準に円軌道で移動する敵
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyCircleMove : EnemyMovement {
    [Header("円移動設定")]
    public float speed = 3f;        // 回る速さ
    public float radius = 2f;       // 円の半径
    public bool isClockwise = true; // 時計回りかどうか

    private Vector2 centerPos;
    private float angle;
    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        // ゲーム開始時の位置を「円の中心」として記憶する
        centerPos = transform.position; 
        
        // 重力で落ちないように設定
        rb.bodyType = RigidbodyType2D.Kinematic; 
    }

    void FixedUpdate(){
        if (!this.enabled) return;

        // 時計回りか反時計回りかで角度の増減を反転させる
        float dir = isClockwise ? -1f : 1f;
        angle += speed * dir * Time.fixedDeltaTime;

        // Cos と Sin を使って円周上の座標を計算
        float x = centerPos.x + Mathf.Cos(angle) * radius;
        float y = centerPos.y + Mathf.Sin(angle) * radius;

        rb.MovePosition(new Vector2(x, y));
    }
}