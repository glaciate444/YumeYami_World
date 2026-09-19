/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : リザルト表示、コイン集計演出、セーブデータの保存
 * 拡張 : PlayerControls完全対応（任意のボタンでスキップ）
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

    [Header("サウンド設定")]
    public AudioClip resultBGM;

    private bool isCounting = false;
    private bool isTransitioning = false;

    // ▼ 新規追加
    private PlayerControls input;

    void Awake() {
        input = new PlayerControls();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }

    void Start(){
        if (SoundManager.instance != null && resultBGM != null){
            SoundManager.instance.PlayBGM(resultBGM);
        }

        // ボス戦等で停止した時間を通常に戻す
        Time.timeScale = 1f;

        if (GameManager.Instance != null){
            UpdateUI(GameManager.Instance.stageCoins, GameManager.Instance.totalCoins);
            StartCoroutine(CoinCountRoutine());
        }
    }

    void Update(){
        if (isTransitioning) return;

        // ▼ PlayerControls対応：よく使うメインボタンのどれかを押せばOKとする
        bool isButtonPressed = input.Player.Attack.WasPressedThisFrame() || 
                               input.Player.Jump.WasPressedThisFrame() || 
                               input.Player.Dash.WasPressedThisFrame() || 
                               input.Player.Pause.WasPressedThisFrame() ||
                               (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);

        if (isButtonPressed){
            if (isCounting){
                isCounting = false; 
            }else{
                StartCoroutine(WaitAndTransitionRoutine());
            }
        }
    }

    private IEnumerator CoinCountRoutine(){
        isCounting = true;

        int currentStageCoins = GameManager.Instance.stageCoins;
        int currentTotalCoins = GameManager.Instance.totalCoins;

        yield return new WaitForSecondsRealtime(0.5f);

        while (currentStageCoins > 0 && isCounting){
            currentStageCoins--;
            currentTotalCoins++;
            UpdateUI(currentStageCoins, currentTotalCoins);

            if (SoundManager.instance != null && countSE != null){
                SoundManager.instance.PlaySE(countSE);
            }
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
        GameManager.Instance.SaveGame(); 
    }

    private void UpdateUI(int stageCoins, int totalCoins){
        if (stageCoinText != null) stageCoinText.text = stageCoins.ToString("D3");
        if (totalCoinText != null) totalCoinText.text = totalCoins.ToString("D6");
    }

// 一番下のメソッドの中身を修正 ▼
    private IEnumerator WaitAndTransitionRoutine(){
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(1.0f);

        // ▼ 修正：定数を使用
        string nextScene = SceneNames.WorldMap;
        
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.returnMapSceneName)){
            nextScene = GameManager.Instance.returnMapSceneName;
        }

        if (SceneTransitionManager.Instance != null){
            SceneTransitionManager.Instance.LoadScene(nextScene, TransitionType.Fade);
        }else{
            SceneManager.LoadScene(nextScene);
        }
    }
}