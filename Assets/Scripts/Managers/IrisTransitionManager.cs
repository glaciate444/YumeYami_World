/* ===================================================
 * スクリプト名 : IrisTransitionManager.cs
 * 用途 : SpriteMaskを使った完璧なアイリスアウト（高解像度画像対応版）
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IrisTransitionManager : MonoBehaviour {

    [Header("演出設定")]
    public float irisDuration = 1.5f; // 閉じるまでにかかる時間

    // ▼【追加】画像の大きさに合わせて、インスペクターから調整できるようにします
    [Header("サイズ調整（自作画像用）")]
    [Tooltip("開始時の穴の大きさ。512px画像なら 6 ～ 12 あたりで綺麗に画面全体をカバーできます")]
    public float startSize = 8f; 

    [Header("スプライト設定")]
    [Tooltip("自作した白い正円のPNG画像をセットしてください")]
    public Sprite circleSprite;

    public void StartIrisOut(Transform targetTransform, string nextSceneName) {
        StartCoroutine(IrisOutRoutine(targetTransform, nextSceneName));
    }

    private IEnumerator IrisOutRoutine(Transform target, string nextSceneName) {
        // 1. プログラムで「真っ黒な四角形の画像」を自動生成
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.black);
        tex.Apply();
        Sprite blackSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        // 2. 画面全体を覆う「巨大な黒い壁」をカメラの前に作る
        GameObject blackWall = new GameObject("BlackWall");
        blackWall.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0f);
        blackWall.transform.localScale = new Vector3(200f, 200f, 1f); 
        blackWall.transform.SetParent(Camera.main.transform);

        SpriteRenderer wallSr = blackWall.AddComponent<SpriteRenderer>();
        wallSr.sprite = blackSquare;
        wallSr.sortingOrder = 32766; 
        wallSr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        // 3. プレイヤー部分を透明にくり抜く「穴（SpriteMask）」を作る
        GameObject maskObj = new GameObject("IrisMask");
        maskObj.transform.position = target.position; 

        // ▼【修正】一律100倍ではなく、設定した startSize の大きさにします
        maskObj.transform.localScale = new Vector3(startSize, startSize, 1f); 

        SpriteMask mask = maskObj.AddComponent<SpriteMask>();
        mask.sprite = circleSprite;
        mask.frontSortingOrder = 32767;
        mask.backSortingOrder = 32765;

        // 4. アニメーション処理（穴だけを徐々に小さくしていく）
        float timer = 0f;
        
        // ▼【修正】ここも startSize に合わせます
        Vector3 startScale = new Vector3(startSize, startSize, 1f);
        Vector3 endScale = Vector3.zero;

        while (timer < irisDuration) {
            timer += Time.deltaTime;
            float progress = timer / irisDuration;
            float ease = progress * progress * progress; 

            maskObj.transform.localScale = Vector3.Lerp(startScale, endScale, ease);

            if (target != null) {
                maskObj.transform.position = target.position;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(nextSceneName);
    }
}