/* ===================================================
 * スクリプト名 : StageManager.cs
 * 用途 : アクションステージの初期化・BGM再生
 * =================================================== */
using UnityEngine;

public class StageManager : MonoBehaviour{
    [Header("このステージのデータ")]
    [Tooltip("このシーンの LevelData をセットしてください")]
    public LevelData myLevelData;

    void Start(){
        // ▼ シーン開始と同時にBGMを再生する
        PlayStageBGM();
    }

    private void PlayStageBGM(){
        if (myLevelData != null && myLevelData.stageBGM != null){
            if (SoundManager.instance != null){
                // ※SoundManagerにBGM再生用のメソッド（PlayBGMなど）がある前提のコードです
                SoundManager.instance.PlayBGM(myLevelData.stageBGM);
            }else
            {
                Debug.LogWarning("SoundManagerが見つかりません。BGMが再生できませんでした。");
            }
        }
    }
}