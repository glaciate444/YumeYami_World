/* ===================================================
 * スクリプト名 : HUDManager.cs
 * 用途 : 画面上のUI（HP、SP、コイン、ダッシュ等）の統括管理
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour {
    // どこからでも HUDManager.Instance でアクセスできるようにする魔法（シングルトン）
    public static HUDManager Instance { get; private set; }

    [Header("HP UI")]
    public Slider healthSlider;
    public TMP_Text healthText;

    [Header("SP UI")]
    public Slider spSlider;
    public TMP_Text spText;

    [Header("Coin UI")]
    public TMP_Text coinText;

    [Header("Dash UI")]
    public GameObject dashIconContainer; // DashTextタグがついていた親オブジェクト

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // ▼ プレイヤーから数値を送ってもらってUIを更新する窓口
    public void UpdateHP(int current, int max) {
        if (healthSlider != null) {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null) healthText.text = current.ToString();
    }

    public void UpdateSP(int current, int max) {
        if (spSlider != null) {
            spSlider.maxValue = max;
            spSlider.value = current;
        }
        if (spText != null) spText.text = current.ToString();
    }

    public void UpdateCoin(int amount) {
        if (coinText != null) coinText.text = amount.ToString("D3");
    }
}