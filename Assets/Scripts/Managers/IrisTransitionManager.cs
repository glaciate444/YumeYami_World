/* ===================================================
 * スクリプト名 : IrisTransitionManager.cs
 * 用途 : プレイヤーの位置を中心に丸く閉じていくアイリスアウト演出
 * =================================================== */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IrisTransitionManager : MonoBehaviour {

    [Header("UI設定")]
    [Tooltip("真ん中が丸く透明にくり抜かれた巨大な黒い画像（Image）")]
    public RectTransform irisImageRect; 
    
    [Header("演出設定")]
    public float irisDuration = 1.5f; // 閉じるまでにかかる時間

    void Start() {
        // シーン開始時はアイリス画像を十分に大きくして画面外（開いた状態）にしておく
        if (irisImageRect != null) {
            irisImageRect.localScale = new Vector3(20f, 20f, 1f); // 画面を覆い尽くすほどの超巨大サイズ
            irisImageRect.gameObject.SetActive(false); // 普段は隠しておく
        }
    }

    public void StartIrisOut(Transform targetTransform, string nextSceneName) {
        if (irisImageRect == null) {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        StartCoroutine(IrisOutRoutine(targetTransform, nextSceneName));
    }

    private IEnumerator IrisOutRoutine(Transform target, string nextSceneName) {
        irisImageRect.gameObject.SetActive(true);

        Camera cam = Camera.main;
        float timer = 0f;

        // 開始スケール（超巨大）と終了スケール（ゼロ）
        Vector3 startScale = new Vector3(20f, 20f, 1f);
        Vector3 endScale = Vector3.zero;

        while (timer < irisDuration) {
            timer += Time.deltaTime;
            float progress = timer / irisDuration;

            // イージング（後半ほどギュン！と早く閉じるようにする計算）
            float ease = progress * progress * progress; 

            // スケールを徐々に小さくしていく
            irisImageRect.localScale = Vector3.Lerp(startScale, endScale, ease);

            // 【超重要】プレイヤーのワールド座標を、UIの画面座標に変換して追いかける！
            if (target != null && cam != null) {
                Vector3 screenPos = cam.WorldToScreenPoint(target.position);
                irisImageRect.position = screenPos;
            }

            yield return null;
        }

        // 完全に閉じきったら、次のシーン（ミニゲーム）へ遷移！
        SceneManager.LoadScene(nextSceneName);
    }
}
/*====================================
 * 🎨 エディタでのセットアップ（超重要）
 * Unityで最も簡単かつ綺麗にアイリスアウトを行うには、
 * 「全体が真っ黒で、真ん中だけ丸く透明にくり抜かれた画像」を使用します。
 * 
 * 画像の準備: ペイントソフト（Photoshopや無料ソフト、あるいはフリー素材）で、
 * 「真ん中が透明なドーナツ状の真っ黒な画像（例：1024x1024px）」 を用意し、Unityにインポートします。
 * 
 * UIの作成: ヒエラルキーに新しく Canvas を作成し、名前を IrisCanvas にします。
 * インスペクターの Sort Order を 100 などの大きな数字にして、常に一番手前に表示されるようにします。
 * 
 * 画像の配置: IrisCanvas の下に Image を作成し、用意した「穴あき画像」をセットします。
 * 
 * マネージャーの設置: 空のGameObjectを作成し、
 * 名前を IrisTransitionManager にして、先ほど作ったスクリプトをアタッチします。
 * そして Iris Image Rect の枠に、手順3で作った Image をドラッグ＆ドロップします。
 * 
*/