/* ===================================================
 * スクリプト名 : ObjectSpawner.cs
 * 用途 : 汎用キャラ・オブジェクト召喚（敵、リフト、アイテムなど）
 * 参考 : ACT4準拠（存在時間、向きの継承、発射時の初速）
 * =================================================== */
using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour{
    [Header("召喚設定")]
    [Tooltip("召喚するプレハブ（キラー、リフトなど）")]
    public GameObject spawnPrefab;

    [Tooltip("召喚する間隔（秒）。0なら1回だけ召喚します")]
    public float spawnInterval = 3.0f;

    [Tooltip("最初の召喚までの待機時間（秒）")]
    public float initialDelay = 0.5f;

    [Header("召喚時のパラメータ")]
    [Tooltip("召喚キャラが存在できる時間（0なら無限。キャラ自身のAIに任せます）")]
    public float lifeTime = 0f;

    [Tooltip("召喚時の初速（発射される勢い）。※召喚キャラにRigidbody2Dが必要です")]
    public Vector2 initialVelocity = Vector2.zero;

    [Header("発射向き（反転）の設定")]
    [Tooltip("このスポナーの向き（Xの反転）に合わせて、召喚キャラの向きと初速を反転させるか")]
    public bool inheritDirection = true;

    [Header("召喚位置")]
    [Tooltip("召喚する位置の基準（空欄ならこのスポナー自身の中心から出ます）")]
    public Transform spawnPoint;

    private void Start(){
        if (spawnPoint == null) spawnPoint = transform;

        // 召喚ルーチンを開始
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine(){
        yield return new WaitForSeconds(initialDelay);

        while (true){
            SpawnObject();

            // 間隔が0以下の場合は1回こっきりでループ終了
            if (spawnInterval <= 0f) break;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 外部（アニメーションイベント等）から強制的に召喚したい時用にpublicにしておきます
    public void SpawnObject(){
        if (spawnPrefab == null) return;

        // 1. プレハブを召喚
        GameObject spawnedObj = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);

        // 2. 発射向き（反転）の計算
        Vector2 finalVelocity = initialVelocity;
        float facingDirection = Mathf.Sign(transform.localScale.x); // 親（スポナー）が右向きなら 1、左向きなら -1

        if (inheritDirection){
            // 召喚キャラの見た目（スケール）を親の向きに合わせる
            Vector3 scale = spawnedObj.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            spawnedObj.transform.localScale = scale;

            // 初速のX方向も親の向きに合わせる（左向きなら初速がマイナスになる）
            finalVelocity.x *= facingDirection;
        }

        // 3. 発射された時の動き（初速）を適用
        if (finalVelocity != Vector2.zero){
            Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
            if (rb != null){
                // ※Unity 6 向けの新しい記述です
                rb.linearVelocity = finalVelocity;
            }else{
                Debug.LogWarning("召喚キャラに初速を与えようとしましたが、Rigidbody2Dがついていません！");
            }
        }

        // 4. 存在時間（寿命）の設定
        if (lifeTime > 0){
            // 指定時間が経過したら自動で消滅させる
            Destroy(spawnedObj, lifeTime);
        }
    }
}