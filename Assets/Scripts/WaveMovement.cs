/* ===================================================
 * スクリプト名 : WaveMovement.cs
 * 用途 : 弾をサイン波（らせん状）に飛ばす追加コンポーネント
 * 使い方 : 弾のプレハブにこれを追加するだけで波打ちます
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaveMovement : MonoBehaviour {
    [Header("波の動き設定")]
    [Tooltip("波の大きさ（上下の幅）")]
    public float magnitude = 5f;

    [Tooltip("波の速さ（うねりの周期）")]
    public float frequency = 10f;

    [Tooltip("チェックを入れると、波のスタートが逆（下）になります。2つ交差させる時に使います")]
    public bool invertWave = false;

    private Rigidbody2D rb;
    private float time;
    private float baseSpeed;
    private Vector2 baseDirection;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // 既存の Bullet.cs が設定した「初速（向かっている方向とスピード）」を読み取って基準にする
        baseSpeed = rb.linearVelocity.magnitude;
        baseDirection = rb.linearVelocity.normalized;

        // 念のため、速度がうまく取れなかった場合の予備設定
        if (baseSpeed == 0f){
            baseSpeed = 5f;
            baseDirection = transform.right;
        }
    }

    void FixedUpdate(){
        time += Time.fixedDeltaTime;

        // 進行方向に対して直角（90度）の向きを計算する
        Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);

        // コサイン関数を使って、上下に揺れる「速度」を計算する
        float waveSpeed = Mathf.Cos(time * frequency) * magnitude;

        // 逆位相（下スタート）の場合は数値を反転させる
        if (invertWave) waveSpeed = -waveSpeed;

        // 「前に直進する速度」＋「上下に波打つ速度」を合体させて、物理エンジンに渡す
        rb.linearVelocity = (baseDirection * baseSpeed) + (perpendicular * waveSpeed);
    }
}