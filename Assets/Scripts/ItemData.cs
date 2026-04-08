using UnityEngine;

public enum ItemType { Health, SP, Score, Stock } // アイテムの種類を定義

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject{
    public string itemName;
    public ItemType itemType; // 種類を選択できるようにする
    public int value;
    public GameObject prefab;
}