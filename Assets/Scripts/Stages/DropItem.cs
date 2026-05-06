/* ===================================================
 * スクリプト名 : DropItem.cs
 * Version : Ver0.01
 * Since : 2026/05/05
 * Update : 2026/05/05
 * 用途 : アイテムドロップの動き
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DropItem : MonoBehaviour{
    [Header("飛び出す力")]
    public float minJumpForce = 5f;    // 上に跳ねる力の最小値
    public float maxJumpForce = 8f;    // 上に跳ねる力の最大値
    public float horizontalForce = 2f; // 左右に散らばる力

    void Start(){
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // ランダムな力を計算する（上方向はランダム、左右もランダム）
        float jumpX = Random.Range(-horizontalForce, horizontalForce);
        float jumpY = Random.Range(minJumpForce, maxJumpForce);

        // コインに瞬間的な力（Impulse）を加えて弾き飛ばす
        rb.AddForce(new Vector2(jumpX, jumpY), ForceMode2D.Impulse);
    }
}