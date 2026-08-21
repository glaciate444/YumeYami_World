/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : リザルト表示、コイン集計演出、セーブデータの保存
 * 修正 : ボス戦等のスロー・停止(Time.timeScale)による進行不可バグを防止
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

    [Header("遷移先（予備）")]
    [Tooltip("記憶がない場合の予備の帰り道")]
    public string fallbackMapSceneName = "MapSelectScene";

    private bool isCounting = false;
    private bool isTransitioning = false;

    void Start(){
        // ▼▼▼ 【重要】前のシーン（ボス戦等）でスローや停止(Time.timeScale=0)になっていても強制的に1(通常)に戻す ▼▼▼
        Time.timeScale = 1f;

        if (GameManager.Instance != null){
            UpdateUI(GameManager.Instance.stageCoins, GameManager.Instance.totalCoins);
            StartCoroutine(CoinCountRoutine());
        }
    }

    void Update(){
        if (isTransitioning) return;

        bool isButtonPressed = false;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame){
            isButtonPressed = true;
        }
        if (Gamepad.current != null &&
           (Gamepad.current.buttonSouth.wasPressedThisFrame ||
            Gamepad.current.buttonEast.wasPressedThisFrame ||
            Gamepad.current.startButton.wasPressedThisFrame)){
            isButtonPressed = true;
        }

        if (isButtonPressed){
            if (isCounting){
                isCounting = false; // カウントをスキップ
            }else{
                StartCoroutine(WaitAndTransitionRoutine());
            }
        }
    }

    private IEnumerator CoinCountRoutine(){
        isCounting = true;

        int currentStageCoins = GameManager.Instance.stageCoins;
        int currentTotalCoins = GameManager.Instance.totalCoins;

        // ▼ 変更：Time.timeScaleの影響を受けない Realtime 待機にする
        yield return new WaitForSecondsRealtime(0.5f);

        while (currentStageCoins > 0 && isCounting){
            currentStageCoins--;
            currentTotalCoins++;

            UpdateUI(currentStageCoins, currentTotalCoins);

            if (SoundManager.instance != null && countSE != null){
                SoundManager.instance.PlaySE(countSE);
            }

            // ▼ 変更
            yield return new WaitForSecondsRealtime(countSpeed);
        }

        int finalTotalCoins = GameManager.Instance.totalCoins + GameManager.Instance.stageCoins;
        UpdateUI(0, finalTotalCoins);

        isCounting = false;

        if (SoundManager.instance != null && finishSE != null){
            SoundManager.instance.PlaySE(finishSE);
        }

        GameManager.Instance.totalCoins = finalTotalCoins;
        GameManager.Instance.stageCoins = 0;
        GameManager.Instance.SaveGame(); // データを保存
    }

    private void UpdateUI(int stageCoins, int totalCoins){
        if (stageCoinText != null) stageCoinText.text = stageCoins.ToString("D3");
        if (totalCoinText != null) totalCoinText.text = totalCoins.ToString("D6");
    }

    private IEnumerator WaitAndTransitionRoutine(){
        isTransitioning = true;

        // ▼ 変更：Time.timeScaleの影響を受けない Realtime 待機にする
        yield return new WaitForSecondsRealtime(1.0f);

        string nextScene = fallbackMapSceneName;
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.returnMapSceneName)){
            nextScene = GameManager.Instance.returnMapSceneName;
            Debug.Log($"記憶された小マップ [{nextScene}] へ帰還します！");
        }else{
            Debug.LogWarning("帰り道の記憶がなかったため、予備のシーンへ遷移します。");
        }

        if (SceneTransitionManager.Instance != null){
            SceneTransitionManager.Instance.LoadScene(nextScene, TransitionType.Fade);
        }else{
            SceneManager.LoadScene(nextScene);
        }
    }
}