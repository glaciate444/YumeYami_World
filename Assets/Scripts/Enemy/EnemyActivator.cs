/* ===================================================
 * スクリプト名 : EnemyActivator.cs
 * 用途 : 画面外（プレイヤーから遠い時）は敵の処理を止めて軽くする
 * 修正 : 敵が死亡している場合は干渉しないよう安全装置を追加
 * =================================================== */
using UnityEngine;

public class EnemyActivator : MonoBehaviour{
    [Header("起動設定")]
    public float activationDistance = 25f;
    public bool resetPositionWhenFar = true;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private MonoBehaviour[] allScripts;
    private Enemy enemy; // ▼ 追加：Enemyの生死を確認するための参照

    private bool isActive = false;
    private Vector3 initialPosition;

    void Start(){
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemy = GetComponent<Enemy>(); // ▼ 追加
        initialPosition = transform.position;

        allScripts = GetComponents<MonoBehaviour>();

        SleepEnemy();
    }

    void Update(){
        if (player == null) return;
        
        // ▼▼▼ 追加：敵が死んでいる場合は、以降の処理（スリープや初期位置リセット）を完全シャットアウト！
        if (enemy != null && enemy.isDead) return; 

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= activationDistance && !isActive){
            WakeUpEnemy();
        }else if (distance > activationDistance && isActive){
            SleepEnemy();
        }
    }

    private void WakeUpEnemy(){
        isActive = true;
        if (rb != null) rb.simulated = true;
        if (anim != null) anim.enabled = true;

        foreach (MonoBehaviour script in allScripts){
            if (script != this) script.enabled = true;
        }
    }

    private void SleepEnemy(){
        isActive = false;
        if (rb != null) rb.simulated = false;
        if (anim != null) anim.enabled = false;

        foreach (MonoBehaviour script in allScripts){
            if (script != this) script.enabled = false;
        }

        if (resetPositionWhenFar){
            transform.position = initialPosition;
        }
    }
}