/* ===================================================
 * スクリプト名 : FallingBlock.cs
 * Version : Ver0.03
 * Since : 2026/07/24
 * Update : 2026/07/25
 * 用途 : プレイヤーが乗ると消滅するブロックのスクリプト
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingBlock : MonoBehaviour{
    public enum ActionType { Fall, Melt }

    [Header("ちくわブロック設定")]
    [Tooltip("Fall=そのまま落下, Melt=Y軸が縮んで溶ける")]
    public ActionType actionType = ActionType.Fall;
    public float fallDelay = 1.0f;
    public float shakeAmount = 0.05f;

    [Header("落下モード(Fall)専用設定")]
    public float initialFallSpeed = -2f;

    [Header("溶けるモード(Melt)専用設定")]
    public float meltDuration = 0.2f; // 縮んで消えるまでの秒数

    // ▼▼▼ 修正：復活設定の拡張 ▼▼▼
    [Header("復活設定")]
    [Tooltip("チェックを外すと、復活せずに完全に消滅(Destroy)します")]
    public bool doesRespawn = true;
    public float respawnTime = 3.0f; // 消滅してから復活するまでの秒数
    // ▲▲▲ 修正ここまで ▲▲▲

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    private float currentTimer = 0f;
    private bool isTriggered = false;
    private bool isPlayerOn = false;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    void Update(){
        if (isTriggered) return;

        if (isPlayerOn){
            currentTimer += Time.deltaTime;

            transform.position = initialPosition + new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                0f,
                0f
            );

            if (currentTimer >= fallDelay){
                StartCoroutine(ActionAndRespawnRoutine());
            }
        }else if (currentTimer > 0f){
            currentTimer = 0f;
            transform.position = initialPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) => CheckPlayerStand(collision);
    private void OnCollisionStay2D(Collision2D collision) => CheckPlayerStand(collision);

    private void OnCollisionExit2D(Collision2D collision){
        if (isTriggered) return;
        if (collision.gameObject.CompareTag("Player")) isPlayerOn = false;
    }

    private void CheckPlayerStand(Collision2D collision){
        if (isTriggered) return;

        if (collision.gameObject.CompareTag("Player")){
            if (collision.transform.position.y > transform.position.y){
                isPlayerOn = true;
            }
        }
    }

    private IEnumerator ActionAndRespawnRoutine(){
        isTriggered = true;
        isPlayerOn = false;

        transform.position = initialPosition;
        transform.DetachChildren();

        // ------------------------------------
        // 1. 選択されたアクションの実行
        // ------------------------------------
        if (actionType == ActionType.Fall){
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = new Vector2(0f, initialFallSpeed);

            // 画面外に落ちるまで待機
            // ※Destroyする場合はこの秒数後に消滅します
            yield return new WaitForSeconds(1.5f);
        }else if (actionType == ActionType.Melt){
            if (col != null) col.enabled = false;

            float time = 0;
            while (time < meltDuration){
                time += Time.deltaTime;
                float newY = Mathf.Lerp(initialScale.y, 0f, time / meltDuration);
                transform.localScale = new Vector3(initialScale.x, newY, initialScale.z);
                yield return null;
            }
        }

        // ▼▼▼ 新規追加：復活しない場合の分岐 ▼▼▼
        if (!doesRespawn){
            // 復活しない設定の場合は、ここで完全にオブジェクトを削除して処理を終わる
            Destroy(gameObject);
            yield break; // これより下の処理（復活処理）は実行されない
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        // ------------------------------------
        // 2. 完全消去（非表示）にして復活を待機
        // ------------------------------------
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        transform.position = initialPosition;

        yield return new WaitForSeconds(respawnTime);

        // ------------------------------------
        // 3. 復活処理
        // ------------------------------------
        transform.localScale = initialScale;

        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;

        currentTimer = 0f;
        isTriggered = false;
    }
}