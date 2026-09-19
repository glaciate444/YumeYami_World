using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerDashHud{
    private readonly Sprite dashOnSprite;
    private readonly Sprite dashOffSprite;
    private Image[] dashIcons;

    public PlayerDashHud(Sprite dashOnSprite, Sprite dashOffSprite){
        this.dashOnSprite = dashOnSprite;
        this.dashOffSprite = dashOffSprite;
    }

    public void Initialize(){
        // ▼ タグ検索を廃止し、HUDManagerが持っている枠を使わせてもらう
        if (HUDManager.Instance != null && HUDManager.Instance.dashIconContainer != null){
            dashIcons = HUDManager.Instance.dashIconContainer.GetComponentsInChildren<Image>();
        }
    }

    public void UpdateChargeIcons(int currentCharges){
        if (dashIcons == null || dashIcons.Length == 0) return;
        for (int i = 0; i < dashIcons.Length; i++){
            dashIcons[i].sprite = i < currentCharges ? dashOnSprite : dashOffSprite;
        }
    }
}