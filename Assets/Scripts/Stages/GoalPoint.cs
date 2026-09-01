/* ===================================================
 * スクリプト名 : GoalPoint.cs
 * Version : Ver0.08
 * 用途 : ゴール判定とアイリスアウト遷移
 * 拡張 : フラグ式進行度管理（リスト登録）に対応
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoalPoint : MonoBehaviour {
    [Header("遷移先シーン名")]
    public string nextSceneName = "MiniGameScene";

    // 単一変数の代入から、自ステージのフラグ登録に変更 ▼▼▼
    [Header("ステージ進行設定（フラグ式）")]
    [Tooltip("このゴールに触れた時にクリア扱いにするステージ番号")]
    public int clearedStageNumber = 1;

    [Header("ワールド進行設定（ボス専用）")]
    public bool unlocksNewWorld = false;
    [Tooltip("解放するワールド番号（フラグとして記録されます）")]
    public int unlockWorldReward = 2;

    [Header("サウンド設定")]
    [Tooltip("ゴール時に鳴らすファンファーレ")]
    public AudioClip resultBGM;

    [Header("演出時間")]
    public float waitTime = 2.0f;

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        if (!isGoal && other.CompareTag("Player")){
            TriggerGoal(other.gameObject);
        }
    }

    public void TriggerGoal(GameObject playerObject){
        if (isGoal) return;

        PlayerController player = playerObject.GetComponentInParent<PlayerController>();
        PlayerInventory inventory = playerObject.GetComponentInParent<PlayerInventory>();

        if (player == null) return;

        isGoal = true;
        Debug.Log("ゴール処理開始！");

        // ゴールした瞬間にジングルを鳴らす（前のBGMは自動で止まります）
        // ▼ 修正：SoundManagerが存在するかどうかの確認（if文）で全体を囲む
        if (SoundManager.instance != null){
            if (resultBGM != null){
                SoundManager.instance.PlayJingle(resultBGM);
            }else{
                SoundManager.instance.StopBGM();
            }
        }

        player.PlayGoalAction();

        if (inventory != null && GameManager.Instance != null){
            GameManager.Instance.stageCoins = inventory.currentCoins;
        }

        // ▼▼▼ 修正：GameManagerのリストにクリア情報を追加する ▼▼▼
        if (GameManager.Instance != null){

            // 1. このステージをクリア済みにする
            GameManager.Instance.MarkStageAsCleared(clearedStageNumber);

            // 2. ボス撃破などで新ワールドが解放される場合、イベントフラグを立てる
            if (unlocksNewWorld){
                string worldFlag = "Unlocked_World_" + unlockWorldReward;
                GameManager.Instance.AddEventFlag(worldFlag);
                Debug.Log($"新ワールド解放フラグ【{worldFlag}】が立ちました！");
            }
        }else{
            Debug.LogWarning("【テストモード】GameManagerがいないため、フラグ保存はスキップされます。");
        }
        // ▲▲▲ 修正ここまで ▲▲▲

        StartCoroutine(GoalRoutine(player.transform));
    }

    private IEnumerator GoalRoutine(Transform playerTransform){
        yield return new WaitForSeconds(waitTime);

        IrisTransitionManager iris = FindFirstObjectByType<IrisTransitionManager>();
        if (iris != null){
            iris.StartIrisOut(playerTransform, nextSceneName);
        }else{
            SceneManager.LoadScene(nextSceneName);
        }
    }
}