/* ===================================================
 * スクリプト名 : MapManager.cs
 * Version : Ver0.04
 * Since : 2026/04/28
 * Update : 2026/06/23
 * 用途 : MapManager (マップ管理者): プレイヤーの移動を制御し、
 * 今どのノードにいるのか、次はどこへ移動できるのかを管理します。
 * 拡張 : GameManagerの記憶からスタート位置を復元する機能を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapManager : MonoBehaviour{
    [Header("マップ設定")]
    public Transform playerIcon;
    public MapNode currentNode;
    public float moveSpeed = 500f;

    [Header("道（ライン）の設定")]
    public GameObject linePrefab;    
    public Transform lineContainer;  
    public Color lockedLineColor = new Color(0.3f, 0.3f, 0.3f); 
    public Color unlockedLineColor = new Color(0.8f, 0.6f, 0.9f); 

    private bool isMoving = false;
    private MapNode targetNode;

    [Header("ワールドマップ（大マップ）へ戻る設定")]
    public string worldMapSceneName = "WorldMapScene"; // ← 先ほど作った大マップのシーン名を入れてください

    void Start(){
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes){
            node.SetupNode();
        }

        DrawAllPaths(allNodes);

        // ▼【超重要追加】GameManagerに記憶された stageNumber から、スタート位置のノードを探す
        if (GameManager.Instance != null) {
            int savedNodeNum = GameManager.Instance.currentMapNodeNumber;
            foreach (var node in allNodes) {
                // 自分にセットされたLevelDataのstageNumberが、記憶と一致したらそこを現在地にする
                if (node.myLevelData != null && node.myLevelData.stageNumber == savedNodeNum) {
                    currentNode = node;
                    break;
                }
            }
        }

        if (currentNode != null && playerIcon != null){
            playerIcon.position = currentNode.transform.position;
        }
    }

    private void DrawAllPaths(MapNode[] allNodes){
        if (linePrefab == null || lineContainer == null) return;

        foreach (var node in allNodes){
            DrawLine(node, node.upNode);
            DrawLine(node, node.downNode);
            DrawLine(node, node.leftNode);
            DrawLine(node, node.rightNode);
        }
    }

    private void DrawLine(MapNode fromNode, MapNode toNode){
        if (fromNode == null || toNode == null) return;

        if (fromNode.GetInstanceID() > toNode.GetInstanceID()) return;

        GameObject lineObj = Instantiate(linePrefab, lineContainer, false);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        Image lineImage = lineObj.GetComponent<Image>();

        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();

        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.anchoredPosition = fromRect.anchoredPosition;

        Vector2 dir = toRect.anchoredPosition - fromRect.anchoredPosition;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.sizeDelta = new Vector2(distance, 15f);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        if (fromNode.IsUnlocked && toNode.IsUnlocked){
            lineImage.color = unlockedLineColor;
        }else{
            lineImage.color = lockedLineColor;
        }

        lineRect.SetAsFirstSibling();
    }

    void Update(){
        if (isMoving){
            MovePlayerIcon();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼【新規追加】Xキー、またはEscキーで大マップへ戻る
        if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) {
            if (SceneTransitionManager.Instance != null) {
                SceneTransitionManager.Instance.LoadScene(worldMapSceneName, TransitionType.Fade);
            } else {
                SceneManager.LoadScene(worldMapSceneName);
            }
            return; // 戻る時はこれ以下の処理をしない
        }

        MapNode nextNode = null;

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) nextNode = currentNode.upNode;
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) nextNode = currentNode.downNode;
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) nextNode = currentNode.leftNode;
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) nextNode = currentNode.rightNode;

        if (nextNode != null && nextNode.IsUnlocked){
            targetNode = nextNode;
            isMoving = true;
        }

        // ▼【修正箇所】MapManager.cs の Update() 内の下の方 ▼

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            if (currentNode != null && currentNode.myLevelData != null && currentNode.IsUnlocked){
                
                // ▼【新規追加】ステージに入る直前に、今いるマップのシーン名をGameManagerに記憶させる！
                if (GameManager.Instance != null) {
                    GameManager.Instance.returnMapSceneName = SceneManager.GetActiveScene().name;
                }

                // トランジション付きのロード
                SceneTransitionManager.Instance.LoadCourse(
                    currentNode.myLevelData.sceneName,
                    currentNode.myLevelData.levelName
                );
            }
        }
    }

    private void MovePlayerIcon(){
        if (playerIcon == null || targetNode == null) return;

        playerIcon.position = Vector3.MoveTowards(playerIcon.position, targetNode.transform.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(playerIcon.position, targetNode.transform.position) < 0.01f){
            playerIcon.position = targetNode.transform.position;
            currentNode = targetNode;
            isMoving = false;

            // ▼【追加】ノードの移動が終わった瞬間に、GameManagerの記憶を上書きしてセーブする
            if (GameManager.Instance != null && currentNode.myLevelData != null) {
                GameManager.Instance.currentMapNodeNumber = currentNode.myLevelData.stageNumber;
                GameManager.Instance.SaveGame(); // 途中でゲームを落としても現在地を維持するため保存
            }
        }
    }
}