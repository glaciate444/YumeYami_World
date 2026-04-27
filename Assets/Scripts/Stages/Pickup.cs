using UnityEngine;

public class Pickup : MonoBehaviour{
    public ItemData data;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag("Player")){
            switch (data.itemType){
                case ItemType.Health:
                    collision.GetComponent<PlayerHealth>().Heal(data.value);
                    break;
                case ItemType.SP:
                    collision.GetComponent<PlayerShoot>().RecoverSp(data.value);
                    break;
                case ItemType.Stock:
                    // 後述のストックシステムに送る
                    collision.GetComponent<PlayerInventory>().AddItem(data);
                    break;
            }
            Destroy(gameObject);
        }
    }
}