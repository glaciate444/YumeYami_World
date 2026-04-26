/* ===================================================
 * スクリプト名 : TitleManager.cs
 * Version : Ver0.01
 * Since : 2026/04/27
 * Update : 2026/04/27
 * 用途 : タイトル画面の演出や、ボタンを押した時の画面遷移だけを担当する使い捨てのスクリプトです。
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement; // シーン移動に必須

public class TitleManager : MonoBehaviour{
    // UIのボタン（Start Button）の OnClick() にこのメソッドを紐付ける
    public void OnClickStart(){
        // 効果音を鳴らす処理などをここに入れる

        // MapSelectScene をロードする
        SceneManager.LoadScene("MapSelectScene");
    }
}