using UnityEngine;

public class BreakableBlock : MonoBehaviour, IDamageable{
    // 叩かれたら無条件で壊れる
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // 破片が飛び散るパーティクルなどを出す場合はここ
        Destroy(gameObject);
    }
}