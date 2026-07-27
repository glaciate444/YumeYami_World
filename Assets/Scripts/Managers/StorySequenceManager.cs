using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StorySequenceManager : MonoBehaviour {
    [Header("遷移先設定")]
    [Tooltip("このストーリーが終わった後に向かうシーン名（マップ画面など）")]
    public string nextSceneName = "MapSelectScene_Level1";

    // ▼▼▼ ここを新規追加 ▼▼▼
    [Header("既読管理")]
    [Tooltip("見終わったことにするワールド番号（例：1。0なら何もしない）")]
    public int worldNumberToMarkWatched = 1;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    public TransitionType transitionType = TransitionType.Fade;
    private bool isFinished = false;

    void Update(){
        if (isFinished) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Xキーでスキップ・終了
        if (keyboard.xKey.wasPressedThisFrame){
            EndStory();
        }
    }

    public void EndStory(){
        if (isFinished) return;
        isFinished = true;

        // ▼▼▼ ここを新規追加：既読フラグの保存 ▼▼▼
        if (worldNumberToMarkWatched > 0){
            // "StoryWatched_World_1" のようなキーで 1 (既読) を保存
            string saveKey = "StoryWatched_World_" + worldNumberToMarkWatched;
            PlayerPrefs.SetInt(saveKey, 1);
            PlayerPrefs.Save();
            Debug.Log($"ワールド {worldNumberToMarkWatched} のストーリーを既読にしました！");
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        if (SceneTransitionManager.Instance != null && !string.IsNullOrEmpty(nextSceneName)){
            SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionType);
        }else{
            SceneManager.LoadScene(nextSceneName);
        }
    }
}