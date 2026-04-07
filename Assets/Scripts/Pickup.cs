using UnityEngine;

public class Pickup : MonoBehaviour{
    public ItemData data;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            if (data.isHealth){
                // プレイヤーのHP回復処理を呼ぶ（PlayerHealthにメソッドを追加）
                other.GetComponent<PlayerHealth>().Heal(data.value);
            }else{
                // スコア加算処理（ScoreManagerなどがある場合）
                Debug.Log($"{data.itemName}をゲット！ スコア+{data.value}");
            }
            Destroy(gameObject);
        }
    }
}