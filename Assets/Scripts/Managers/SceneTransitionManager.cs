/* ===================================================
 * スクリプト名 : SceneTransitionManager.cs
 * Version : Ver0.02
 * Since : 2026/05/15
 * Update : 2026/05/15
 * 用途 : スタイリッシュな画面遷移とシーンロードの管理
 * 更新 : 複数種類のトランジション対応版
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

// ▼【追加】トランジションの種類を定義するリスト
public enum TransitionType {
    Fade,       // 通常の暗転フェード
    CourseSlide // コース選択時のスライド
}

public class SceneTransitionManager : MonoBehaviour {
    public static SceneTransitionManager Instance;

    [Header("UI連携")]
    public Animator anim;
    public TMP_Text courseText;

    [Header("設定")]
    public float fadeWaitTime = 1.0f;  // 暗転にかかる時間
    public float slideWaitTime = 1.2f; // スライドにかかる時間

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =================================================
    // ▼ 呼び出し用メソッド（種類を指定できるように拡張）
    // =================================================

    // ① 文字なし遷移（タイトル→マップなど）。デフォルトを Fade に設定。
    public void LoadScene(string sceneName, TransitionType type = TransitionType.Fade) {
        StartCoroutine(TransitionRoutine(sceneName, "", type));
    }

    // ② コースイン遷移（マップ→ステージなど）。デフォルトを CourseSlide に設定。
    public void LoadCourse(string sceneName, string displayName, TransitionType type = TransitionType.CourseSlide) {
        StartCoroutine(TransitionRoutine(sceneName, displayName, type));
    }

    // =================================================

    private IEnumerator TransitionRoutine(string sceneName, string displayName, TransitionType type) {
        // テキストのON/OFF設定
        if (courseText != null) {
            courseText.text = displayName;
            courseText.gameObject.SetActive(!string.IsNullOrEmpty(displayName)); 
        }

        // ▼【重要】指定されたタイプに合わせて、Animatorに送る合図と待機時間を変える
        string inTrigger = "";
        string outTrigger = "";
        float waitTime = 1.0f;

        switch (type) {
            case TransitionType.Fade:
                inTrigger = "FadeIn";
                outTrigger = "FadeOut";
                waitTime = fadeWaitTime;
                break;
            case TransitionType.CourseSlide:
                inTrigger = "CourseIn";
                outTrigger = "CourseOut";
                waitTime = slideWaitTime;
                break;
        }

        // トランジションIn アニメーション再生
        anim.SetTrigger(inTrigger);
        yield return new WaitForSeconds(waitTime);

        // シーン切り替え
        SceneManager.LoadScene(sceneName);
        yield return null; 

        // トランジションOut アニメーション再生
        anim.SetTrigger(outTrigger);
    }
}