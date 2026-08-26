/* ===================================================
 * スクリプト名 : ObjectSpawner.cs
 * 用途 : 汎用キャラ・オブジェクト召喚（敵、リフト、アイテムなど）
 * 拡張 : プレイヤーが近くにいる時のみ作動する最適化機能を追加
 * =================================================== */
using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour {
    [Header("召喚設定")]
    public GameObject spawnPrefab;
    public float spawnInterval = 3.0f;
    public float initialDelay = 0.5f;

    [Header("召喚時のパラメータ")]
    public float lifeTime = 0f;
    public Vector2 initialVelocity = Vector2.zero;
    public bool inheritDirection = true;
    public Transform spawnPoint;

    // ▼▼▼ 新規追加：アクティブ範囲設定 ▼▼▼
    [Header("アクティブ範囲（最適化）")]
    [Tooltip("プレイヤーがこの距離以内にいる時だけ召喚します。0なら無限（常に召喚）")]
    public float activeDistance = 20f;
    private Transform playerTransform;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    private void Start(){
        if (spawnPoint == null) spawnPoint = transform;

        // プレイヤーを探して記憶しておく
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine(){
        yield return new WaitForSeconds(initialDelay);

        while (true){
            // ▼ 修正：プレイヤーとの距離を測り、範囲内なら召喚する
            if (IsPlayerInRange()){
                SpawnObject();
                if (spawnInterval <= 0f) break; // 1回きりの場合は終了
            }

            // 範囲外であってもインターバル時間はカウントし続ける（近づいた瞬間に連射されるのを防ぐため）
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ▼▼▼ 新規追加：距離判定メソッド ▼▼▼
    private bool IsPlayerInRange(){
        // 距離制限が0以下、またはプレイヤーがいない場合は常にtrue（召喚する）
        if (activeDistance <= 0f || playerTransform == null) return true;

        // スポナーとプレイヤーの距離を計算
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= activeDistance;
    }
    // ▲▲▲ 新規追加ここまで ▲▲▲

    public void SpawnObject(){
        if (spawnPrefab == null) return;
        GameObject spawnedObj = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);

        Vector2 finalVelocity = initialVelocity;
        float facingDirection = Mathf.Sign(transform.localScale.x);

        if (inheritDirection){
            Vector3 scale = spawnedObj.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            spawnedObj.transform.localScale = scale;
            finalVelocity.x *= facingDirection;
        }

        if (finalVelocity != Vector2.zero){
            Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = finalVelocity;
        }

        if (lifeTime > 0) Destroy(spawnedObj, lifeTime);
    }
}