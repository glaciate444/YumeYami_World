/* ===================================================
 * スクリプト名 : SwitchBlock.cs
 * 用途 : ON/OFFの状態に合わせて出現・透明化するブロック
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SwitchBlock : MonoBehaviour {
    public enum BlockColor { Red, Blue }

    [Header("ブロックの設定")]
    public BlockColor myColor = BlockColor.Red;
    
    [Header("画像設定")]
    public Sprite solidSprite;   // 実体化している時の画像（ブロック）
    public Sprite outlineSprite; // 点線になっている時の画像（枠だけ）

    private BoxCollider2D col;
    private SpriteRenderer sr;

    void Awake() {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start() {
        if (SwitchManager.Instance != null) {
            // マネージャーの「お知らせ（イベント）」に、自分の変更メソッドを登録する
            SwitchManager.Instance.OnSwitchToggled += UpdateBlockState;
            
            // ゲーム開始時に、初期状態に合わせて姿を変えておく
            UpdateBlockState(SwitchManager.Instance.isRedOn);
        }
    }

    void OnDestroy() {
        // オブジェクトが消える時は、エラー防止のために登録を解除する
        if (SwitchManager.Instance != null) {
            SwitchManager.Instance.OnSwitchToggled -= UpdateBlockState;
        }
    }

    // マネージャーから「切り替わったよ！」と呼ばれるメソッド
    private void UpdateBlockState(bool isRedOn) {
        // 自分が「赤」で赤がON、または「青」で青がONの時だけ実体化する
        bool isSolid = (myColor == BlockColor.Red && isRedOn) || (myColor == BlockColor.Blue && !isRedOn);

        if (isSolid) {
            col.enabled = true; // 当たり判定ON（乗れる）
            sr.sprite = solidSprite;
            sr.color = new Color(1f, 1f, 1f, 1f); // 完全に不透明
        } else {
            col.enabled = false; // 当たり判定OFF（すり抜ける）
            sr.sprite = outlineSprite;
            sr.color = new Color(1f, 1f, 1f, 0.5f); // 半透明（点線）にする
        }
    }
}