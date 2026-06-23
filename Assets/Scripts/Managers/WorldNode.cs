/* ===================================================
 * スクリプト名 : WorldNode.cs
 * Version : Ver0.01
 * 用途 : 各レベルの入り口となるアイコン用のスクリプトです。
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class WorldNode : MonoBehaviour {
    [Header("ワールド設定")]
    public WorldData myWorldData; 

    [Header("UI設定")]
    public Image nodeImage;       
    public Color lockedColor = Color.gray;   
    public Color unlockedColor = Color.white;

    [Header("隣接ノード")]
    public WorldNode upNode;
    public WorldNode downNode;
    public WorldNode leftNode;
    public WorldNode rightNode;

    public bool IsUnlocked { get; private set; }

    public void SetupNode() {
        if (myWorldData == null) return;

        int currentLevel = 1;
        if (GameManager.Instance != null) {
            currentLevel = GameManager.Instance.unlockedWorldLevel;
        }

        IsUnlocked = currentLevel >= myWorldData.requiredWorldLevel;
        UpdateVisuals();
    }

    private void UpdateVisuals() {
        if (nodeImage == null) return;
        nodeImage.color = IsUnlocked ? unlockedColor : lockedColor;
    }
}