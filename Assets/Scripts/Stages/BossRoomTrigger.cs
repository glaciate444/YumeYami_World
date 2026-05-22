/* ===================================================
 * スクリプト名 : BossRoomTrigger.cs
 * Version : Ver0.02
 * Since : 2026/05/23
 * Update : 2026/05/23
 * 用途 : プレイヤーがボス部屋に入ったことを検知し、戦闘を開始する
 * 拡張 : ボス部屋専用カメラへの切り替え対応
 * =================================================== */
using UnityEngine;

public class BossRoomTrigger : MonoBehaviour{
    [Header("連携するボス")]
    public Boss targetBoss;

    [Header("入り口の壁（オプション）")]
    [Tooltip("入った後に退路を断つための見えない壁などがあればセット")]
    public GameObject entranceBlocker;

    [Header("カメラ切り替え")]
    [Tooltip("ボス部屋全体を映す固定カメラ（CinemachineCamera）のオブジェクトをセット")]
    public GameObject bossCameraObj;

    private bool hasTriggered = false;

    void Start(){
        // 最初は入り口の壁をオフにして、通れるようにしておく
        if (entranceBlocker != null){
            entranceBlocker.SetActive(false);
        }

        // 最初はボス部屋カメラをOFFにしておく（メインカメラが優先される）
        if (bossCameraObj != null){
            bossCameraObj.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (hasTriggered) return;

        if (other.CompareTag("Player")){
            hasTriggered = true;
            Debug.Log("プレイヤーがボス部屋に侵入！");

            // 1. 退路を断つ
            if (entranceBlocker != null){
                entranceBlocker.SetActive(true);
            }

            // 2. ボス部屋カメラをONにする（これで自動的にブレンドして切り替わります）
            if (bossCameraObj != null){
                bossCameraObj.SetActive(true);
            }

            // 3. ボス戦の演出を開始する
            if (targetBoss != null){
                targetBoss.StartBossBattle();
            }
        }
    }
}