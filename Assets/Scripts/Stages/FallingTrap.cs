/* ===================================================
* スクリプト名 : FallingTrap.cs
 * Version : Ver0.01
 * Since : 2026/05/22
 * Update : 2026/05/22
* 用途 : プレイヤーが下を通ると落下してくる罠
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingTrap : MonoBehaviour {

    // ▼ 落下のパターンの種類
    public enum DropPattern {
        WarningParticles, // パーティクルで予告した後に出現して落下
        ShakeAndDrop      // 氷柱のように、ゆらゆら揺れてから落下
    }

    // ▼ 落下後の挙動の種類
    public enum ImpactAction {
        DestroyOnImpact, // 地面やプレイヤーに当たったら砕け散る
        RemainOnGround   // 壊れずに足場や障害物としてそのまま残る
    }

    [Header("落下・予告設定")]
    public DropPattern dropPattern = DropPattern.WarningParticles;
    public float warningTime = 1.0f; // 予告（パーティクルや揺れ）の時間

    [Header("衝突後の設定")]
    public ImpactAction impactAction = ImpactAction.DestroyOnImpact;
    public GameObject breakEffectPrefab; // 砕けた時のエフェクト

    [Header("揺れ設定（ShakeAndDrop用）")]
    public float shakeMagnitude = 0.05f; // 揺れる幅

    [Header("ダメージ・衝撃")]
    public int damage = 2;
    public float impact = 10f;

    [Header("連携コンポーネント")]
    [Tooltip("予告用パーティクル（子オブジェクトに配置してアタッチしてください）")]
    public ParticleSystem warningParticles;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isTriggered = false;
    private Vector3 initialPosition;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        initialPosition = transform.position;

        // 最初は重力の影響を受けないように固定しておく
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 「WarningParticles」パターンの場合、最初は本体を透明にしておく
        if (dropPattern == DropPattern.WarningParticles && sr != null){
            sr.enabled = false;
        }
    }

    // プレイヤーがセンサー（下に伸ばしたTriggerコライダー）に触れたら発動
    private void OnTriggerEnter2D(Collider2D other){
        if (!isTriggered && other.CompareTag("Player")){
            StartCoroutine(DropRoutine());
        }
    }

    private IEnumerator DropRoutine(){
        isTriggered = true;

        // ▼ パターンごとの予告演出 ▼
        if (dropPattern == DropPattern.WarningParticles){
            // パーティクルを再生し、指定時間待ってから本体を表示
            if (warningParticles != null) warningParticles.Play();
            yield return new WaitForSeconds(warningTime);
            if (sr != null) sr.enabled = true;
        }else if (dropPattern == DropPattern.ShakeAndDrop){
            // 指定時間の間、左右にランダムに揺らす
            float elapsed = 0f;
            while (elapsed < warningTime){
                float x = initialPosition.x + Random.Range(-shakeMagnitude, shakeMagnitude);
                transform.position = new Vector3(x, initialPosition.y, initialPosition.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            // 揺れが終わったら元のX座標に戻す
            transform.position = initialPosition;
        }

        // ▼ 落下開始 ▼
        // キネマティック（固定）からダイナミック（物理演算）に変更し、重力で落とす
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    // 落下中に何かに激突した時の処理
    private void OnCollisionEnter2D(Collision2D other){
        if (!isTriggered) return;

        // ▼【追加】天井に触れた判定で即座に自爆するのを防ぐ処理 ▼
        bool isHitFromBottom = false;

        // ぶつかったすべてのポイント（接点）を確認する
        foreach (ContactPoint2D contact in other.contacts){
            // 接点が罠の中心より「下」であれば、地面かプレイヤーに当たったと判定
            if (contact.point.y < transform.position.y + 0.1f){
                isHitFromBottom = true;
                break;
            }
        }

        // 下にぶつかっていない（＝天井に触れているだけ）なら、ここで処理を止める
        if (!isHitFromBottom) return;

        // 1. プレイヤーに当たった場合はダメージと衝撃を与える
        if (other.gameObject.CompareTag("Player")){
            IDamageable target = other.gameObject.GetComponent<IDamageable>();
            if (target != null){
                // 斜め下方向に衝撃を計算
                Vector2 dir = (other.transform.position - transform.position).normalized;
                target.TakeDamage(damage, dir * impact);
            }
        }

        // 2. 衝突後の処理（砕ける or 残る）
        if (impactAction == ImpactAction.DestroyOnImpact){
            if (breakEffectPrefab != null){
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject); // 自分自身を消滅
        }else{
            // 残る場合は、これ以上ダメージを与えないようにスクリプト自体をオフにする
            this.enabled = false;
        }
    }
}