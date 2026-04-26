/* ===================================================
 * スクリプト名 : MapSelectManager.cs
 * Version : Ver0.01
 * Since : 2026/04/27
 * Update : 2026/04/27
 * 用途 : UIのアイコンを管理し、選んだステージのSceneをロードします。
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectManager : MonoBehaviour{
    // ステージ1のボタンが押されたら呼ばれる
    public void LoadStage01(){
        SceneManager.LoadScene("Stage_01");
    }

    // ステージ2のボタンが押されたら呼ばれる
    public void LoadStage02(){
        // SceneManager.LoadScene("Stage_02");
    }
}