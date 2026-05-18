/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.02
 * Since : 2026/04/08
 * Update : 2026/05/15
 * 用途 : ゴール判定とトランジション遷移
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    [Tooltip("クリア後に戻るマップ画面や、次のステージ名を指定")]
    public string nextSceneName = "MapSelectScene";

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        // まだゴールしておらず、プレイヤーが触れたら
        if (!isGoal && other.CompareTag("Player")){
            isGoal = true;
            Debug.Log("ゴール！おめでとう！");

            // ▼ 【変更】SceneTransitionManagerを使って、フェードアウトで画面遷移する ▼
            if (SceneTransitionManager.Instance != null) {
                // Enumで「Fade」を指定して、文字無しの暗転を行う
                SceneTransitionManager.Instance.LoadScene(nextSceneName, TransitionType.Fade);
            } else {
                // ※マネージャーを配置せずに、このシーン単体でテストプレイした時のための保険
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}