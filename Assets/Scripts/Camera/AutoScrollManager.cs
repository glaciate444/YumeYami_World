/* ===================================================
 * スクリプト名 : AutoScrollManager.cs
 * 用途 : 強制スクロール制御
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;

public class AutoScrollManager : MonoBehaviour {
    [Header("参照")]
    public Transform player;

    [Header("設定")]
    public float scrollSpeed = 2.0f;
    [Tooltip("チェックを入れるとシーン開始時から強制スクロールになります")]
    public bool startOnAwake = false;

    [Header("現在の状態（デバッグ確認用）")]
    public bool isScrolling = false;

    private void Start(){
        isScrolling = startOnAwake;

        // 開始時にプレイヤーの位置に合わせる
        if (player != null && !isScrolling){
            transform.position = player.position;
        }
    }

    // カメラのガタつきを防止するため LateUpdate で処理
    private void LateUpdate(){
        if (isScrolling){
            // 強制スクロール時は自身を右へ移動（必要に応じて方向は変更してください）
            transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
        }else{
            // 普段はプレイヤーの座標に完全に同期
            if (player != null){
                transform.position = player.position;
            }
        }
    }

    // 外部のトリガーから状態を変更するためのメソッド
    public void SetScrollState(bool state){
        isScrolling = state;
    }
}