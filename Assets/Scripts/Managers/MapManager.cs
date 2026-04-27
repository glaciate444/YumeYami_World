/* ===================================================
 * スクリプト名 : MapManager.cs
 * Version : Ver0.01
 * Since : 2026/04/28
 * Update : 2026/04/28
 * 用途 : MapManager (マップ管理者): プレイヤーの移動を制御し、
 * 今どのノードにいるのか、次はどこへ移動できるのかを管理します。
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour{
    [Header("マップ設定")]
    public Transform playerIcon;      // プレイヤーの駒（ImageなどのTransform）
    public MapNode currentNode;       // 現在プレイヤーがいるノード
    public float moveSpeed = 5f;      // 駒の移動速度

    private bool isMoving = false;    // 移動中かどうかのフラグ
    private MapNode targetNode;       // 移動先のノード

    void Start(){
        // 最初に全てのノードをセットアップする（雑な実装ですが、最初はこれで十分です）
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes){
            node.SetupNode();
        }

        // プレイヤーの駒を最初のノードの位置に合わせる
        if (currentNode != null && playerIcon != null){
            playerIcon.position = currentNode.transform.position;
        }
    }

    void Update(){
        // 移動中は入力を受け付けない
        if (isMoving){
            MovePlayerIcon();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ 移動入力の処理 ▼
        MapNode nextNode = null;

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            nextNode = currentNode.upNode;
        }else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            nextNode = currentNode.downNode;
        }else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame){
            nextNode = currentNode.leftNode;
        }else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
            nextNode = currentNode.rightNode;
        }

        // 次のノードが存在し、かつ解放されていれば移動を開始する
        if (nextNode != null && nextNode.IsUnlocked){
            targetNode = nextNode;
            isMoving = true;
        }

        // ▼ 決定ボタンの処理 ▼
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            if (currentNode != null && currentNode.myLevelData != null && currentNode.IsUnlocked){
                // そのノードのSceneをロードする！
                SceneManager.LoadScene(currentNode.myLevelData.sceneName);
            }
        }
    }

    private void MovePlayerIcon(){
        if (playerIcon == null || targetNode == null) return;

        // ターゲットに向かって移動
        playerIcon.position = Vector3.MoveTowards(playerIcon.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        // 到着したかチェック
        if (Vector3.Distance(playerIcon.position, targetNode.transform.position) < 0.01f){
            playerIcon.position = targetNode.transform.position; // ピタッと合わせる
            currentNode = targetNode; // 現在地を更新
            isMoving = false;         // 移動完了
        }
    }
}