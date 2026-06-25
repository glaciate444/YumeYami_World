/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.07
 * 用途 : ゴール判定とアイリスアウト遷移
 * 拡張 : 外部（ボスなど）から強制的にゴール処理を呼び出せるように修正
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    public string nextSceneName = "MiniGameScene"; 

    [Header("ステージ進行設定（通常）")]
    public int unlockLevelReward = 2;

    [Header("ワールド進行設定（ボス専用）")]
    public bool unlocksNewWorld = false;
    public int unlockWorldReward = 2;

    [Header("演出時間")]
    public float waitTime = 2.0f; 

    private bool isGoal;

    // 通常の「触れたらゴール」の処理
    private void OnTriggerEnter2D(Collider2D other){
        if (!isGoal && other.CompareTag("Player")){
            TriggerGoal(other.gameObject); // 下のメソッドにパスする
        }
    }

    // ▼【新規追加】ボス撃破時など、外部から自動でゴール処理をスタートさせるメソッド
    public void TriggerGoal(GameObject playerObject) {
        if (isGoal) return;

        PlayerController player = playerObject.GetComponentInParent<PlayerController>();
        PlayerInventory inventory = playerObject.GetComponentInParent<PlayerInventory>();

        if (player == null) return;

        isGoal = true;
        Debug.Log("ゴール処理開始！");

        // 1. プレイヤーにポーズを取らせる
        player.PlayGoalAction();

        // 2. コイン引き継ぎ
        if (inventory != null && GameManager.Instance != null){
            GameManager.Instance.stageCoins = inventory.currentCoins;            
        }

        // 3. 進行度とワールド解放
        if (GameManager.Instance != null){
            if (GameManager.Instance.unlockedStageLevel < unlockLevelReward){
                GameManager.Instance.unlockedStageLevel = unlockLevelReward;
            }
            
            if (unlocksNewWorld) {
                if (GameManager.Instance.unlockedWorldLevel < unlockWorldReward){
                    GameManager.Instance.unlockedWorldLevel = unlockWorldReward;
                    Debug.Log($"新ワールド {unlockWorldReward} が解放されました！");
                }
            }
        }

        // 4. アイリスアウト開始
        StartCoroutine(GoalRoutine(player.transform));
    }

    private IEnumerator GoalRoutine(Transform playerTransform){
        yield return new WaitForSeconds(waitTime);

        IrisTransitionManager iris = FindFirstObjectByType<IrisTransitionManager>();
        if (iris != null){
            iris.StartIrisOut(playerTransform, nextSceneName);
        } else {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}