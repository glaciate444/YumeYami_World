/* ===================================================
 * スクリプト名 : OptionManager.cs
 * Version : Ver0.01
 * Since : 2026/04/27
 * Update : 2026/04/27
 * 用途 : オプション画面マネージャー
 * =================================================== */
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 【追加】キーボード操作用

public class OptionManager : MonoBehaviour{
    [Header("オーディオ設定")]
    public AudioMixer mainMixer;
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("UI操作設定")]
    public GameObject optionPanel;        // オプション画面が開いているか判定するため
    public RectTransform cursorImage;     // オプション画面用のカーソル
    public RectTransform[] menuPositions; // 0:BGMラベル, 1:SEラベル, 2:BACK
    public TitleManager titleManager;     // BACKを押した時に閉じる処理を呼ぶため

    private int currentIndex = 0;
    private float slideSpeed = 1.5f;      // 左右キーを押した時にスライダーが動く速度

    void Start(){
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        if (seSlider != null) seSlider.onValueChanged.AddListener(SetSEVolume);

        if (bgmSlider != null) bgmSlider.value = 0.5f;
        if (seSlider != null) seSlider.value = 0.5f;
    }

    void Update(){
        // オプション画面が非表示の時は、裏でキーボード操作が暴発しないようにする
        if (optionPanel == null || !optionPanel.activeSelf) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ 上下移動（操作項目の切り替え） ▼
        if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            currentIndex++;
            if (currentIndex >= menuPositions.Length) currentIndex = 0;
            UpdateCursorPosition();
        }else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuPositions.Length - 1;
            UpdateCursorPosition();
        }

        // ▼ 左右移動（スライダーの調整） ▼
        // ※ wasPressedThisFrame(1回押し) ではなく isPressed(押しっぱなし) にして滑らかに動かします
        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed){
            AdjustSlider(-slideSpeed * Time.deltaTime);
        }else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed){
            AdjustSlider(slideSpeed * Time.deltaTime);
        }

        // ▼ 決定ボタン（BACK選択時のみ発動） ▼
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame){
            if (currentIndex == 2){ // 2番目(BACK)が選ばれている時
                // スクリプトから強制的に TitleManager の CloseOptions() を呼ぶ
                if (titleManager != null) titleManager.CloseOptions();

                // 次回開いた時に一番上のBGMに戻るようにリセットしておく
                currentIndex = 0;
                UpdateCursorPosition();
            }
        }
    }

    // スライダーの数値を増減させる（UnityのSliderは自動的にMin/Max内に収まるので安心です）
    private void AdjustSlider(float amount){
        if (currentIndex == 0 && bgmSlider != null){
            bgmSlider.value += amount;
        }else if (currentIndex == 1 && seSlider != null){
            seSlider.value += amount;
        }
    }

    // カーソルのY座標を合わせる
    private void UpdateCursorPosition(){
        if (menuPositions.Length > 0 && cursorImage != null && menuPositions[currentIndex] != null){
            Vector2 newPos = cursorImage.anchoredPosition;
            newPos.y = menuPositions[currentIndex].anchoredPosition.y;
            cursorImage.anchoredPosition = newPos;
        }
    }

    public void SetBGMVolume(float value){
        float volume = Mathf.Log10(value) * 20f;
        mainMixer.SetFloat("BGM_Vol", volume);
    }

    public void SetSEVolume(float value){
        float volume = Mathf.Log10(value) * 20f;
        mainMixer.SetFloat("SE_Vol", volume);
    }
}