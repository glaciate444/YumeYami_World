using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour{
    public List<ItemData> stockItems = new List<ItemData>();
    private PlayerControls inputActions;

    void Awake(){
        inputActions = new PlayerControls();
        // 例：Vキーなどでストックアイテムを使用するアクション（要InputSystem設定）
        // inputActions.Player.UseItem.performed += context => UseStockItem();
    }

    public void AddItem(ItemData item){
        stockItems.Add(item);
        Debug.Log($"{item.itemName} をストックしました。現在の所持数：{stockItems.Count}");
    }

    public void UseStockItem(){
        if (stockItems.Count > 0){
            ItemData item = stockItems[0]; // 最初のアイテムを使用
            // ここで item.itemType に合わせて回復処理などを実行
            stockItems.RemoveAt(0);
        }
    }
}