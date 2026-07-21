/* ===================================================
 * スクリプト名 : EquipHUD.cs
 * Version : Ver0.01
 * Since : 2026/07/21
 * Update : 2026/07/21
 * 用途 : 各スロットに装備したデータをHUD側で受け取る
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class EquipHUD : MonoBehaviour {
    [Header("HUDのアイコン画像（中身）")]
    public Image attackIcon;  // Z枠のアイコン（Attack_Image_Icon）
    public Image actionIcon;  // X枠のアイコン（Action_Image_Icon）
    public Image specialIcon; // C枠のアイコン（Special_Image_Icon）

    void Start(){
        // シーン開始時に、初期装備をHUDに反映させる
        UpdateHUD();
    }

    /// <summary>
    /// プレイヤーの装備情報を読み取り、HUDのアイコンを更新する
    /// </summary>
    public void UpdateHUD(){
        // プレイヤーの各スクリプトを取得
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        PlayerShoot ps = FindFirstObjectByType<PlayerShoot>();

        // 攻撃判定は子要素にあるため GetComponentInChildren を使用
        PlayerAttack pa = null;
        if (pc != null){
            pa = pc.GetComponentInChildren<PlayerAttack>(true);
        }

        // ▼ Z枠（通常攻撃）の更新
        if (pa != null && pa.currentWeaponEquip != null){
            attackIcon.sprite = pa.currentWeaponEquip.icon;
            attackIcon.color = Color.white; // 不透明にして表示
        }else if (attackIcon != null){
            attackIcon.color = new Color(1, 1, 1, 0); // 何も装備していなければ透明にして隠す
        }

        // ▼ X枠（サブアクション）の更新
        if (pc != null && pc.currentSubActionEquip != null){
            actionIcon.sprite = pc.currentSubActionEquip.icon;
            actionIcon.color = Color.white;
        }else if (actionIcon != null){
            actionIcon.color = new Color(1, 1, 1, 0);
        }

        // ▼ C枠（スペシャル）の更新
        if (ps != null && ps.currentSpecialEquip != null){
            specialIcon.sprite = ps.currentSpecialEquip.icon;
            specialIcon.color = Color.white;
        }else if (specialIcon != null){
            specialIcon.color = new Color(1, 1, 1, 0);
        }
    }
}