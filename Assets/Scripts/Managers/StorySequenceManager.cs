/* ===================================================
 * スクリプト名 : StorySequenceManager.cs
 * 用途 : 寸劇と会話テキストを連動させるストーリー進行管理
 * 拡張 : 一枚絵（スチル）の切り替え機能をサポート
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ▼ Imageを使うために追加
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StoryPage {
    [Header("会話テキスト")]
    public string speakerName;
    [TextArea(2, 4)]
    public string message;

    // ▼▼▼ 新規追加：一枚絵の連動 ▼▼▼
    [Header("一枚絵連動（任意）")]
    [Tooltip("このページで表示したい一枚絵（空欄なら前の絵を維持）")]
    public Sprite cgSprite;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("アニメーション連動（任意）")]
    public Animator targetAnimator;
    public string animationTrigger;

    [Header("移動連動（任意）")]
    public Transform targetToMove;
    public Transform moveDestination;
    public float moveDuration = 1.0f;
}

public class StorySequenceManager : MonoBehaviour {
    [Header("ストーリーデータ")]
    public List<StoryPage> pages = new List<StoryPage>();

    [Header("UI参照")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;

    // ▼▼▼ 新規追加：一枚絵を表示する枠 ▼▼▼
    [Header("一枚絵表示用のUI")]
    [Tooltip("Canvas内に作った StoryImage をアサインしてください")]
    public Image storyImageUI;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("テキスト表示設定")]
    public float typingSpeed = 0.05f;

    [Header("遷移先設定")]
    public string nextSceneName = "MapSelectScene_Level1";
    public int worldNumberToMarkWatched = 1;
    public TransitionType transitionType = TransitionType.Fade;

    private int currentPageIndex = 0;
    private bool isFinished = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start(){
        if (pages.Count > 0){
            PlayPage(currentPageIndex);
        }
    }

    void Update(){
        if (isFinished) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            if (isTyping){
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                messageText.text = pages[currentPageIndex].message;
                isTyping = false;
            }else{
                NextPage();
            }
        }

        if (keyboard.xKey.wasPressedThisFrame){
            EndStory();
        }
    }

    private void PlayPage(int index){
        StoryPage page = pages[index];

        if (speakerNameText != null) speakerNameText.text = page.speakerName;

        // ▼▼▼ 新規追加：一枚絵の切り替え処理 ▼▼▼
        if (storyImageUI != null){
            if (page.cgSprite != null){
                // 新しい画像が設定されていれば、それを表示して透明度を100%にする
                storyImageUI.sprite = page.cgSprite;
                storyImageUI.color = Color.white;
            }else if (storyImageUI.sprite == null){
                // 画像が設定されておらず、元々の画像も無い場合は透明（見えない状態）にしておく
                storyImageUI.color = Color.clear;
            }
            // ※page.cgSprite が空欄で、すでに何かの画像が表示されている場合は、前のページの画像をそのまま引き継ぎます。
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        // アニメーション指示
        if (page.targetAnimator != null && !string.IsNullOrEmpty(page.animationTrigger)){
            page.targetAnimator.SetTrigger(page.animationTrigger);
        }

        // 移動指示
        if (page.targetToMove != null && page.moveDestination != null){
            StartCoroutine(MoveCharacterRoutine(page.targetToMove, page.moveDestination, page.moveDuration));
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(page.message));
    }

    private IEnumerator MoveCharacterRoutine(Transform target, Transform dest, float duration){
        Vector3 startPos = target.position;
        Vector3 endPos = dest.position;
        float time = 0;

        if (duration <= 0f){
            target.position = endPos;
            yield break;
        }

        while (time < duration){
            time += Time.deltaTime;
            target.position = Vector3.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        target.position = endPos;
    }

    private IEnumerator TypeText(string text){
        isTyping = true;
        messageText.text = "";

        foreach (char c in text){
            messageText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void NextPage(){
        currentPageIndex++;
        if (currentPageIndex < pages.Count) PlayPage(currentPageIndex);
        else EndStory();
    }

    public void EndStory(){
        if (isFinished) return;
        isFinished = true;

        if (worldNumberToMarkWatched > 0){
            string saveKey = "StoryWatched_World_" + worldNumberToMarkWatched;
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
        }

        if (SceneTransitionManager.Instance != null && !string.IsNullOrEmpty(nextSceneName)){
            SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionType);
        }else{
            SceneManager.LoadScene(nextSceneName);
        }
    }
}