using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/ItemData")]
public class ItemData : ScriptableObject{
    public string itemName;
    public int value;      // スコア量や回復量
    public GameObject prefab; // 敵がドロップする際の見た目
    public bool isHealth;  // HP回復アイテムかどうかの判定用
}