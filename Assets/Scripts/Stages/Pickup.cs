using UnityEngine;

public class Pickup : MonoBehaviour{
    public ItemData data;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            switch (data.itemType){
                case ItemType.Health:
                    other.GetComponent<PlayerHealth>().Heal(data.value);
                    break;
                case ItemType.SP:
                    other.GetComponent<PlayerShoot>().RecoverSp(data.value);
                    break;
                case ItemType.Stock:
                    // 後述のストックシステムに送る
                    other.GetComponent<PlayerInventory>().AddItem(data);
                    break;
                // コインだった場合の処理
                case ItemType.Coin:
                    other.GetComponent<PlayerInventory>().AddCoin(data.value);
                    break;
            }
            Destroy(gameObject);
        }
    }
}