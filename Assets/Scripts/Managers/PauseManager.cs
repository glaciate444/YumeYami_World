using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour{
    // どこからでもアクセスできるようにするためのシングルトン
    public static PauseManager Instance;

    [Header("UI参照")]
    [Tooltip("スクリーンショットにある Menu_Panel をアサインしてください")]
    public GameObject menuPanel;

    private bool isPaused = false;

    void Awake(){
        // シングルトンの設定（テストプレイ時にも各シーンで独立して動くようにする）
        if (Instance == null){
            Instance = this;
        }else{
            Destroy(gameObject);
        }

        // 開始時は必ずポーズメニューを非表示にする
        if (menuPanel != null){
            menuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ポーズ状態のON/OFFを切り替えるメソッド
    /// PlayerController等から呼ばれます
    /// </summary>
    public void TogglePause(){
        isPaused = !isPaused;

        // UIの表示切り替え
        if (menuPanel != null){
            menuPanel.SetActive(isPaused);
        }

        // 時間の停止・再開
        // timeScaleを0にすることで、FixedUpdateやアニメーションが停止します
        Time.timeScale = isPaused ? 0f : 1f;
    }

    // （保険）シーン破棄時に時間が止まったままになるのを防ぐ
    private void OnDestroy(){
        Time.timeScale = 1f;
    }
}