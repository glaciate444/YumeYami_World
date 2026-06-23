/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.05
 * 用途 : ゴール判定とアイリスアウト遷移
 * 修正 : 子オブジェクト（足元センサー等）による接触エラーを完全に防止
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    public string nextSceneName = "MapSelectScene"; 

    [Header("ステージ進行設定")]
    public int unlockLevelReward = 2;

    [Header("演出時間")]
    public float waitTime = 2.0f; 

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        if (!isGoal && other.CompareTag("Player")){
            
            // ▼【超重要修正】other.GetComponent ではなく、親を含めて検索する ▼
            // これにより、足元センサーが触れても確実に「プレイヤー本体」を取得できます
            PlayerController player = other.GetComponentInParent<PlayerController>();
            PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();

            // ※もし「Player」タグが付いているのに本体が見つからない場合は、ただの誤爆なので弾く
            if (player == null) return;

            isGoal = true;
            Debug.Log("ゴール！");

            // 1. プレイヤーの操作を奪い、ポーズを取らせる
            player.PlayGoalAction();

            // 2. コインの引き継ぎ（inventory は確実に取得できているので0枚にならない！）
            if (inventory != null && GameManager.Instance != null){
                GameManager.Instance.stageCoins = inventory.currentCoins;            
            }

            // 3. GameManagerの進行度を更新
            if (GameManager.Instance != null){
                if (GameManager.Instance.unlockedStageLevel < unlockLevelReward){
                    GameManager.Instance.unlockedStageLevel = unlockLevelReward;
                }
            }

            // 4. 待ち時間＆アイリスアウトをコルーチンで開始
            // ▼【重要】消えるかもしれない other.transform ではなく、確実に存在する player.transform を渡す！
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