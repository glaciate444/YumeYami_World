/* ===================================================
 * スクリプト名 : RewardPopup.cs
 * 用途 : 宝箱から出たアイテムをフワッと上に表示して消す
 * =================================================== */
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour {
    [Header("UIパーツ")]
    public Image iconImage;
    public TextMeshProUGUI amountText;

    [Header("アニメーション設定")]
    public float moveSpeed = 50f;  // 上に昇るスピード
    public float lifeTime = 1.5f;  // 何秒で消えるか

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    public void Setup(Sprite icon, int amount) {
        rect = GetComponent<RectTransform>();
        
        // 透明度をいじるためにCanvasGroupを自動追加
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // アイコンとテキストの設定
        if (iconImage != null) iconImage.sprite = icon;
        if (amountText != null) {
            // プラスの時は「+50」、マイナスの時は「-20」と表示する
            amountText.text = (amount >= 0 ? "+" : "") + amount.ToString();
            // プラスは黄色、マイナスは赤色にして分かりやすくする
            amountText.color = amount >= 0 ? Color.yellow : Color.red;
        }

        // 指定秒数後に自動で消滅させる
        Destroy(gameObject, lifeTime);
    }

    void Update() {
        // 上に移動させる
        if (rect != null) {
            rect.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
        }
        
        // 徐々に透明にする
        if (canvasGroup != null) {
            canvasGroup.alpha -= (1f / lifeTime) * Time.deltaTime;
        }
    }
}