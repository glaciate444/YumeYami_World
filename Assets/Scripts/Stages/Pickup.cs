/* ===================================================
 * スクリプト名 : Pickup.cs
 * Version : Ver0.03
 * 用途 : アイテム取得
 * 修正 : 子オブジェクト（足元判定など）接触時のエラーを修正
 * =================================================== */
using UnityEngine;

public class Pickup : MonoBehaviour{
    public ItemData data;

    [Header("効果音")]
    public AudioClip itemSE;

    private void OnTriggerEnter2D(Collider2D other){
        PickItem(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other){
        PickItem(other.gameObject);
    }

    private void PickItem(GameObject playerObj){
        if (playerObj.CompareTag("Player")){
            // ▼ 修正：子オブジェクトに触れても「親（プレイヤー本体）」から取得するように変更
            PlayerInventory inventory = playerObj.GetComponentInParent<PlayerInventory>();

            if (SoundManager.instance != null){
                SoundManager.instance.PlaySE(itemSE);
            }

            if ((data.itemType == ItemType.Coin || data.itemType == ItemType.Stock) && inventory == null){
                Debug.LogError("エラー：親オブジェクトに PlayerInventory が見つかりません！");
                return;
            }
            if (data.itemType == ItemType.LifePiece && GameManager.Instance == null){
                Debug.LogError("エラー：GameManagerがありません！");
                return;
            }

            // ▼ 修正：他のステータス系も親から取得し、存在チェックを追加
            switch (data.itemType){
                case ItemType.Health:
                    PlayerHealth health = playerObj.GetComponentInParent<PlayerHealth>();
                    if (health != null) health.Heal(data.value);
                    break;
                case ItemType.SP:
                    PlayerShoot shoot = playerObj.GetComponentInParent<PlayerShoot>();
                    if (shoot != null) shoot.RecoverSp(data.value);
                    break;
                case ItemType.Stock:
                    inventory.AddItem(data);
                    break;
                case ItemType.Coin:
                    inventory.AddCoin(data.value);
                    break;
                case ItemType.LifePiece:
                    GameManager.Instance.AddLifePiece(data.value);
                    break;
            }
            Destroy(gameObject);
        }
    }
}