/* ===================================================
 * スクリプト名 : MapManager.cs
 * Version : Ver0.05
 * Since : 2026/04/28
 * Update : 2026/07/02
 * 用途 : MapManager (マップ管理者): プレイヤーの移動を制御し、
 * 今どのノードにいるのか、次はどこへ移動できるのかを管理します。
 * 拡張 : 決定ボタンの連打バグを防ぐフラグを追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // ▼【新規追加】TextMeshProを扱うために必要

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

    // ▼【新規追加】シーン遷移が始まったら true にして入力の連打を防ぐ
    private bool isStartingCourse = false; 

    [Header("ワールドマップ（大マップ）へ戻る設定")]
    public string worldMapSceneName = "WorldMapScene";

    [Header("UI表示設定")]
    public TextMeshProUGUI stageNameText; // 「1-1: AAAAA」などのステージ名表示用
    public TextMeshProUGUI livesText;     // 残機表示用
    public Image[] medalImages;           // メダルのアイコン画像（インスペクターで3つセットする）

    [Tooltip("取得済みのメダルの色（デフォルトは白）")]
    public Color gotMedalColor = Color.white;
    [Tooltip("未取得のメダルの色（デフォルトは半透明の黒など）")]
    public Color notGotMedalColor = new Color(0, 0, 0, 0.5f);

    void Start(){
        MapNode[] allNodes = FindObjectsByType<MapNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes){
            node.SetupNode();
        }

        DrawAllPaths(allNodes);

        if (GameManager.Instance != null) {
            int savedNodeNum = GameManager.Instance.currentMapNodeNumber;
            foreach (var node in allNodes) {
                if (node.myLevelData != null && node.myLevelData.stageNumber == savedNodeNum) {
                    currentNode = node;
                    break;
                }
            }
        }

        if (currentNode != null && playerIcon != null){
            playerIcon.position = currentNode.transform.position;
        }

        // ▼【新規追加】ゲーム開始時にUIを初期状態に更新する
        UpdateMapUI();
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
        // ▼【新規追加】すでに画面遷移が始まっていたら、これ以下の処理（キー入力）を一切無視する！
        if (isStartingCourse) return;

        if (isMoving){
            MovePlayerIcon();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ キャンセルキーでの戻る処理（ここでも連打防止のロックをかける）
        if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) {
            isStartingCourse = true; // ← ここでロック！
            if (SceneTransitionManager.Instance != null) {
                SceneTransitionManager.Instance.LoadScene(worldMapSceneName, TransitionType.Fade);
            } else {
                SceneManager.LoadScene(worldMapSceneName);
            }
            return; 
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

        // ▼ 決定ボタンの処理
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            if (currentNode != null && currentNode.myLevelData != null && currentNode.IsUnlocked){
                
                // ▼【新規追加】コースに入ることが確定したら、フラグをONにして連打をロックする！
                isStartingCourse = true;

                if (GameManager.Instance != null) {
                    GameManager.Instance.returnMapSceneName = SceneManager.GetActiveScene().name;
                }

                SceneTransitionManager.Instance.LoadCourseByNumber(
                    currentNode.myLevelData.sceneName,
                    currentNode.myLevelData.stageNumber // ← 修正：ステージ番号（int）を渡す！
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

            if (GameManager.Instance != null && currentNode.myLevelData != null) {
                GameManager.Instance.currentMapNodeNumber = currentNode.myLevelData.stageNumber;
                GameManager.Instance.SaveGame(); 
            }
        }
        UpdateMapUI();
    }
    // ▼▼▼ ここから新規追加 ▼▼▼
    /// <summary>
    /// 現在のノード情報やGameManagerのデータを元に、UIを最新状態に更新します
    /// </summary>
    private void UpdateMapUI(){
        // 1. 残機の更新
        if (livesText != null && GameManager.Instance != null){
            livesText.text = GameManager.Instance.currentLives.ToString("D2");
        }

        // 2. ステージ名とメダルの更新
        if (currentNode != null && currentNode.myLevelData != null){

            if (stageNameText != null){
                stageNameText.text = currentNode.myLevelData.levelName;
            }

            // ▼▼▼ 修正：LevelDataから最大枚数を取得して表示枠を切り替える ▼▼▼
            int maxMedals = currentNode.myLevelData.maxMedals;

            for (int i = 0; i < medalImages.Length; i++){
                if (medalImages[i] != null){
                    // ループの回数が、そのステージの最大メダル数より少ない場合は表示する
                    if (i < maxMedals){
                        medalImages[i].gameObject.SetActive(true); // 枠を表示

                        string saveKey = $"Stage_{currentNode.myLevelData.stageNumber}_SpecialItem_{i}";
                        bool isGot = PlayerPrefs.GetInt(saveKey, 0) == 1;

                        medalImages[i].color = isGot ? gotMedalColor : notGotMedalColor;


                    } // 最大枚数を超えている枠は非表示にする（例：最大3枚なら、iが3, 4の枠は消す）
                    else{
                        medalImages[i].gameObject.SetActive(false);
                    }
                }
            }
            // ▲▲▲ 修正ここまで ▲▲▲

        }else{
            // ステージデータを持たない「通過点」に止まった場合の処理
            if (stageNameText != null) stageNameText.text = "";

            // ▼ 修正：通過点は枠ごと非表示にする
            for (int i = 0; i < medalImages.Length; i++){
                if (medalImages[i] != null){
                    medalImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}