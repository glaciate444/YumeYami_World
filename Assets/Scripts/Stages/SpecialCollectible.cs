/* ===================================================
 * スクリプト名 : SpecialCollectible.cs
 * Version : Ver0.01
 * Since : 2026/05/26
 * Update : 2026/05/26
 * 用途 : ステージ固有の収集アイテム（KONGパネルやクリスタルなど）
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpecialCollectible : MonoBehaviour {

    [Header("収集アイテム設定")]
    [Tooltip("このアイテムが配置されているステージの番号")]
    public int stageId = 1;

    [Tooltip("このステージ内での固有ID（0, 1, 2, 3...と連番を振ります）")]
    public int collectibleId = 0;

    [Header("効果音")]
    public AudioClip collectSE;

    // セーブデータに記録するための「合言葉（キー）」を作るメソッド
    // 例: "Stage_1_SpecialItem_0" という固有の名前になります
    private string GetSaveKey(){
        return $"Stage_{stageId}_SpecialItem_{collectibleId}";
    }

    void Start(){
        // ▼ ゲーム開始時（ステージに入った瞬間）のチェック ▼
        // 過去にこのアイテムを取得したことがあるか（1 なら取得済み）
        if (PlayerPrefs.GetInt(GetSaveKey(), 0) == 1){

            // 既に取得済みなら、最初から消去しておく（カービィ方式）
            Destroy(gameObject);

            // ※もし「マリオワールドの取った後のドラゴンコイン」のように
            // うっすらと表示だけ残したい場合は、Destroyの代わりに以下を使います。
            /*
            GetComponent<Collider2D>().enabled = false;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) {
                sr.color = new Color(1f, 1f, 1f, 0.3f); // 半透明にする
            }
            */
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){

            // 1. 取得したという記録（1）をセーブデータに書き込んで保存！
            PlayerPrefs.SetInt(GetSaveKey(), 1);
            PlayerPrefs.Save();

            // 2. GameManager 等で UI に通知する処理があればここに書く
            // (例: K-O-N-G のUIを点灯させるなど)

            // 3. 音を鳴らす
            if (SoundManager.instance != null && collectSE != null){
                SoundManager.instance.PlaySE(collectSE);
            }

            Debug.Log($"ステージ {stageId} のスペシャルアイテム {collectibleId} をゲット！記録しました。");

            // 4. エフェクトなどを出してから消滅する
            Destroy(gameObject);
        }
    }
}