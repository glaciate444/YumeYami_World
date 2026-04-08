/* ===================================================
 * スクリプト名 : スイッチスクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : 使い方
 * Unityエディタでの設定（超便利！）
 * スイッチのインスペクターを見ると、On Activate () というリストが表示されます。
 * + ボタンを押します。
 * 邪魔をしている「扉（Door）」のオブジェクトを、ヒエラルキーからその枠にドラッグ＆ドロップします。
 * 右側の No Function と書かれたプルダウンを開き、GameObject ＞ SetActive(bool) を選びます。
 * その下のチェックボックスは**空（False）**のままにしておきます。
 * =================================================== */
using UnityEngine;
using UnityEngine.Events; // UnityEventを使うために必要

public class Switch : MonoBehaviour{
    [Header("スイッチを踏んだ時に実行する処理")]
    public UnityEvent onActivate;

    private bool isPressed = false;

    private void OnTriggerEnter2D(Collider2D other){
        // まだ押されておらず、プレイヤーが触れたら
        if (!isPressed && other.CompareTag("Player")){
            isPressed = true;
            Debug.Log("スイッチON！");

            // スイッチを少し凹ませるなどの視覚効果（スケールをY方向に半分にする等）
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.5f, 1f);

            // インスペクターで設定したイベントを実行！
            onActivate.Invoke();
        }
    }
}