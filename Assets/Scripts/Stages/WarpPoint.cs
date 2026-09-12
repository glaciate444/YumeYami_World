/* ===================================================
 * スクリプト名 : WarpPoint.cs
 * 用途 : 上キーで発動する一方通行ワープ（入り口専用）
 * 変更 : タイムスケールを使わず、全敵の動きをPauseMovementで停止する
 * =================================================== */
using UnityEngine;
using System.Collections;

public class WarpPoint : MonoBehaviour{
    public Transform destination;
    private static bool globalIsWarping = false;

    private void OnTriggerStay2D(Collider2D other){
        if (globalIsWarping || destination == null) return;

        if (other.CompareTag("Player")){
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && pc.MoveInputY > 0.5f){
                StartCoroutine(WarpExecution(pc));
            }
        }
    }

    private IEnumerator WarpExecution(PlayerController pc){
        globalIsWarping = true;
        pc.isWarping = true;

        Rigidbody2D rb = pc.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // ▼ 変更：シーン内のすべての敵を取得し、動きを一時停止する
        EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
        foreach (var enemy in enemies){
            if (enemy != null) enemy.PauseMovement(true);
        }

        bool isTransitionFinished = false;

        SceneTransitionManager.Instance.WarpInSameScene(() => {
            pc.transform.position = destination.position;

            Camera mainCam = Camera.main;
            if (mainCam != null){
                Behaviour brain = mainCam.GetComponent("CinemachineBrain") as Behaviour;
                if (brain != null) brain.enabled = false;

                mainCam.transform.position = new Vector3(destination.position.x, destination.position.y, mainCam.transform.position.z);

                if (brain != null) brain.enabled = true;
            }

            isTransitionFinished = true;
        });

        while (!isTransitionFinished){
            yield return null;
        }

        if (rb != null) rb.simulated = true;

        // ▼ 変更：ワープ完了後、全敵の動きを再開する
        foreach (var enemy in enemies){
            if (enemy != null) enemy.PauseMovement(false);
        }

        pc.isWarping = false;
        globalIsWarping = false;
    }
}