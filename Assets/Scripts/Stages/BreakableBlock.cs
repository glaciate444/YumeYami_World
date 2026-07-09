/* ===================================================
 * スクリプト名 : BreakableBlock.cs
 * 用途 : 破壊できるオブジェクト
 * 更新 : 鉄の箱でも、大玉(特大ダメージ)なら壊れる設定を追加
 * =================================================== */
using UnityEngine;

public class BreakableBlock : MonoBehaviour, IDamageable{
    [Header("耐久度・破壊設定")]
    [Tooltip("チェックを入れると、攻撃や踏みつけを受けても絶対に壊れない「鉄の箱」になります")]
    public bool isIndestructible = false;

    // ▼【新規追加】大玉ギミック用
    [Tooltip("チェックを入れると、鉄の箱であっても大玉(特大ダメージ)が当たった時だけ壊れます")]
    public bool canBreakByHazard = false;

    [Header("ドロップ設定")]
    public GameObject dropItemPrefab;

    [Header("エフェクト設定")]
    public GameObject breakParticlePrefab;

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // ▼【修正】壊れない設定（鉄の箱）の時の判定
        if (isIndestructible){
            // 大玉で壊せる設定がON かつ、ダメージが9999(大玉クラス)の場合は特別に壊す！
            if (canBreakByHazard && damage >= 9999) {
                // ガードを突破して下の破壊処理へ進む
            } else {
                return; // 通常の攻撃ならここで処理を止める
            }
        }

        // 1. パーティクルを生成
        if (breakParticlePrefab != null){
            Instantiate(breakParticlePrefab, transform.position, Quaternion.identity);
        }

        // 2. アイテムをドロップ
        if (dropItemPrefab != null){
            int randomInt = Random.Range(1, 6);
            for (int i = 0; i < randomInt; i++){
                Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.3f), 0f);
                Instantiate(dropItemPrefab, transform.position + randomOffset, Quaternion.identity);
            }
        }

        // 3. 自分自身を消去
        Destroy(gameObject);
    }
}