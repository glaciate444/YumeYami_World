/* ===================================================
 * スクリプト名 : HUDManager.cs
 * 用途 : 画面上のUI（HP、SP、コイン、ダッシュ等）の統括管理
 * 拡張 : ボスHPバーの管理機能を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour {
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
    public GameObject dashIconContainer;

    // ▼▼▼ 新規追加：ボスUI用の枠 ▼▼▼
    [Header("Boss UI")]
    public GameObject bossHpContainer; // ボスHPバーの親オブジェクト（表示ON/OFF用）
    public Slider bossHpSlider;
    public TMP_Text bossHpText;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

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

    // ==========================================
    // ▼ ここから下に追加：ボス用のUI更新メソッド
    // ==========================================
    public void SetBossHpActive(bool isActive) {
        if (bossHpContainer != null) bossHpContainer.SetActive(isActive);
    }

    public void SetupBossHP(int maxHp) {
        if (bossHpSlider != null) bossHpSlider.maxValue = maxHp;
    }

    public void UpdateBossHP(float currentHp, string textValue) {
        if (bossHpSlider != null) bossHpSlider.value = currentHp;
        if (bossHpText != null) bossHpText.text = textValue;
    }
}