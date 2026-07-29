/* ===================================================
 * スクリプト名 : StorySequenceManager.cs
 * 用途 : 寸劇と会話テキストを連動させるストーリー進行管理
 * =================================================== */
/* ===================================================
 * スクリプト名 : StorySequenceManager.cs
 * 用途 : 寸劇と会話テキストを連動させるストーリー進行管理
 * 拡張 : キャラクターの座標移動機能をサポート
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    // ▼▼▼ 新規追加：移動システム ▼▼▼
    [Header("移動連動（任意）")]
    [Tooltip("移動させたいキャラクターを指定")]
    public Transform targetToMove;
    [Tooltip("移動先の目標地点（空のGameObject等）を指定")]
    public Transform moveDestination;
    [Tooltip("移動にかかる時間（秒）")]
    public float moveDuration = 1.0f;
    // ▲▲▲ 新規追加ここまで ▲▲▲
}

public class StorySequenceManager : MonoBehaviour {
    [Header("ストーリーデータ")]
    public List<StoryPage> pages = new List<StoryPage>();

    [Header("UI参照")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;

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

        if (page.targetAnimator != null && !string.IsNullOrEmpty(page.animationTrigger)){
            page.targetAnimator.SetTrigger(page.animationTrigger);
        }

        // ▼▼▼ 新規追加：移動指示があればコルーチンを開始 ▼▼▼
        if (page.targetToMove != null && page.moveDestination != null){
            StartCoroutine(MoveCharacterRoutine(page.targetToMove, page.moveDestination, page.moveDuration));
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(page.message));
    }

    // ▼▼▼ 新規追加：滑らかに移動させるコルーチン ▼▼▼
    private IEnumerator MoveCharacterRoutine(Transform target, Transform dest, float duration){
        Vector3 startPos = target.position;
        Vector3 endPos = dest.position;
        float time = 0;

        // 念のため、0秒が指定された場合は即座にワープさせる
        if (duration <= 0f){
            target.position = endPos;
            yield break;
        }

        while (time < duration){
            time += Time.deltaTime;
            // Lerp関数を使って、現在地から目的地まで時間経過に合わせて滑らかに移動させる
            target.position = Vector3.Lerp(startPos, endPos, time / duration);
            yield return null; // 次のフレームまで待つ
        }

        // 最後に数値をピッタリ合わせる
        target.position = endPos;
    }
    // ▲▲▲ 新規追加ここまで ▲▲▲

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