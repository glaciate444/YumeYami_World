/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.06
 * 用途 : ゴール判定とアイリスアウト遷移
 * 拡張 : ボスステージクリア時に新しいワールドを解放する機能を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    public string nextSceneName = "MiniGameScene"; 

    [Header("ステージ進行設定（通常）")]
    public int unlockLevelReward = 2;

    // ▼【新規追加】ボス用の設定
    [Header("ワールド進行設定（ボス専用）")]
    [Tooltip("チェックを入れると、クリア時に新しいワールドが解放されます")]
    public bool unlocksNewWorld = false;
    [Tooltip("解放するワールドの番号（レベル2の島を解放するなら 2）")]
    public int unlockWorldReward = 2;

    [Header("演出時間")]
    public float waitTime = 2.0f; 

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        if (!isGoal && other.CompareTag("Player")){
            
            PlayerController player = other.GetComponentInParent<PlayerController>();
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();

            if (player == null) return;

            isGoal = true;
            Debug.Log("ゴール！");

            player.PlayGoalAction();

            if (inventory != null && GameManager.Instance != null){
                GameManager.Instance.stageCoins = inventory.currentCoins;            
            }

            if (GameManager.Instance != null){
                // 1. 通常のステージ進行度を更新
                if (GameManager.Instance.unlockedStageLevel < unlockLevelReward){
                    GameManager.Instance.unlockedStageLevel = unlockLevelReward;
                }
                
                // 2. ▼【追加】ボス設定がONなら、ワールド進行度も更新！
                if (unlocksNewWorld) {
                    if (GameManager.Instance.unlockedWorldLevel < unlockWorldReward){
                        GameManager.Instance.unlockedWorldLevel = unlockWorldReward;
                        Debug.Log($"新ワールド {unlockWorldReward} が解放されました！");
                    }
                }
            }

            StartCoroutine(GoalRoutine(player.transform));
        }
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