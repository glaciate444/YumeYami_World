/* ===================================================
 * スクリプト名 : BlinkingBlock.cs
 * Version : Ver0.01
 * Since : 2026/05/03
 * Update : 2026/05/03
 * 用途 : 点滅ブロック
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BlinkingBlock : MonoBehaviour{
    [Header("点滅タイミング設定（秒）")]
    [Tooltip("ゲーム開始から最初に現れるまでの待機時間（順番に現れるギミック用）")]
    public float initialDelay = 0f;

    [Tooltip("ブロックが現れている時間")]
    public float activeTime = 2f;

    [Tooltip("ブロックが消えている時間")]
    public float inactiveTime = 2f;

    [Header("見た目の設定")]
    [Tooltip("チェックを入れると、消えている時は完全に見えなくなります。外すとうっすら半透明で残ります。")]
    public bool completelyHide = false;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Start(){
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        // サイクル開始
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine(){
        // 1. 最初は「初期遅延（initialDelay）」の分だけ待つ
        if (initialDelay > 0f){
            SetBlockState(false); // 待っている間は消しておく
            yield return new WaitForSeconds(initialDelay);
        }

        // 2. 以降は無限ループで出現と消失を繰り返す
        while (true){
            // 現れる
            SetBlockState(true);
            yield return new WaitForSeconds(activeTime);

            // 消える
            SetBlockState(false);
            yield return new WaitForSeconds(inactiveTime);
        }
    }

    private void SetBlockState(bool isVisible){
        // 当たり判定のオンオフ
        // isVisible が true ならコライダーがONになり乗れる、false ならOFFになりすり抜ける
        col.enabled = isVisible;

        // 見た目のオンオフ
        if (isVisible){
            // 完全な不透明
            sr.color = new Color(1, 1, 1, 1);
        }else{
            if (completelyHide){
                // 完全な透明（見えない）
                sr.color = new Color(1, 1, 1, 0);
            }else{
                // アルファ値を0.3にして、うっすら見えるようにする（足場の場所を教える親切設計）
                sr.color = new Color(1, 1, 1, 0.3f);
            }
        }
    }
}