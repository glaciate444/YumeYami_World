/* ===================================================
 * スクリプト名 : SoundManager.cs
 * Version : Ver0.01
 * Since : 2026/05/25
 * Update : 2026/05/25
 * 用途 : 全てのSEを一括管理・再生する（音が途切れるのを防ぐ）
 * =================================================== */
using UnityEngine;

// ▼ アタッチした時、自動的に AudioSource（スピーカー）を追加してくれる便利機能
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour{
    // どこからでもアクセスできるシングルトン
    public static SoundManager instance;

    private AudioSource seSource;

    void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);

            seSource = GetComponent<AudioSource>();
            // 効果音用なので、ループ再生や起動時の自動再生はOFFにしておく
            seSource.playOnAwake = false;
            seSource.loop = false;
        }else{
            Destroy(gameObject);
        }
    }

    // ▼ 各スクリプト（プレイヤーや敵）から呼ばれる再生メソッド ▼
    public void PlaySE(AudioClip clip){
        if (clip != null){
            // PlayOneShot は、音が重なっても途切れずに「複数同時再生」してくれる優秀な機能です
            seSource.PlayOneShot(clip);
        }
    }
}