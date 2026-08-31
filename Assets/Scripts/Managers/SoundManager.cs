/* ===================================================
 * スクリプト名 : SoundManager.cs
 * Version : Ver0.02
 * 用途 : 全てのSEとBGMを一括管理・再生する
 * 拡張 : BGM再生機能（ループ対応、重複再生防止）の追加
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour{
    public static SoundManager instance;

    private AudioSource seSource;
    private AudioSource bgmSource; // ▼ 新規追加：BGM専用スピーカー

    void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 1. SE用の設定（元からあるコンポーネントを使用）
            seSource = GetComponent<AudioSource>();
            seSource.playOnAwake = false;
            seSource.loop = false;

            // 2. BGM用の設定（スクリプトから自動で追加する）
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true; // BGMなのでループ再生をONにする
        }else{
            Destroy(gameObject);
        }
    }

    public void PlaySE(AudioClip clip){
        if (clip != null){
            seSource.PlayOneShot(clip);
        }
    }

    // 通常のBGM再生メソッド
    public void PlayBGM(AudioClip clip){
        if (clip == null) return;

        // 同じ曲が「ループあり」で流れている場合のみスキップ
        if (bgmSource.clip == clip && bgmSource.isPlaying && bgmSource.loop == true) return;

        bgmSource.loop = true; // ★通常のBGMなので確実にループをONにする
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // ジングル（1回だけ鳴らす音楽）用のメソッド
    public void PlayJingle(AudioClip clip){
        if (clip == null) return;

        bgmSource.loop = false; // ★ジングルなのでループをOFFにする
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // BGMを止めるメソッド
    public void StopBGM(){
        if (bgmSource.isPlaying){
            bgmSource.Stop();
        }
    }
}