/* ===================================================
 * スクリプト名 : ParallaxBackground.cs
 * Version : Ver0.06
 * 用途 : カメラ追従 ＋ 自動スクロール ＋ Tiled完全対応
 * 修正 : クローン（分身）生成時にDrawModeとSizeを完全にコピーする
 * =================================================== */
using UnityEngine;

public class ParallaxBackground : MonoBehaviour{
    [Header("パララックス設定")]
    [Tooltip("1 = カメラに完全に追従, 0 = 通常のスクロール, 0.5 = 中間の速度")]
    public float parallaxEffect;

    [Header("自動スクロール設定（列車用）")]
    [Tooltip("マイナスにすると左へ、プラスにすると右へ勝手に流れます。0なら止まります。")]
    public float autoScrollSpeed = 0f;

    [Header("Y軸の固定設定")]
    public bool fixYToCamera = true;

    [Header("ズーム追従設定")]
    public bool scaleWithCamera = true;

    private Transform cam;
    private Camera camComponent;

    private float startPosX;
    private float length;
    private float startOffsetY;

    private float startCamSize;
    private Vector3 startScale;
    private float currentAutoScrollDist = 0f;

    void Start(){
        cam = Camera.main.transform;
        camComponent = Camera.main;

        startPosX = transform.position.x;
        startScale = transform.localScale;

        if (cam != null){
            startOffsetY = transform.position.y - cam.position.y;
        }

        if (camComponent != null){
            startCamSize = camComponent.orthographicSize;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null){
            length = sr.bounds.size.x;

            CreateClone(length, "RightClone");
            CreateClone(-length, "LeftClone");
        }
    }

    private void CreateClone(float offsetX, string cloneName){
        GameObject clone = new GameObject(cloneName);
        clone.transform.SetParent(this.transform);

        float localOffsetX = offsetX / transform.localScale.x;
        clone.transform.localPosition = new Vector3(localOffsetX, 0, 0);
        clone.transform.localScale = Vector3.one;

        SpriteRenderer mySr = GetComponent<SpriteRenderer>();
        SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();

        cloneSr.sprite = mySr.sprite;
        cloneSr.color = mySr.color;
        cloneSr.sortingLayerName = mySr.sortingLayerName;
        cloneSr.sortingOrder = mySr.sortingOrder;

        // ▼▼▼ 追加：Tiled（タイリング）設定を分身にも完全にコピーする ▼▼▼
        cloneSr.drawMode = mySr.drawMode;
        cloneSr.tileMode = mySr.tileMode;
        cloneSr.size = mySr.size;
        // ▲▲▲ 追加ここまで ▲▲▲
    }

    void LateUpdate(){
        if (cam == null) return;

        float currentLength = length;
        if (scaleWithCamera && camComponent != null && startCamSize > 0f){
            float scaleRatio = camComponent.orthographicSize / startCamSize;
            transform.localScale = startScale * scaleRatio;
            currentLength = length * scaleRatio;
        }

        currentAutoScrollDist += autoScrollSpeed * Time.deltaTime;

        float temp = (cam.position.x * (1 - parallaxEffect)) - currentAutoScrollDist;
        float dist = (cam.position.x * parallaxEffect) + currentAutoScrollDist;

        float targetY = fixYToCamera ? cam.position.y + startOffsetY : transform.position.y;
        transform.position = new Vector3(startPosX + dist, targetY, transform.position.z);

        if (temp > startPosX + currentLength){
            startPosX += currentLength;
        }else if (temp < startPosX - currentLength){
            startPosX -= currentLength;
        }
    }
}