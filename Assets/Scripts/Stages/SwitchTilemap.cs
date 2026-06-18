/* ===================================================
 * スクリプト名 : SwitchTilemap.cs
 * 用途 : Tilemapを使ったON/OFFブロックの管理
 * =================================================== */
using UnityEngine;
using UnityEngine.Tilemaps; // Tilemapの操作に必要

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
[RequireComponent(typeof(Collider2D))] // TilemapColliderでもCompositeColliderでもOK
public class SwitchTilemap : MonoBehaviour {
    public enum BlockColor { Red, Blue }

    [Header("ブロックの設定")]
    public BlockColor myColor = BlockColor.Red;

    [Header("OFF時の透明度")]
    [Range(0f, 1f)]
    public float offAlpha = 0.3f; // 0.3 なら 30% の濃さ（半透明）

    private Tilemap tilemap;
    private Collider2D col;

    void Awake() {
        tilemap = GetComponent<Tilemap>();
        col = GetComponent<Collider2D>(); // CompositeCollider2Dを使っている場合にも対応
    }

    void Start() {
        if (SwitchManager.Instance != null) {
            // マネージャーのイベントに登録
            SwitchManager.Instance.OnSwitchToggled += UpdateTilemapState;
            
            // 初期状態を反映
            UpdateTilemapState(SwitchManager.Instance.isRedOn);
        }
    }

    void OnDestroy() {
        if (SwitchManager.Instance != null) {
            SwitchManager.Instance.OnSwitchToggled -= UpdateTilemapState;
        }
    }

    private void UpdateTilemapState(bool isRedOn) {
        bool isSolid = (myColor == BlockColor.Red && isRedOn) || (myColor == BlockColor.Blue && !isRedOn);

        if (isSolid) {
            // ▼ 実体化（ON）
            col.enabled = true;
            tilemap.color = new Color(1f, 1f, 1f, 1f); // 完全に不透明
        } else {
            // ▼ すり抜け（OFF）
            col.enabled = false;
            tilemap.color = new Color(1f, 1f, 1f, offAlpha); // 半透明にして点線っぽさを表現
        }
    }
}