/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : リザルト表示、コイン集計演出、セーブデータの保存
 * 修正 : Input System対応、ボタン入力で1秒待機後に暗転遷移
 * =================================================== */
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; 
using UnityEngine.InputSystem; // ▼【追加】キーボード・コントローラー操作に必要

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
    private bool isTransitioning = false; // ▼【追加】遷移が始まったかを判定するフラグ

    void Start() {
        if (GameManager.Instance != null) {
            UpdateUI(GameManager.Instance.stageCoins, GameManager.Instance.totalCoins);
            StartCoroutine(CoinCountRoutine());
        }
    }

    // ▼【追加】毎フレーム、プレイヤーの入力を監視する
    void Update() {
        // すでに暗転待ち状態に入っていたら、これ以上の入力は無視する
        if (isTransitioning) return;

        bool isButtonPressed = false;

        // キーボードの何かのキーが押されたか
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) {
            isButtonPressed = true;
        }
        // ゲームパッド（コントローラー）の決定・キャンセル・スタート系ボタンが押されたか
        if (Gamepad.current != null && 
           (Gamepad.current.buttonSouth.wasPressedThisFrame || 
            Gamepad.current.buttonEast.wasPressedThisFrame || 
            Gamepad.current.startButton.wasPressedThisFrame)) {
            isButtonPressed = true;
        }

        // 何かボタンが押された時の処理
        if (isButtonPressed) {
            if (isCounting) {
                // 1. カウント演出中なら、演出をスキップして一気に結果を出す
                isCounting = false;
            } else {
                // 2. カウントが終わっているなら、1秒待ってから遷移するコルーチンを呼ぶ
                StartCoroutine(WaitAndTransitionRoutine());
            }
        }
    }

    private IEnumerator CoinCountRoutine() {
        isCounting = true;

        int currentStageCoins = GameManager.Instance.stageCoins;
        int currentTotalCoins = GameManager.Instance.totalCoins;

        yield return new WaitForSeconds(0.5f);

        while (currentStageCoins > 0 && isCounting) {
            currentStageCoins--;
            currentTotalCoins++;

            UpdateUI(currentStageCoins, currentTotalCoins);

            if (SoundManager.instance != null && countSE != null) {
                SoundManager.instance.PlaySE(countSE);
            }

            yield return new WaitForSeconds(countSpeed);
        }

        // スキップされた場合や完了時、最終的な数値を反映
        currentStageCoins = 0;
        currentTotalCoins = GameManager.Instance.totalCoins + GameManager.Instance.stageCoins;
        UpdateUI(currentStageCoins, currentTotalCoins);

        isCounting = false;

        if (SoundManager.instance != null && finishSE != null) {
            SoundManager.instance.PlaySE(finishSE);
        }

        GameManager.Instance.totalCoins = currentTotalCoins;
        GameManager.Instance.stageCoins = 0; 
        GameManager.Instance.SaveGame();
    }

    private void UpdateUI(int stageCoins, int totalCoins) {
        if (stageCoinText != null) stageCoinText.text = stageCoins.ToString("D3");
        if (totalCoinText != null) totalCoinText.text = totalCoins.ToString("D3");
    }

    // ▼【追加】ボタンが押された後、1秒間待機して暗転する処理
    private IEnumerator WaitAndTransitionRoutine() {
        isTransitioning = true; // 連打されても2回以上呼ばれないようにロックを掛ける

        // ここで指定した秒数（1秒）だけピタッと待機する
        yield return new WaitForSeconds(1.0f);

        // トランジション（暗転）を呼び出してマップへ戻る
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(mapSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}