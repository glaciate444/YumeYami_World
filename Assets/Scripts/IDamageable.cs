using UnityEngine;

// ダメージを受けることができるオブジェクトの共通ルール
public interface IDamageable{
    // ダメージ量と、吹っ飛ぶ方向を受け取るメソッド
    void TakeDamage(int damage, Vector2 knockbackDirection);
}