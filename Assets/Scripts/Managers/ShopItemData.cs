/* ===================================================
 * スクリプト名 : ShopItemData.cs
 * 用途 : ショップに並べるアイテムのデータ定義
 * =================================================== */
using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "GameData/ShopItem")]
public class ShopItemData : ScriptableObject {
    public enum ItemEffectType {
        Immediate_HpUp,      // 即効性：最大HP上昇
        Immediate_SpUp,      // 即効性：最大SP上昇
        Immediate_WeaponUp,  // 即効性：武器レベル上昇
        Passive_Inventory    // パッシブ：インベントリへ追加
    }

    [Header("基本情報")]
    public string itemName;
    public int price;
    public Sprite itemIcon;

    [TextArea(2, 3)]
    public string description;

    [Header("効果設定")]
    public ItemEffectType effectType;

    [Tooltip("上昇量や、パッシブアイテムのIDなど")]
    public int effectValue;

    [Header("インベントリ連携（Passive_Inventory選択時のみ）")]
    [Tooltip("この商品を買った時にインベントリに追加するアイテムのデータをセットします")]
    public ItemInventoryData inventoryItemData;
}