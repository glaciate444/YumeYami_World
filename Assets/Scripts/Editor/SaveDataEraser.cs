/* ===================================================
 * スクリプト名 : SaveDataEraser.cs
 * 用途 : Unityエディタ上から1クリックでセーブデータを消去するツール
 * 注意 : 必ず「Editor」フォルダの中に入れてください
 * =================================================== */
#if UNITY_EDITOR // エディタ上でのみ動くようにするお約束
using UnityEngine;
using UnityEditor;

public class SaveDataEraser {
    
    // Unityの上部メニューに「Tools」＞「セーブデータを初期化」というボタンを作ります
    [MenuItem("Tools/セーブデータを初期化 (PlayerPrefsクリア)")]
    public static void ResetPlayerPrefs() {
        // PlayerPrefsの全データを消去
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("【完了】すべてのセーブデータを初期化しました！まっさらな状態からテストできます。");
    }
}
#endif