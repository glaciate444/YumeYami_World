/* ===================================================
 * スクリプト名 : GameManager.cs
 * Version : Ver0.01
 * Since : 2026/04/27
 * Update : 2026/04/27
 * 用途 : シーンを切り替えても絶対に消滅しない、ゲームの総司令塔
 * =================================================== */
using UnityEngine;

public class GameManager : MonoBehaviour{
    // どこからでも GameManager.Instance でアクセスできるようにする（シングルトン）
    public static GameManager Instance;

    [Header("プレイヤーのデータ")]
    public int currentMaxHp = 10;
    public int currentMaxSp = 6;
    public int unlockedStageLevel = 1; // どこまでクリアしたか

    void Awake(){
        // 自分が最初の1つ目なら、シーンを跨いでも消えないようにする
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // すでにGameManagerが存在しているなら、自分（重複分）を削除する
        else{
            Destroy(gameObject);
        }
    }
}