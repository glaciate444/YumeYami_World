/* ===================================================
 * スクリプト名 : StorySequenceManager.cs
 * 用途 : 寸劇と会話テキストを連動させるストーリー進行管理
 * 拡張 : 一枚絵対応 ＆ 移動完了時の自動進行・スキップ制御を追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StoryPage {
    [Header("会話テキスト")]
    public string speakerName;
    [TextArea(2, 4)]
    public string message;

    [Header("一枚絵連動（任意）")]
    [Tooltip("このページで表示したい一枚絵（空欄なら前の絵を維持）")]
    public Sprite cgSprite;

    [Header("アニメーション連動（任意）")]
    public Animator targetAnimator;
    public string animationTrigger;

    [Header("移動連動（任意）")]
    public Transform targetToMove;
    public Transform moveDestination;
    public float moveDuration = 1.0f;

    // ▼▼▼ 新規追加：移動中の制御オプション ▼▼▼
    [Tooltip("ONにすると、この移動が終わるまでEnterキーでのスキップを禁止します")]
    public bool blockInputDuringMove = false;

    [Tooltip("ONにすると、指定位置に到達した瞬間に自動で次のページ（配列）へ進みます")]
    public bool autoProceedAfterMove = false;
    // ▲▲▲ 新規追加ここまで ▲▲▲
}

public class StorySequenceManager : MonoBehaviour {
    [Header("ストーリーデータ")]
    public List<StoryPage> pages = new List<StoryPage>();

    [Header("UI参照")]
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;
    public Image storyImageUI;

    [Header("テキスト表示設定")]
    public float typingSpeed = 0.05f;

    [Header("演出設定")]
    public float cgFadeDuration = 0.5f; // フェードにかかる時間
    private Coroutine cgFadeCoroutine;

    [Header("遷移先設定")]
    public string nextSceneName = "MapSelectScene_Level1";
    public int worldNumberToMarkWatched = 1;
    public TransitionType transitionType = TransitionType.Fade;

    private int currentPageIndex = 0;
    private bool isFinished = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    // ▼ 新規追加：移動状態を管理する変数
    private Coroutine moveCoroutine;
    private bool isMoving = false;
    private Transform currentMovingTarget;
    private Transform currentDestination;

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

            // ▼ 修正1：移動中で、かつスキップ禁止設定なら何もしない
            if (isMoving && pages[currentPageIndex].blockInputDuringMove) return;

            // ▼ 修正2：移動中にスキップされた場合、目的地へ一瞬でワープさせてスライド移動を防ぐ
            if (isMoving && currentMovingTarget != null && currentDestination != null){
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                currentMovingTarget.position = currentDestination.position;
                isMoving = false;
            }

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

        // ▼▼▼ ここにフェード切り替え処理を呼び出す ▼▼▼
        ChangeCGWithFade(page.cgSprite);

        // アニメーション指示
        if (page.targetAnimator != null && !string.IsNullOrEmpty(page.animationTrigger)){
            page.targetAnimator.SetTrigger(page.animationTrigger);
        }

        // ▼ 修正3：移動指示（コルーチンを管理する）
        if (page.targetToMove != null && page.moveDestination != null){
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveCharacterRoutine(page, page.targetToMove, page.moveDestination, page.moveDuration));
        }else{
            isMoving = false; // 移動がないページではフラグを折る
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(page.message));
    }

    private IEnumerator MoveCharacterRoutine(StoryPage page, Transform target, Transform dest, float duration){
        isMoving = true;
        currentMovingTarget = target;
        currentDestination = dest;

        Vector3 startPos = target.position;
        Vector3 endPos = dest.position;
        float time = 0;

        if (duration > 0f){
            while (time < duration){
                time += Time.deltaTime;
                target.position = Vector3.Lerp(startPos, endPos, time / duration);
                yield return null;
            }
        }

        // 目的地にピッタリ合わせる
        target.position = endPos;
        isMoving = false;
        moveCoroutine = null;

        // ▼ 修正4：自動進行オプションがONなら、勝手に次のページへ進む
        if (page.autoProceedAfterMove){
            // テキストがまだタイピング途中なら強制完了させる
            if (isTyping){
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                messageText.text = page.message;
                isTyping = false;
            }
            NextPage();
        }
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

    /// <summary>
    /// 一枚絵（CG）をフェードアウト・インで切り替える
    /// </summary>
    private void ChangeCGWithFade(Sprite nextSprite)
    {
        if (storyImageUI == null) return;

        // すでにフェード処理中なら一旦止める
        if (cgFadeCoroutine != null)
        {
            StopCoroutine(cgFadeCoroutine);
        }

        cgFadeCoroutine = StartCoroutine(FadeCGRoutine(nextSprite));
    }

    private IEnumerator FadeCGRoutine(Sprite nextSprite)
    {
        // 1. 次の画像が空（None）の場合：現在の画像をフェードアウトして消す
        if (nextSprite == null)
        {
            yield return StartCoroutine(FadeAlpha(0f, cgFadeDuration));
            storyImageUI.sprite = null;
        }
        // 2. 現在の画像と同じ画像が設定されている場合：何もしない（チラつき防止）
        else if (storyImageUI.sprite == nextSprite)
        {
            yield return StartCoroutine(FadeAlpha(1f, cgFadeDuration));
        }
        // 3. 違う画像に切り替わる場合：一度フェードアウトしてから、新しい画像をセットしてフェードイン
        else
        {
            // すでに絵が表示されているなら、一度透明にする
            if (storyImageUI.sprite != null && storyImageUI.color.a > 0)
            {
                yield return StartCoroutine(FadeAlpha(0f, cgFadeDuration / 2f));
            }

            // 新しい画像をセットして、不透明（1f）に向かってフェードイン
            storyImageUI.sprite = nextSprite;
            yield return StartCoroutine(FadeAlpha(1f, cgFadeDuration));
        }
    }

    private IEnumerator FadeAlpha(float targetAlpha, float duration)
    {
        float startAlpha = storyImageUI.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            storyImageUI.color = new Color(storyImageUI.color.r, storyImageUI.color.g, storyImageUI.color.b, newAlpha);
            yield return null;
        }

        storyImageUI.color = new Color(storyImageUI.color.r, storyImageUI.color.g, storyImageUI.color.b, targetAlpha);
    }


    public void EndStory(){
        if (isFinished) return;
        isFinished = true;

        if (worldNumberToMarkWatched > 0){
            string storyFlag = "StoryWatched_World_" + worldNumberToMarkWatched;

            if (GameManager.Instance != null){
                // セーブデータ（GameManager）にストーリー既読フラグを刻み込む
                GameManager.Instance.AddEventFlag(storyFlag);
            }else{
                // テスト用の旧処理（GameManager不在時）
                PlayerPrefs.SetInt(storyFlag, 1);
                PlayerPrefs.Save();
            }
        }

        if (SceneTransitionManager.Instance != null && !string.IsNullOrEmpty(nextSceneName)){
            SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionType);
        }else{
            SceneManager.LoadScene(nextSceneName);
        }
    }
}