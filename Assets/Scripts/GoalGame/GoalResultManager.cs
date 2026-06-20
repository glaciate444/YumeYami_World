/* ===================================================
 * スクリプト名 : GoalResultManager.cs
 * 用途 : 最終的なリザルト表示と、セーブデータの保存
 * =================================================== */
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GoalResultManager : MonoBehaviour {
    [Header("UI設定")]
    public TextMeshProUGUI totalCoinText;
    public TextMeshProUGUI totalHeartText;

    [Header("遷移先")]
    public string mapSceneName = "MapSelectScene";

    void Start() {
        if (GameManager.Instance != null) {
            
            // 1. ▼【超重要】ミニゲーム終了時の最終コインを、ここで初めてトータルに加算する！
            GameManager.Instance.totalCoins += GameManager.Instance.stageCoins;

            // 2. UIに結果を表示
            if (totalCoinText != null) totalCoinText.text = GameManager.Instance.totalCoins.ToString("D3");
            if (totalHeartText != null) totalHeartText.text = GameManager.Instance.currentLifePieces.ToString("D3");

            // 3. ミニゲームの結果も含めたデータを完全に保存する
            GameManager.Instance.SaveGame();

            // 4. 次のステージのために、一時枠を0にリセットしておく
            GameManager.Instance.stageCoins = 0;
        }
    }

    // 「次へ（マップへ戻る）」ボタン用
    public void OnClickReturnToMap() {
        if (SceneTransitionManager.Instance != null) {
            SceneTransitionManager.Instance.LoadScene(mapSceneName, TransitionType.Fade);
        } else {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}