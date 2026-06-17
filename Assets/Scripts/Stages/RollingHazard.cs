/* ===================================================
 * スクリプト名 : RollingHazard.cs
 * 用途 : 物理演算で転がり、プレイヤーを狙う障害物
 * 更新 : 破壊時の減速を防ぐための「バンパー（Trigger）センサー」に対応
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RollingHazard : MonoBehaviour{
    [Header("ダメージ設定")]
    public int damage = 2;

    [Header("初速設定")]
    public float initialForce = 3f;
    public float initialTorque = 5f;

    [Header("消滅設定")]
    public float lifeTime = 10f; 

    [Header("効果音設定")]
    public AudioClip impactSE; 
    public float impactThreshold = 2.0f; 

    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null){
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            rb.AddForce(new Vector2(direction * initialForce, 0f), ForceMode2D.Impulse);
            rb.AddTorque(-direction * initialTorque, ForceMode2D.Impulse);
        }

        Destroy(gameObject, lifeTime);
    }

    // ▼【追加】物理的にぶつかる「直前」に検知するセンサー処理
    private void OnTriggerEnter2D(Collider2D other){
        IDamageable target = other.GetComponent<IDamageable>();
        
        if (target != null){
            // 1. プレイヤーだった場合
            if (other.CompareTag("Player")){
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = Mathf.Max(knockbackDir.y, 0.5f); 
                target.TakeDamage(damage, knockbackDir);
            }
            // 2. 木箱や敵だった場合
            else {
                // 物理エンジンがブレーキをかける「前」に、センサーが触れた瞬間に破壊する！
                Vector2 dir = (other.transform.position - transform.position).normalized;
                target.TakeDamage(damage, dir);
            }
        }
    }

    // ▼【変更】通常の物理的な激突（地面や壁へのバウンドなど）
    private void OnCollisionEnter2D(Collision2D other){
        // ダメージ処理は OnTriggerEnter2D に引っ越したため、ここでは「音を鳴らすだけ」にします
        if (other.relativeVelocity.magnitude > impactThreshold){
            if (SoundManager.instance != null && impactSE != null){
                SoundManager.instance.PlaySE(impactSE);
            }
        }
    }
}