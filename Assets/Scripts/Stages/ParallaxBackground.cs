/* ===================================================
 * スクリプト名 : トラップ用スクリプト
 * Version : Ver0.02
 * Since : 2026/04/29
 * Update : 2026/04/30
 * 用途 : 遠くの景色（空や雲）はカメラと一緒にゆっくり動く
 * 一番奥の背景（空・太陽など）: 1 に設定します。（カメラと完全に同じ速度で動くため、永遠に遠くにあるように見えます）
 * 中間の背景（遠くの山など）: 0.8 や 0.5 などに設定します。
 * 手前の背景（近くの木など）: 0.2 や 0.1 などに設定します。
 * =================================================== */
using UnityEngine;

public class ParallaxBackground : MonoBehaviour{
    [Header("パララックス設定")]
    [Tooltip("1 = カメラに完全に追従, 0 = 通常のスクロール, 0.5 = 中間の速度")]
    public float parallaxEffect;

    private Transform cam;
    private float startPosX;
    private float length;

    void Start(){
        cam = Camera.main.transform;
        startPosX = transform.position.x;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null){
            length = sr.bounds.size.x;

            // ▼【追加】自動で左右に「繋ぎ目」用の分身を作る！ ▼
            CreateClone(length, "RightClone");
            CreateClone(-length, "LeftClone");
        }else{
            Debug.LogWarning("SpriteRendererが見つかりません。");
        }
    }

    // --- 【追加】分身（クローン）を生成する専用メソッド ---
    private void CreateClone(float offsetX, string cloneName){
        // 新しい空のオブジェクトを作る
        GameObject clone = new GameObject(cloneName);

        // このオブジェクト（親）の子要素にする
        clone.transform.SetParent(this.transform);

        // 親のスケール（X:3など）を考慮して、ローカル座標でのズレを計算して配置
        float localOffsetX = offsetX / transform.localScale.x;
        clone.transform.localPosition = new Vector3(localOffsetX, 0, 0);

        // スケールは親の設定を引き継ぐため 1 にする
        clone.transform.localScale = Vector3.one;

        // 絵（SpriteRenderer）の設定を丸写しする
        SpriteRenderer mySr = GetComponent<SpriteRenderer>();
        SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();

        cloneSr.sprite = mySr.sprite;
        cloneSr.color = mySr.color;
        cloneSr.sortingLayerName = mySr.sortingLayerName;
        cloneSr.sortingOrder = mySr.sortingOrder;
    }
    // --------------------------------------------------

    void LateUpdate(){
        if (cam == null) return;

        float temp = (cam.position.x * (1 - parallaxEffect));
        float dist = (cam.position.x * parallaxEffect);

        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        if (temp > startPosX + length){
            startPosX += length;
        }else if (temp < startPosX - length){
            startPosX -= length;
        }
    }
}