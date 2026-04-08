/* ===================================================
 * スクリプト名 : ゴール判定用スクリプト
 * Version : Ver0.01
 * Update : 2026/04/08
 * 用途 : ゴール判定
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class GoalPoint : MonoBehaviour{
    [Header("遷移先シーン名")]
    public string nextSceneName = "Stage2";

    private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other){
        // まだゴールしておらず、プレイヤーが触れたら
        if (!isGoal && other.CompareTag("Player")){
            isGoal = true;
            Debug.Log("ゴール！おめでとう！");

            // --- シーン遷移の実行（現在はコメントアウト） ---
            // SceneManager.LoadScene(nextSceneName);
        }
    }
}