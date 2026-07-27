/* ===================================================
 * スクリプト名 : WorldSelectManager.cs
 * 用途 : 大マップ（ワールド選択）のカーソル移動とシーン遷移
 * 拡張 : 決定ボタンの連打バグを防ぐフラグを追加 / ストーリー既読判定を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class WorldSelectManager : MonoBehaviour {
    [Header("設定")]
    public Transform playerIcon;
    public WorldNode currentNode;
    public float moveSpeed = 500f;

    private bool isMoving = false;
    private WorldNode targetNode;

    // シーン遷移が始まったら true にして入力の連打を防ぐ
    private bool isStartingWorld = false;

    void Start()
    {
        WorldNode[] allNodes = FindObjectsByType<WorldNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            node.SetupNode();
        }

        // GameManagerの記憶からスタート位置を復元
        if (GameManager.Instance != null)
        {
            int savedNodeNum = GameManager.Instance.currentWorldNodeNumber;
            foreach (var node in allNodes)
            {
                if (node.myWorldData != null && node.myWorldData.worldNumber == savedNodeNum)
                {
                    currentNode = node;
                    break;
                }
            }
        }

        if (currentNode != null && playerIcon != null)
        {
            playerIcon.position = currentNode.transform.position;
        }
    }

    void Update()
    {
        // すでに画面遷移が始まっていたら、これ以下の処理（キー入力）を一切無視する！
        if (isStartingWorld) return;

        if (isMoving)
        {
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

        if (nextNode != null && nextNode.IsUnlocked)
        {
            targetNode = nextNode;
            isMoving = true;
        }

        // 決定ボタンで遷移！
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            if (currentNode != null && currentNode.myWorldData != null && currentNode.IsUnlocked)
            {

                // ワールドに入ることが確定したら、フラグをONにして連打をロックする！
                isStartingWorld = true;

                // ▼▼▼ ここから修正：ストーリーの既読判定 ▼▼▼
                WorldData targetWorld = currentNode.myWorldData;

                // 1. セーブデータから「既読フラグ」を読み込む（1なら既読、0なら未読）
                string saveKey = "StoryWatched_World_" + targetWorld.worldNumber;
                bool isStoryWatched = PlayerPrefs.GetInt(saveKey, 0) == 1;

                string sceneToLoad = targetWorld.sceneName; // デフォルトは直接マップへ遷移

                // 2. 「未読」かつ「ストーリーシーン名が設定されている」場合はストーリーシーンへ上書き
                if (!isStoryWatched && !string.IsNullOrEmpty(targetWorld.storySceneName))
                {
                    sceneToLoad = targetWorld.storySceneName;
                }

                // ロード実行
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(sceneToLoad, TransitionType.Fade);
                }
                else
                {
                    SceneManager.LoadScene(sceneToLoad);
                }
                // ▲▲▲ 修正ここまで ▲▲▲
            }
        }
    }

    private void MovePlayerIcon()
    {
        if (playerIcon == null || targetNode == null) return;

        playerIcon.position = Vector3.MoveTowards(playerIcon.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(playerIcon.position, targetNode.transform.position) < 0.01f)
        {
            playerIcon.position = targetNode.transform.position;
            currentNode = targetNode;
            isMoving = false;

            if (GameManager.Instance != null && currentNode.myWorldData != null)
            {
                GameManager.Instance.currentWorldNodeNumber = currentNode.myWorldData.worldNumber;
                GameManager.Instance.SaveGame();
            }
        }
    }
}