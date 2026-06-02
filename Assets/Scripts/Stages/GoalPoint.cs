/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.03
 * 用途 : ゴール判定とトランジション遷移
 * 更新 : クリア時にGameManagerの進行度を更新する機能を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    [Tooltip("クリア後に戻るマップ画面や、次のステージ名を指定")]
    public string nextSceneName = "MapSelectScene";

    // ▼【追加】クリアした時に解放するレベル（進行度）
    [Header("ステージ進行設定")]
    [Tooltip("このゴールに触れた時に、GameManagerの進行度をいくつにするか（例：1-1クリアなら2）")]
    public int unlockLevelReward = 2;

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        // まだゴールしておらず、プレイヤーが触れたら
        if (!isGoal && other.CompareTag("Player")){
            isGoal = true;
            Debug.Log("ゴール！おめでとう！");

            // ▼【追加】GameManagerに「クリアしたから次のレベルを解放して！」と伝える ▼
            if (GameManager.Instance != null){
                // 現在の進行度より、このゴールで得られる進行度の方が大きければ上書き更新する
                if (GameManager.Instance.unlockedStageLevel < unlockLevelReward){
                    GameManager.Instance.unlockedStageLevel = unlockLevelReward;
                    
                    // ※もし今後 GameManager にセーブ機能（Save()など）を作った場合は、ここで呼ぶとベストです！
                }
            }

            // SceneTransitionManagerを使って、フェードアウトで画面遷移する
            if (SceneTransitionManager.Instance != null) {
                SceneTransitionManager.Instance.LoadScene(nextSceneName, TransitionType.Fade);
            } else {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}