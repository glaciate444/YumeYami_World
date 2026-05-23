/* ===================================================
 * スクリプト名 : EnemyActivator.cs
 * Version : Ver0.01a
 * Since : 2026/05/23
 * Update : 2026/05/23
 * 用途 : 画面外（プレイヤーから遠い時）は敵の処理を止めて軽くする
 * =================================================== */
using UnityEngine;

public class EnemyActivator : MonoBehaviour{
    [Header("起動設定")]
    [Tooltip("プレイヤーがこの距離以内に近づいたら動き出す")]
    public float activationDistance = 25f;

    [Tooltip("画面外で落下し続けないよう、初期位置を記憶して戻すか？")]
    public bool resetPositionWhenFar = true;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private MonoBehaviour[] allScripts;

    private bool isActive = false;
    private Vector3 initialPosition;

    void Start(){
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        initialPosition = transform.position;

        // 自分に付いている「EnemyActivator 以外の」すべてのスクリプトを取得
        allScripts = GetComponents<MonoBehaviour>();

        // 最初は強制的にスリープ状態にする
        SleepEnemy();
    }

    void Update(){
        if (player == null) return;

        // プレイヤーとの距離を測る
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= activationDistance && !isActive){
            WakeUpEnemy();
        }else if (distance > activationDistance && isActive){
            SleepEnemy();
        }
    }

    private void WakeUpEnemy(){
        isActive = true;

        // 物理演算をONにする
        if (rb != null){
            rb.simulated = true;
        }

        // アニメーションを再開する
        if (anim != null){
            anim.enabled = true;
        }

        // 他のスクリプト（Enemy.cs, EnemyTurret.cs, EnemyPatrol.cs等）をONにする
        foreach (MonoBehaviour script in allScripts){
            if (script != this) script.enabled = true;
        }
    }

    private void SleepEnemy(){
        isActive = false;

        // 物理演算をOFFにする（落下もストップする）
        if (rb != null){
            rb.simulated = false;
        }

        // アニメーションを停止する
        if (anim != null){
            anim.enabled = false;
        }

        // 他のスクリプトをOFFにする
        foreach (MonoBehaviour script in allScripts){
            if (script != this) script.enabled = false;
        }

        // 遠くに離れたら、初期位置にリセットする（穴落ち防止）
        if (resetPositionWhenFar){
            transform.position = initialPosition;
        }
    }
}