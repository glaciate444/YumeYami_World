/* ===================================================
 * スクリプト名 : BreakableBlock.cs
 * Version : Ver0.03
 * Since : 2026/04/06
 * Update : 2026/05/05
 * 用途 : 破壊できるオブジェクト、木箱(Box Collider 2D)、ブロックなど(Tilemap Collider 2D)
 * 更新 : 木箱を踏むと破壊できるか設定
 * =================================================== */
using UnityEngine;

public class BreakableBlock : MonoBehaviour, IDamageable{
    [Header("耐久度・破壊設定")]
    [Tooltip("チェックを入れると、攻撃や踏みつけを受けても絶対に壊れない「鉄の箱」になります")]
    public bool isIndestructible = false;

    [Header("ドロップ設定")]
    [Tooltip("壊した時に出すアイテム（ドロップ用コインのプレハブなど）")]
    public GameObject dropItemPrefab;

    [Header("エフェクト設定")]
    [Tooltip("壊れた時の破片パーティクル")]
    public GameObject breakParticlePrefab;

    // 叩かれたら無条件で壊れる
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // ▼【追加】壊れない設定（鉄の箱）なら、ここで処理を止めて何もしない
        if (isIndestructible){
            return;
        }

        // 1. パーティクルを生成（設定されていれば）
        if (breakParticlePrefab != null){
            // 木箱と同じ位置にパーティクルを発生させる
            Instantiate(breakParticlePrefab, transform.position, Quaternion.identity);
        }

        // 2. アイテムをドロップ（設定されていれば）
        if (dropItemPrefab != null){
            // 【修正1】1～5を出したい場合、最大値は「6」にします（整数の Random.Range は最大値を含まないため）
            int randomInt = Random.Range(1, 6);
            Debug.Log($"生成数 = {randomInt}");

            for (int i = 0; i < randomInt; i++){
                // 【修正2】生成位置（XとY）にほんの少しだけランダムなズレ（オフセット）を加える
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.3f), 0f);

                // 元の位置にズレを足して生成する
                Instantiate(dropItemPrefab, transform.position + randomOffset, Quaternion.identity);
            }
        }

        // 3. 自分自身（木箱）をシーンから消去
        Destroy(gameObject);
    }
}