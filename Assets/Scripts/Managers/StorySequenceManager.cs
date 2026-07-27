using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StorySequenceManager : MonoBehaviour {
    [Header("遷移先設定")]
    [Tooltip("このストーリーが終わった後に向かうシーン名")]
    public string nextSceneName = "WorldMapScene";

    [Tooltip("画面遷移の演出タイプ")]
    public TransitionType transitionType = TransitionType.Fade;

    private bool isFinished = false;

    void Update(){
        if (isFinished) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼ 進行処理（今後の拡張用）
        // ZキーやEnterキーでテキストを読み進める処理をここに書きます。

        // ▼【テスト用】スキップ・終了判定
        // テキストがすべて終わった時、またはXキーでスキップした時に次のシーンへ行く
        if (keyboard.xKey.wasPressedThisFrame){
            EndStory();
        }
    }

    /// <summary>
    /// ストーリーを終了し、次のシーンへ遷移する
    /// （テキストの最後のページを読み終わった時などに呼び出します）
    /// </summary>
    public void EndStory(){
        if (isFinished) return;
        isFinished = true;

        // SceneTransitionManagerを使ってスタイリッシュに遷移する
        if (SceneTransitionManager.Instance != null && !string.IsNullOrEmpty(nextSceneName)){
            SceneTransitionManager.Instance.LoadScene(nextSceneName, transitionType);
        }else{
            SceneManager.LoadScene(nextSceneName);
        }
    }
}