/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.04
 * 用途 : ゴール判定とアイリスアウト遷移
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; // コルーチンを使うために追加

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    public string nextSceneName = "MapSelectScene"; // ※後でここを「ミニゲームのScene名」に変更します

    [Header("ステージ進行設定")]
    public int unlockLevelReward = 2;

    [Header("演出時間")]
    public float waitTime = 2.0f; // ポーズをとってから暗転が始まるまでの「ドヤ顔」の時間

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        if (!isGoal && other.CompareTag("Player")){
            isGoal = true;
            Debug.Log("ゴール！");

            // 1. プレイヤーの操作を奪い、ポーズを取らせる
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null){
                player.PlayGoalAction();
            }

            // ▼【修正】プレイヤーのコインを回収し、GameManagerの一時枠（stageCoins）にそのまま記憶させる
            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null && GameManager.Instance != null){
                // ※inventory.currentCoins の部分は、お使いのインベントリの変数名に合わせてください
                GameManager.Instance.stageCoins = inventory.currentCoins;
                Debug.Log($"ゴール！ ステージコイン {GameManager.Instance.stageCoins} 枚をミニゲームへ引き継ぎます。");
            }

            // 2. GameManagerの進行度を更新
            if (GameManager.Instance != null){
                if (GameManager.Instance.unlockedStageLevel < unlockLevelReward){
                    GameManager.Instance.unlockedStageLevel = unlockLevelReward;
                }
            }

            // 3. 待ち時間＆アイリスアウトをコルーチンで開始
            StartCoroutine(GoalRoutine(other.transform));
        }
    }

    private IEnumerator GoalRoutine(Transform playerTransform){
        // ポーズを見せるために指定した時間（2秒）だけ待機
        yield return new WaitForSeconds(waitTime);

        // アイリスアウトのマネージャーを探して起動！
        IrisTransitionManager iris = FindFirstObjectByType<IrisTransitionManager>();
        if (iris != null){
            iris.StartIrisOut(playerTransform, nextSceneName);
        } else {
            // マネージャーが無ければ保険として普通に遷移
            SceneManager.LoadScene(nextSceneName);
        }
    }
}