/* ===================================================
 * スクリプト名 : StorySequenceManager.cs
 * 用途 : 寸劇と会話テキストを連動させるストーリー進行管理
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// ▼ 1ページ分のデータを定義するクラス
[System.Serializable]
public class StoryPage {
    [Header("会話テキスト")]
    public string speakerName;
    [TextArea(2, 4)]
    public string message;

    [Header("アニメーション連動（任意）")]
    [Tooltip("動かしたいキャラクターのAnimatorを指定")]
    public Animator targetAnimator;
    [Tooltip("再生したいTrigger名（例: Jump, Walk など）")]
    public string animationTrigger;
}

public class StorySequenceManager : MonoBehaviour {
    [Header("ストーリーデータ")]
    public List<StoryPage> pages = new List<StoryPage>();

    [Header("UI参照")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;

    [Header("テキスト表示設定")]
    public float typingSpeed = 0.05f; // 1文字表示される間隔（秒）

    [Header("遷移先設定")]
    public string nextSceneName = "MapSelectScene_Level1";
    public int worldNumberToMarkWatched = 1;
    public TransitionType transitionType = TransitionType.Fade;

    private int currentPageIndex = 0;
    private bool isFinished = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start(){
        // 最初のページを表示開始
        if (pages.Count > 0){
            PlayPage(currentPageIndex);
        }else{
            Debug.LogWarning("ストーリーのページが設定されていません。");
        }
    }

    void Update(){
        if (isFinished) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 決定ボタン（ZキーやEnter）での進行
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            if (isTyping){
                // 文字送り中の場合は、スキップして全文を即座に表示する
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                messageText.text = pages[currentPageIndex].message;
                isTyping = false;
            }else{
                // 文字表示が完了している場合は、次のページへ
                NextPage();
            }
        }

        // Xキーでストーリー自体をスキップ
        if (keyboard.xKey.wasPressedThisFrame){
            EndStory();
        }
    }

    private void PlayPage(int index){
        StoryPage page = pages[index];

        // 名前テキストの更新（空欄なら名前枠を非表示にするなどの処理も可能）
        if (speakerNameText != null) speakerNameText.text = page.speakerName;

        // アニメーションの再生指示があれば実行
        if (page.targetAnimator != null && !string.IsNullOrEmpty(page.animationTrigger)){
            page.targetAnimator.SetTrigger(page.animationTrigger);
        }

        // タイプライター演出の開始
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(page.message));
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

        if (currentPageIndex < pages.Count){
            // 次のページがあるなら再生
            PlayPage(currentPageIndex);
        }else{
            // 全ページ終了したらシーン遷移
            EndStory();
        }
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