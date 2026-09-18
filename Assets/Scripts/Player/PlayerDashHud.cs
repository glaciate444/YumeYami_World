using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダッシュ残量アイコンの検索と表示更新を担当する、PlayerController から分離した UI ヘルパー。
/// </summary>
public sealed class PlayerDashHud{
    private readonly Sprite dashOnSprite;
    private readonly Sprite dashOffSprite;
    private Image[] dashIcons;

    public PlayerDashHud(Sprite dashOnSprite, Sprite dashOffSprite){
        this.dashOnSprite = dashOnSprite;
        this.dashOffSprite = dashOffSprite;
    }

    public void Initialize(){
        GameObject dashIconContainer = GameObject.FindWithTag("DashText");
        if (dashIconContainer != null){
            dashIcons = dashIconContainer.GetComponentsInChildren<Image>();
        }else{
            Debug.LogWarning("DashTextタグの付いたアイコンの親が見つかりません。");
        }
    }

    public void UpdateChargeIcons(int currentCharges){
        if (dashIcons == null || dashIcons.Length == 0) return;

        for (int i = 0; i < dashIcons.Length; i++){
            dashIcons[i].sprite = i < currentCharges ? dashOnSprite : dashOffSprite;
        }
    }
}
