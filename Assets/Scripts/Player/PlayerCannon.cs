/* ===================================================
 * スクリプト名 : PlayerCannon.cs
 * 用途 : プレイヤーを斜め等に発射する大砲ギミック
 * 修正 : 発射方向をWaitPointの向きに依存させ、エディタ上に軌道を表示
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCannon : MonoBehaviour {
    public enum LaunchMode { Auto, Manual }

    [Header("発射設定")]
    public LaunchMode mode = LaunchMode.Auto;
    public float autoDelay = 1.0f;
    public float launchPower = 25f;

    [Header("位置設定")]
    public Transform playerWaitPoint;

    private bool isPlayerInside = false;
    private PlayerController playerController;
    private float cooldownTimer = 0f;

    private void OnTriggerEnter2D(Collider2D collision){
        if (cooldownTimer > 0f) return;

        if (!isPlayerInside && collision.CompareTag("Player")){
            playerController = collision.GetComponent<PlayerController>();
            if (playerController != null){
                isPlayerInside = true;
                playerController.EnterCannon(playerWaitPoint != null ? playerWaitPoint : transform);

                if (mode == LaunchMode.Auto){
                    StartCoroutine(AutoLaunchRoutine());
                }
            }
        }
    }

    private void Update(){
        if (cooldownTimer > 0f){
            cooldownTimer -= Time.deltaTime;
        }

        if (isPlayerInside && mode == LaunchMode.Manual){
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)){
                LaunchPlayer();
            }
        }
    }

    private IEnumerator AutoLaunchRoutine(){
        yield return new WaitForSeconds(autoDelay);
        if (isPlayerInside){
            LaunchPlayer();
        }
    }

    private void LaunchPlayer(){
        isPlayerInside = false;
        cooldownTimer = 0.5f;

        if (playerController != null){
            // ▼▼▼ 修正：大砲自身の向きではなく、WaitPointの「上方向(Y軸)」に向かって飛ばす ▼▼▼
            Transform firePoint = playerWaitPoint != null ? playerWaitPoint : transform;
            Vector2 force = firePoint.up * launchPower;
            // ▲▲▲ 修正ここまで ▲▲▲

            playerController.FireFromCannon(force);
            playerController = null;
        }
    }

    // ▼▼▼ 新規追加：エディタ上で発射方向を赤い線で表示する補助機能 ▼▼▼
    private void OnDrawGizmos(){
        Transform point = playerWaitPoint != null ? playerWaitPoint : transform;

        Gizmos.color = Color.red;
        Vector3 endPos = point.position + point.up * 3f; // 3メートル先まで線を引く

        Gizmos.DrawLine(point.position, endPos);
        Gizmos.DrawSphere(endPos, 0.15f); // 先端に丸を描いて矢印っぽくする
    }
    // ▲▲▲ 新規追加ここまで ▲▲▲
}