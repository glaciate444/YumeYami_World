/* ===================================================
 * スクリプト名 : MapManager.cs
 * Version : Ver0.02
 * Since : 2026/04/28
 * Update : 2026/04/28
 * 用途 : MapManager (マップ管理者): プレイヤーの移動を制御し、
 * 今どのノードにいるのか、次はどこへ移動できるのかを管理します。
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 【追加】UI(Image)を操作するために必要

public class MapManager : MonoBehaviour{
    [Header("マップ設定")]
    public Transform playerIcon;
    public MapNode currentNode;
    public float moveSpeed = 500f;

    [Header("道（ライン）の設定")]
    public GameObject linePrefab;    // 道になるUI Imageのプレハブ
    public Transform lineContainer;  // 線をまとめる空オブジェクト
    public Color lockedLineColor = new Color(0.3f, 0.3f, 0.3f); // 暗いグレー
    public Color unlockedLineColor = new Color(0.8f, 0.6f, 0.9f); // 病みかわ感のあるパステルパープル

    private bool isMoving = false;
    private MapNode targetNode;

    void Start(){
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes){
            node.SetupNode();
        }

        // ▼【追加】全てのノードの間に自動で線を引く ▼
        DrawAllPaths(allNodes);

        if (currentNode != null && playerIcon != null){
            playerIcon.position = currentNode.transform.position;
        }
    }

    // --- 【追加】線を引くための処理群 ---

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

        // 【プロのテクニック】A→B と B→A で2回線を引いてしまわないよう、IDで弾く
        if (fromNode.GetInstanceID() > toNode.GetInstanceID()) return;

        // プレハブを生成し、lineContainer の子オブジェクトにする
        GameObject lineObj = Instantiate(linePrefab, lineContainer, false);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        Image lineImage = lineObj.GetComponent<Image>();

        RectTransform fromRect = fromNode.GetComponent<RectTransform>();
        RectTransform toRect = toNode.GetComponent<RectTransform>();

        // 【超重要】画像のピボット（中心点）を「左端」に強制設定し、伸縮しやすくする
        lineRect.pivot = new Vector2(0, 0.5f);

        // 線の開始位置を fromNode に合わせる
        lineRect.anchoredPosition = fromRect.anchoredPosition;

        // 2点間の距離（長さ）と角度を計算
        Vector2 dir = toRect.anchoredPosition - fromRect.anchoredPosition;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 計算した長さと角度を適用（太さは 15px に設定）
        lineRect.sizeDelta = new Vector2(distance, 15f);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        // 両方のノードが解放されていれば明るい色、そうでなければ暗い色にする
        if (fromNode.IsUnlocked && toNode.IsUnlocked){
            lineImage.color = unlockedLineColor;
        }else{
            lineImage.color = lockedLineColor;
        }

        // 線がノードやプレイヤーの上の重ならないよう、一番奥（一番上）に移動させる
        lineRect.SetAsFirstSibling();
    }

    // -----------------------------------

    void Update(){
        if (isMoving){
            MovePlayerIcon();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        MapNode nextNode = null;

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) nextNode = currentNode.upNode;
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) nextNode = currentNode.downNode;
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) nextNode = currentNode.leftNode;
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) nextNode = currentNode.rightNode;

        if (nextNode != null && nextNode.IsUnlocked){
            targetNode = nextNode;
            isMoving = true;
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            if (currentNode != null && currentNode.myLevelData != null && currentNode.IsUnlocked){
                SceneManager.LoadScene(currentNode.myLevelData.sceneName);
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
        }
    }
}