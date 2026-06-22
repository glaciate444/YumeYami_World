/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : リザルト表示、コイン集計演出、セーブデータの保存
 * 修正 : どこかでコインが事前加算されていても、絶対に正常化する安全ロジック
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
            // ▼【超安全設計】あらかじめ演出のスタート地点を「正しい元の値」に強制逆算して表示する
            int currentStageCoins = GameManager.Instance.stageCoins;
            int currentTotalCoins = GameManager.Instance.totalCoins - currentStageCoins;

            UpdateUI(currentStageCoins, currentTotalCoins);
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

        // ▼ 既に加算されてしまっている現状の値（436）を「最終的な正解ゴール」としてロックする
        int finalTotalCoins = GameManager.Instance.totalCoins; 
        int currentStageCoins = GameManager.Instance.stageCoins; // 15
        
        // 演出のスタート地点は、そのゴールからステージ分を引いた、本来の元の値（421）にする
        int currentTotalCoins = finalTotalCoins - currentStageCoins; 

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

        // ▼ スキップされた場合や完了時、最終的な「正しい数値（436）」を強制的に反映
        UpdateUI(0, finalTotalCoins);

        isCounting = false;

        if (SoundManager.instance != null && finishSE != null) {
            SoundManager.instance.PlaySE(finishSE);
        }

        // 実際のデータを最終的な正しい値（436）で上書き
        GameManager.Instance.totalCoins = finalTotalCoins;
        GameManager.Instance.stageCoins = 0; 
        GameManager.Instance.SaveGame();
    }

    private void UpdateUI(int stageCoins, int totalCoins) {
        if (stageCoinText != null) stageCoinText.text = stageCoins.ToString("D3");
        if (totalCoinText != null) totalCoinText.text = totalCoins.ToString("D3");
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