/* ===================================================
 * スクリプト名 : SwitchManager.cs
 * 用途 : ON/OFFスイッチの状態を管理し、全ブロックに一斉通知する
 * =================================================== */
using UnityEngine;
using System; // Action（イベント）を使うために必要

public class SwitchManager : MonoBehaviour {
    public static SwitchManager Instance;

    [Header("初期状態")]
    public bool isRedOn = true; // trueなら赤がON、falseなら青がON

    // ▼【魔法の機能】スイッチが切り替わったことを全ブロックに一斉送信する「イベント」
    public event Action<bool> OnSwitchToggled;

    [Header("効果音")]
    public AudioClip switchSE;

    void Awake() {
        Instance = this;
    }

    public void Toggle() {
        // 状態を反転させる（赤⇔青）
        isRedOn = !isRedOn;

        // 登録されているすべてのブロックとスイッチに「切り替わったよ！」と知らせる
        OnSwitchToggled?.Invoke(isRedOn);

        if (SoundManager.instance != null && switchSE != null) {
            SoundManager.instance.PlaySE(switchSE);
        }
        
        Debug.Log("スイッチ切り替え！ 現在ON: " + (isRedOn ? "赤" : "青"));
    }
}