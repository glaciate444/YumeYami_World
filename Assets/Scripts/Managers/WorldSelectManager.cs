/* ===================================================
 * スクリプト名 : WorldSelectManager.cs
 * Version : Ver0.01
 * 用途 : ワールド選択画面を動かすマネージャースクリプト
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldSelectManager : MonoBehaviour {
    [Header("設定")]
    public Transform playerIcon;
    public WorldNode currentNode;
    public float moveSpeed = 500f;

    private bool isMoving = false;
    private WorldNode targetNode;

    void Start() {
        WorldNode[] allNodes = FindObjectsByType<WorldNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes) {
            node.SetupNode();
        }

        // GameManagerの記憶からスタート位置を復元
        if (GameManager.Instance != null) {
            int savedNodeNum = GameManager.Instance.currentWorldNodeNumber;
            foreach (var node in allNodes) {
                if (node.myWorldData != null && node.myWorldData.worldNumber == savedNodeNum) {
                    currentNode = node;
                    break;
                }
            }
        }

        if (currentNode != null && playerIcon != null) {
            playerIcon.position = currentNode.transform.position;
        }
    }

    void Update() {
        if (isMoving) {
            MovePlayerIcon();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        WorldNode nextNode = null;

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) nextNode = currentNode.upNode;
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) nextNode = currentNode.downNode;
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) nextNode = currentNode.leftNode;
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) nextNode = currentNode.rightNode;

        if (nextNode != null && nextNode.IsUnlocked) {
            targetNode = nextNode;
            isMoving = true;
        }

        // 決定ボタンで、そのレベルの小マップ（MapSelectScene_Level〇）へ遷移！
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) {
            if (currentNode != null && currentNode.myWorldData != null && currentNode.IsUnlocked) {
                if (SceneTransitionManager.Instance != null) {
                    SceneTransitionManager.Instance.LoadScene(currentNode.myWorldData.sceneName, TransitionType.Fade);
                }
            }
        }
    }

    private void MovePlayerIcon() {
        if (playerIcon == null || targetNode == null) return;

        playerIcon.position = Vector3.MoveTowards(playerIcon.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(playerIcon.position, targetNode.transform.position) < 0.01f) {
            playerIcon.position = targetNode.transform.position;
            currentNode = targetNode;
            isMoving = false;

            if (GameManager.Instance != null && currentNode.myWorldData != null) {
                GameManager.Instance.currentWorldNodeNumber = currentNode.myWorldData.worldNumber;
                GameManager.Instance.SaveGame();
            }
        }
    }
}