/* ===================================================
 * スクリプト名 : WorldSelectManager.cs
 * 用途 : 大マップ（ワールド選択）のカーソル移動とシーン遷移
 * 拡張 : PlayerControls完全対応
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
    private bool isStartingWorld = false;

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
        WorldNode[] allNodes = FindObjectsByType<WorldNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes){
            node.SetupNode();
        }

        if (GameManager.Instance != null){
            int savedNodeNum = GameManager.Instance.currentWorldNodeNumber;
            foreach (var node in allNodes){
                if (node.myWorldData != null && node.myWorldData.worldNumber == savedNodeNum){
                    currentNode = node;
                    break;
                }
            }
        }

        if (currentNode != null && playerIcon != null){
            playerIcon.position = currentNode.transform.position;
        }
    }

    void Update(){
        if (isStartingWorld) return;

        if (inputCooldown > 0f) {
            inputCooldown -= Time.unscaledDeltaTime;
        }

        if (isMoving){
            MovePlayerIcon();
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

        WorldNode nextNode = null;
        if (isUp) nextNode = currentNode.upNode;
        else if (isDown) nextNode = currentNode.downNode;
        else if (isLeft) nextNode = currentNode.leftNode;
        else if (isRight) nextNode = currentNode.rightNode;

        if (nextNode != null && nextNode.IsUnlocked){
            targetNode = nextNode;
            isMoving = true;
            inputCooldown = 0.2f;
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();

        if (isSubmit){
            if (currentNode != null && currentNode.myWorldData != null && currentNode.IsUnlocked){
                isStartingWorld = true;

                WorldData targetWorld = currentNode.myWorldData;
                string storyFlag = "StoryWatched_World_" + targetWorld.worldNumber;
                bool isStoryWatched = false;

                if (GameManager.Instance != null){
                    isStoryWatched = GameManager.Instance.HasEventFlag(storyFlag);
                }else{
                    isStoryWatched = PlayerPrefs.GetInt(storyFlag, 0) == 1;
                }

                string sceneToLoad = targetWorld.sceneName; 

                if (!isStoryWatched && !string.IsNullOrEmpty(targetWorld.storySceneName)){
                    sceneToLoad = targetWorld.storySceneName;
                }

                if (SceneTransitionManager.Instance != null){
                    SceneTransitionManager.Instance.LoadScene(sceneToLoad, TransitionType.Fade);
                }else{
                    SceneManager.LoadScene(sceneToLoad);
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

            if (GameManager.Instance != null && currentNode.myWorldData != null){
                GameManager.Instance.currentWorldNodeNumber = currentNode.myWorldData.worldNumber;
                GameManager.Instance.SaveGame();
            }
        }
    }
}