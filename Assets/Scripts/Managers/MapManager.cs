/* ===================================================
 * スクリプト名 : MapManager.cs
 * 用途 : プレイヤーのマップ移動制御
 * 拡張 : PlayerControls完全対応
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

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
    private bool isStartingCourse = false; 

    [Header("ワールドマップへ戻る設定")]
    public string worldMapSceneName = "WorldMapScene";

    [Header("UI表示設定")]
    public TextMeshProUGUI stageNameText; 
    public TextMeshProUGUI livesText;     
    public Image[] medalImages;           
    public Color gotMedalColor = Color.white;
    public Color notGotMedalColor = new Color(0, 0, 0, 0.5f);

    [Header("データ")]
    public WorldData myWorldData;

    // ▼ 新規追加
    private PlayerControls input;
    private float inputCooldown = 0f;

    void Awake() {
        input = new PlayerControls();
    }

    void OnEnable() {
        input.Enable();
    }

    void OnDisable() {
        input.Disable();
    }

    void Start(){
        PlayStageBGM();

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
        lineImage.color = (fromNode.IsUnlocked && toNode.IsUnlocked) ? unlockedLineColor : lockedLineColor;
        lineRect.SetAsFirstSibling();
    }

    void Update(){
        if (isStartingCourse) return;

        if (inputCooldown > 0f) {
            inputCooldown -= Time.unscaledDeltaTime;
        }

        if (isMoving){
            MovePlayerIcon();
            return;
        }

        bool isCancel = input.Player.Dash.WasPressedThisFrame() || input.Player.Pause.WasPressedThisFrame();
        
        if (isCancel) {
            isStartingCourse = true; 
            if (SceneTransitionManager.Instance != null) {
                SceneTransitionManager.Instance.LoadScene(worldMapSceneName, TransitionType.Fade);
            } else {
                SceneManager.LoadScene(worldMapSceneName);
            }
            return; 
        }

        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        bool isUp = false, isDown = false, isLeft = false, isRight = false;

        if (inputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f) {
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
                if (moveDir.x > 0.5f) isRight = true;
                else if (moveDir.x < -0.5f) isLeft = true;
            } else {
                if (moveDir.y > 0.5f) isUp = true;
                else if (moveDir.y < -0.5f) isDown = true;
            }
        }

        MapNode nextNode = null;
        if (isUp) nextNode = currentNode.upNode;
        else if (isDown) nextNode = currentNode.downNode;
        else if (isLeft) nextNode = currentNode.leftNode;
        else if (isRight) nextNode = currentNode.rightNode;

        if (nextNode != null && nextNode.IsUnlocked){
            targetNode = nextNode;
            isMoving = true;
            inputCooldown = 0.2f; // 行き過ぎ防止
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();

        if (isSubmit){
            if (currentNode != null && currentNode.IsUnlocked){
                if (currentNode.isShopNode){
                    isStartingCourse = true;
                    if (GameManager.Instance != null) GameManager.Instance.returnMapSceneName = SceneManager.GetActiveScene().name;
                    
                    if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene(currentNode.shopSceneName, TransitionType.Fade);
                    else SceneManager.LoadScene(currentNode.shopSceneName);
                }
                else if (currentNode.myLevelData != null){
                    isStartingCourse = true;
                    if (GameManager.Instance != null) GameManager.Instance.returnMapSceneName = SceneManager.GetActiveScene().name;
                    
                    SceneTransitionManager.Instance.LoadCourseByNumber(
                        currentNode.myLevelData.sceneName,
                        currentNode.myLevelData.displayCourseNumber 
                    );
                }
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
            UpdateMapUI();
        }
    }

    private void UpdateMapUI(){
        if (livesText != null && GameManager.Instance != null){
            livesText.text = GameManager.Instance.currentLives.ToString("D2");
        }

        if (currentNode != null && currentNode.isShopNode){
            if (stageNameText != null) stageNameText.text = "ショップ";
            for (int i = 0; i < medalImages.Length; i++){
                if (medalImages[i] != null) medalImages[i].gameObject.SetActive(false);
            }
        }else if (currentNode != null && currentNode.myLevelData != null){
            if (stageNameText != null) stageNameText.text = currentNode.myLevelData.levelName;

            int maxMedals = currentNode.myLevelData.maxMedals;
            int slot = (GameManager.Instance != null) ? GameManager.Instance.currentSaveSlot : 1;

            for (int i = 0; i < medalImages.Length; i++){
                if (medalImages[i] != null){
                    if (i < maxMedals){
                        medalImages[i].gameObject.SetActive(true);
                        string saveKey = $"Stage_{currentNode.myLevelData.stageNumber}_SpecialItem_{i}_{slot}";
                        bool isGot = PlayerPrefs.GetInt(saveKey, 0) == 1;
                        medalImages[i].color = isGot ? gotMedalColor : notGotMedalColor;
                    }else{
                        medalImages[i].gameObject.SetActive(false);
                    }
                }
            }
        }else{
            if (stageNameText != null) stageNameText.text = "";
            for (int i = 0; i < medalImages.Length; i++){
                if (medalImages[i] != null) medalImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void PlayStageBGM(){
        if (myWorldData != null && myWorldData.worldBGM != null && SoundManager.instance != null){
            SoundManager.instance.PlayBGM(myWorldData.worldBGM);
        }
    }
}