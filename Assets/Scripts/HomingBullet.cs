/* ===================================================
 * スクリプト名 : HomingBullet.cs
 * 用途 : プレイヤー・敵共通のホーミング弾
 * 連携 : Bulletクラスを「継承」し、ダメージや貫通処理を完全流用
 * =================================================== */
using UnityEngine;

// ▼ 重要：MonoBehaviour ではなく「Bullet」を継承します！
public class HomingBullet : Bullet{
    [Header("ホーミング設定")]
    [Tooltip("追尾の鋭さ（旋回力）")]
    public float rotateSpeed = 200f;

    [Tooltip("狙う相手のタグ（敵弾ならPlayer、味方弾ならEnemyを設定）")]
    public string targetTag = "Player";

    [Tooltip("ターゲットの中心を狙うためのズレ")]
    public Vector2 targetOffset = new Vector2(0f, 0.5f);

    private Transform target;
    private Rigidbody2D homingRb;

    void Start(){
        homingRb = GetComponent<Rigidbody2D>();

        // 物理演算で回転させるため、重力をゼロにして動的（Dynamic）にします
        homingRb.bodyType = RigidbodyType2D.Dynamic;
        homingRb.gravityScale = 0f;

        // 生成された瞬間に、一番近いターゲットを探し出します
        FindNearestTarget();
    }

    // シーン内の targetTag を持つオブジェクトの中から、一番近いものをロックオンする
    private void FindNearestTarget(){
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);
        if (candidates.Length == 0) return;

        float minDistance = Mathf.Infinity;
        foreach (GameObject candidate in candidates){
            float dist = Vector2.Distance(transform.position, candidate.transform.position);
            if (dist < minDistance){
                minDistance = dist;
                target = candidate.transform;
            }
        }
    }

    void FixedUpdate(){
        // ターゲットが倒されたり、見つからない場合は正面へ直進し続ける
        if (target == null){
            // ※「speed」は親である Bullet.cs からそのまま引き継いで使っています
            homingRb.linearVelocity = transform.right * speed;
            return;
        }

        // ターゲットへの方向を計算
        Vector2 targetPos = (Vector2)target.position + targetOffset;
        Vector2 directionToTarget = (targetPos - (Vector2)transform.position).normalized;

        // 現在向いている方向とターゲットの方向の「角度のズレ」を計算して旋回させる
        float rotateAmount = Vector3.Cross(directionToTarget, transform.right).z;
        homingRb.angularVelocity = -rotateAmount * rotateSpeed;

        // 常に弾の正面（transform.right）に向かって進み続ける
        homingRb.linearVelocity = transform.right * speed;
    }
}