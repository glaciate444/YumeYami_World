/* ===================================================
 * スクリプト名 : WarpPoint.cs
 * 用途 : 上キーで発動する一方通行ワープ（入り口専用）
 * 解決 : ワープ中の時間停止、カメラの瞬間同期、既存マネージャーの活用
 * =================================================== */
using UnityEngine;
using System.Collections;

public class WarpPoint : MonoBehaviour{
    [Header("ワープ先の座標（出口のオブジェクトを指定）")]
    public Transform destination;

    // ワープの重複発動を防ぐ全体フラグ
    private static bool globalIsWarping = false;

    private void OnTriggerStay2D(Collider2D other){
        // ワープ先がセットされていない、またはすでにワープ中の場合は何もしない
        if (globalIsWarping || destination == null) return;

        if (other.CompareTag("Player")){
            PlayerController pc = other.GetComponent<PlayerController>();

            // プレイヤーが上キーを押しているか判定
            if (pc != null && pc.MoveInputY > 0.5f){
                StartCoroutine(WarpExecution(pc));
            }
        }
    }

    private IEnumerator WarpExecution(PlayerController pc){
        globalIsWarping = true;
        pc.isWarping = true; // プレイヤーの操作を完全ロック

        // 1. ワープ中の物理演算と時間を止める（敵のアクティブ停止）
        Rigidbody2D rb = pc.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false; // 重力や慣性を無効化
        Time.timeScale = 0f;

        bool isTransitionFinished = false;

        // 2. SceneTransitionManagerのフェード機能を呼び出し
        SceneTransitionManager.Instance.WarpInSameScene(() => {

            // ▼ プレイヤーを目的地に瞬間移動
            pc.transform.position = destination.position;

            // ▼ カメラの追従バグを防ぐため、カメラも瞬間同期させる
            Camera mainCam = Camera.main;
            if (mainCam != null){
                Behaviour brain = mainCam.GetComponent("CinemachineBrain") as Behaviour;
                if (brain != null) brain.enabled = false; // Cinemachineの追従を一時OFF

                mainCam.transform.position = new Vector3(destination.position.x, destination.position.y, mainCam.transform.position.z);

                if (brain != null) brain.enabled = true;  // Cinemachineの追従をONに戻す
            }

            isTransitionFinished = true; // 処理完了
        });

        // 暗転とワープの処理が終わるまで待機
        while (!isTransitionFinished){
            yield return null;
        }

        // 3. ワープ完了、操作と時間を元に戻す
        if (rb != null) rb.simulated = true;
        Time.timeScale = 1f;
        pc.isWarping = false;
        globalIsWarping = false;
    }
}