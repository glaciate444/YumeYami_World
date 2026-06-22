/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : リザルト表示、コイン集計演出、セーブデータの保存
 * 修正 : 根本原因（フライング加算）解決に伴い、純粋な加算ロジックに修正
 * =================================================== */
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; 
using UnityEngine.InputSystem; 

public class GoalResultManager : MonoBehaviour {
    [Header("UI設定")]
    public TextMeshProUGUI stageCoinText; 
    public TextMeshProUGUI totalCoinText;

    [Header("演出設定")]
    public float countSpeed = 0.05f; 
    public AudioClip countSE;        
    public AudioClip finishSE;       

    [Header("遷移先")]
    public string mapSceneName = "MapSelectScene";

    private bool isCounting = false;
    private bool isTransitioning = false; 

    void Start() {
        if (GameManager.Instance != null) {
            // フライング加算が無くなったので、そのまま素直に表示するだけでOK！
            UpdateUI(GameManager.Instance.stageCoins, GameManager.Instance.totalCoins);
            StartCoroutine(CoinCountRoutine());
        }
    }

    void Update() {
        if (isTransitioning) return;

        bool isButtonPressed = false;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) {
            isButtonPressed = true;
        }
        if (Gamepad.current != null && 
           (Gamepad.current.buttonSouth.wasPressedThisFrame || 
            Gamepad.current.buttonEast.wasPressedThisFrame || 
            Gamepad.current.startButton.wasPressedThisFrame)) {
            isButtonPressed = true;
        }

        if (isButtonPressed) {
            if (isCounting) {
                isCounting = false;
            } else {
                StartCoroutine(WaitAndTransitionRoutine());
            }
        }
    }

    private IEnumerator CoinCountRoutine() {
        isCounting = true;

        int currentStageCoins = GameManager.Instance.stageCoins;
        int currentTotalCoins = GameManager.Instance.totalCoins; // 正しい元の値（例: 421）

        yield return new WaitForSeconds(0.5f);

        // 1枚ずつ移動させるループ
        while (currentStageCoins > 0 && isCounting) {
            currentStageCoins--;
            currentTotalCoins++;

            UpdateUI(currentStageCoins, currentTotalCoins);

            if (SoundManager.instance != null && countSE != null) {
                SoundManager.instance.PlaySE(countSE);
            }

            yield return new WaitForSeconds(countSpeed);
        }

        // ▼ スキップ時などのため、最終的な正しい値（421 + 15 = 436）を計算して反映
        int finalTotalCoins = GameManager.Instance.totalCoins + GameManager.Instance.stageCoins;
        UpdateUI(0, finalTotalCoins);

        isCounting = false;

        if (SoundManager.instance != null && finishSE != null) {
            SoundManager.instance.PlaySE(finishSE);
        }

        // GameManagerのデータを更新してセーブ！
        GameManager.Instance.totalCoins = finalTotalCoins;
        GameManager.Instance.stageCoins = 0; 
        GameManager.Instance.SaveGame();
    }

    private void UpdateUI(int stageCoins, int totalCoins) {
        if (stageCoinText != null) stageCoinText.text = stageCoins.ToString("D3");
        if (totalCoinText != null) totalCoinText.text = totalCoins.ToString("D6");
    }

    private IEnumerator WaitAndTransitionRoutine() {
        isTransitioning = true; 

        yield return new WaitForSeconds(1.0f);

        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(mapSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}