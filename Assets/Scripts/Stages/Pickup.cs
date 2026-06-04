/* ===================================================
 * スクリプト名 : Pickup.cs
 * Version : Ver0.02
 * Since : 2026/04/11
 * Update : 2026/05/05
 * 用途 : アイテム
 * =================================================== */
using NUnit.Framework.Interfaces;
using UnityEngine;

public class Pickup : MonoBehaviour{
    public ItemData data;

    [Header("効果音")]
    public AudioClip itemSE;

    // ▼ すり抜けるタイプ（空中に浮いているコインなど）で呼ばれる
    private void OnTriggerEnter2D(Collider2D other){
        PickItem(other.gameObject);
    }

    // ▼ すり抜けないタイプ（地面に落ちるドロップコインなど）で呼ばれる
    private void OnCollisionEnter2D(Collision2D other){
        PickItem(other.gameObject);
    }

    // ▼ 共通の取得処理
    private void PickItem(GameObject playerObj){
        if (playerObj.CompareTag("Player")){
            PlayerInventory inventory = playerObj.GetComponent<PlayerInventory>();

            // ▼ 音を鳴らす（安全装置付き）
            if (SoundManager.instance != null){
                SoundManager.instance.PlaySE(itemSE);
            }

            // 安全装置：インベントリ系のアイテムなのにスクリプトが付いていない場合
            if ((data.itemType == ItemType.Coin || data.itemType == ItemType.Stock) && inventory == null){
                Debug.LogError("エラー：プレイヤーに PlayerInventory スクリプトがアタッチされていません！");
                return;
            }
            if(GameManager.Instance == null){
                Debug.LogError("エラー：GameManagerがありません！");
            }

            switch (data.itemType){
                case ItemType.Health:
                    playerObj.GetComponent<PlayerHealth>().Heal(data.value);
                    break;
                case ItemType.SP:
                    playerObj.GetComponent<PlayerShoot>().RecoverSp(data.value);
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
            Destroy(gameObject); // アイテムを消す
        }
    }
}