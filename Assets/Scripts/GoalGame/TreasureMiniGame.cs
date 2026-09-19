/* ===================================================
 * スクリプト名 : TreasureMiniGame.cs
 * 用途 : 8つの宝箱から2つを選んで開けるミニゲーム
 * 拡張 : PlayerControls完全対応（コントローラー＆キーボード両対応版）
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class TreasureMiniGame : MonoBehaviour{
    [System.Serializable]
    public class RewardData{
        public bool isCoin;
        public int amount;
    }

    [Header("宝箱の設定")]
    public Button[] chestButtons;
    public Sprite openChestSprite;
    public List<RewardData> rewards;

    [Header("UIナビゲーション設定")]
    public RectTransform cursorImage;
    public RectTransform[] menuPositions;
    public Vector2 cursorOffset = new Vector2(0f, 0f);

    [Header("サウンド設定")]
    public AudioClip miniGameBGM;

    [Header("HUD連携")]
    public TextMeshProUGUI hudCoinText;
    public TextMeshProUGUI hudHeartText;
    public TextMeshProUGUI hudLivesText;
    public TextMeshProUGUI remainingOpensText;

    [Header("ポップアップ演出")]
    public GameObject rewardPopupPrefab;
    public Sprite coinIconSprite;
    public Sprite heartIconSprite;

    private int openedCount = 0;
    private const int MAX_OPENS = 2;
    private bool isGameOver = false;

    private int currentIndex = 0;

    // ▼ 新規追加：Input Actionのクラスとクールダウン
    private PlayerControls input;
    private float inputCooldown = 0f;

    private readonly int[,] navigation = new int[9, 4] {
        { 0, 4, 0, 1 }, { 1, 5, 0, 2 }, { 2, 6, 1, 3 }, { 3, 7, 2, 3 },
        { 0, 8, 4, 5 }, { 1, 8, 4, 6 }, { 2, 8, 5, 7 }, { 3, 8, 6, 7 },
        { 5, 8, 8, 8 }
    };

    void Awake(){
        input = new PlayerControls();
    }

    void OnEnable(){
        input.Enable();
    }

    void OnDisable(){
        input.Disable();
    }

    void Start(){
        if (SoundManager.instance != null && miniGameBGM != null){
            SoundManager.instance.PlayBGM(miniGameBGM);
        }

        if (rewards.Count < chestButtons.Length){
            Debug.LogError($"【設定エラー】Rewardsの数が足りません！");
            return;
        }
        if (menuPositions.Length < 9){
            Debug.LogError($"【設定エラー】Menu Positions が足りません！9個設定してください。");
            return;
        }

        ShuffleRewards();

        for (int i = 0; i < chestButtons.Length; i++){
            if (chestButtons[i] == null) continue;
            int index = i;
            chestButtons[i].onClick.AddListener(() => OnClickChest(index));
        }

        UpdateCursorPosition();
        UpdateHUD();
    }

    private void ShuffleRewards(){
        for (int i = 0; i < rewards.Count; i++){
            RewardData temp = rewards[i];
            int randomIndex = Random.Range(i, rewards.Count);
            rewards[i] = rewards[randomIndex];
            rewards[randomIndex] = temp;
        }
    }

    void Update(){
        if (isGameOver) return;

        if (inputCooldown > 0f){
            inputCooldown -= Time.unscaledDeltaTime;
        }

        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        bool isUp = false, isDown = false, isLeft = false, isRight = false;

        if (inputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f){
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)){
                if (moveDir.x > 0.5f) isRight = true;
                else if (moveDir.x < -0.5f) isLeft = true;
            }else{
                if (moveDir.y > 0.5f) isUp = true;
                else if (moveDir.y < -0.5f) isDown = true;
            }
        }

        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();

        bool moved = false;
        if (isUp){
            currentIndex = navigation[currentIndex, 0];
            moved = true;
        }
        else if (isDown){
            currentIndex = navigation[currentIndex, 1];
            moved = true;
        }
        else if (isLeft){
            currentIndex = navigation[currentIndex, 2];
            moved = true;
        }
        else if (isRight){
            currentIndex = navigation[currentIndex, 3];
            moved = true;
        }

        if (moved){
            UpdateCursorPosition();
            inputCooldown = 0.2f;
        }

        if (isSubmit){
            ExecuteMenu();
        }
    }

    private void UpdateCursorPosition(){
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null){
            cursorImage.position = menuPositions[currentIndex].position;
            cursorImage.anchoredPosition += cursorOffset;
        }
    }

    private void ExecuteMenu(){
        if (currentIndex == 8){
            OnClickSkip();
        }else{
            if (chestButtons[currentIndex].interactable){
                OnClickChest(currentIndex);
            }
        }
    }

    public void OnClickChest(int chestIndex){
        if (openedCount >= MAX_OPENS || isGameOver) return;

        openedCount++;
        Button clickedChest = chestButtons[chestIndex];

        if (openChestSprite != null){
            clickedChest.image.sprite = openChestSprite;
        }
        clickedChest.interactable = false;

        ApplyReward(rewards[chestIndex], clickedChest);

        if (openedCount >= MAX_OPENS){
            isGameOver = true;
            StartCoroutine(WaitAndGoToResult());
        }
    }

    private void ApplyReward(RewardData reward, Button chestBtn){
        ShowPopup(chestBtn, reward);

        if (GameManager.Instance != null){
            if (reward.isCoin){
                GameManager.Instance.stageCoins += reward.amount;
                if (GameManager.Instance.stageCoins < 0) GameManager.Instance.stageCoins = 0;
            }else{
                GameManager.Instance.AddLifePiece(reward.amount);
            }
        }
        UpdateHUD();
    }

    private void UpdateHUD(){
        if (GameManager.Instance != null){
            if (hudCoinText != null) hudCoinText.text = GameManager.Instance.stageCoins.ToString("D3");
            if (hudHeartText != null) hudHeartText.text = GameManager.Instance.currentLifePieces.ToString("D2");
            if (hudLivesText != null) hudLivesText.text = GameManager.Instance.currentLives.ToString("D2");
        }
        if (remainingOpensText != null){
            remainingOpensText.text = (MAX_OPENS - openedCount).ToString();
        }
    }

    private void ShowPopup(Button chest, RewardData reward){
        if (rewardPopupPrefab == null) return;
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        GameObject popupObj = Instantiate(rewardPopupPrefab, parentCanvas.transform);
        popupObj.transform.position = chest.transform.position;
        popupObj.transform.SetAsLastSibling();

        RewardPopup popup = popupObj.GetComponent<RewardPopup>();
        if (popup != null){
            Sprite icon = reward.isCoin ? coinIconSprite : heartIconSprite;
            popup.Setup(icon, reward.amount);
        }
    }

    public void OnClickSkip(){
        if (isGameOver) return;
        isGameOver = true;
        GoToResult();
    }

    private IEnumerator WaitAndGoToResult(){
        yield return new WaitForSeconds(2.0f);
        GoToResult();
    }

    private void GoToResult(){
        if (SceneTransitionManager.Instance != null){
            SceneTransitionManager.Instance.LoadScene(SceneNames.Result, TransitionType.Fade);
        }else{
            SceneManager.LoadScene(SceneNames.Result);
        }
    }
}