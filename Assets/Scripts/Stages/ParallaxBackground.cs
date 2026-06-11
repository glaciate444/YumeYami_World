/* ===================================================
 * スクリプト名 : パララックススクリプト
 * Version : Ver0.04
 * 用途 : 遠くの景色（空や雲）はカメラと一緒にゆっくり動く
 * 更新 : カメラのズーム（サイズ変更）に合わせて背景も拡大縮小する機能を追加
 * =================================================== */
using UnityEngine;

public class ParallaxBackground : MonoBehaviour{
    [Header("パララックス設定")]
    [Tooltip("1 = カメラに完全に追従, 0 = 通常のスクロール, 0.5 = 中間の速度")]
    public float parallaxEffect;

    [Header("Y軸の固定設定")]
    [Tooltip("カメラが上下に動いても背景が画面内の同じ高さ（Y座標）に固定されます")]
    public bool fixYToCamera = true; 

    // ▼【追加】カメラのズームに対応する設定
    [Header("ズーム追従設定")]
    [Tooltip("チェックを入れると、カメラがズームアウトした時に背景も自動で拡大されます")]
    public bool scaleWithCamera = true;

    private Transform cam;
    private Camera camComponent; // カメラのサイズ取得用
    
    private float startPosX;
    private float length;
    private float startOffsetY; 
    
    // ▼ 初期のカメラサイズと背景のスケールを記憶しておく変数
    private float startCamSize;   
    private Vector3 startScale;   

    void Start(){
        cam = Camera.main.transform;
        camComponent = Camera.main; // メインカメラのコンポーネントを取得
        
        startPosX = transform.position.x;
        startScale = transform.localScale;

        if (cam != null){
            startOffsetY = transform.position.y - cam.position.y;
        }
        
        // ▼【追加】ゲーム開始時の「カメラの映す範囲（サイズ）」を記憶しておく
        if (camComponent != null){
            startCamSize = camComponent.orthographicSize;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null){
            length = sr.bounds.size.x;

            CreateClone(length, "RightClone");
            CreateClone(-length, "LeftClone");
        }else{
            Debug.LogWarning("SpriteRendererが見つかりません。");
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
    }

    void LateUpdate(){
        if (cam == null) return;

        // ▼【追加】カメラのズームに合わせて背景のスケール（大きさとループ幅）を調整する
        float currentLength = length;
        
        if (scaleWithCamera && camComponent != null && startCamSize > 0f){
            // 現在のカメラサイズ ÷ 初期のカメラサイズ で「何倍ズームアウトしたか」を計算
            float scaleRatio = camComponent.orthographicSize / startCamSize;
            
            // 背景の大きさをカメラのズーム倍率と同じにする（分身も一緒に大きくなります！）
            transform.localScale = startScale * scaleRatio;
            
            // 背景がループする「幅」も、拡大した分だけ広げる
            currentLength = length * scaleRatio;
        }

        float temp = (cam.position.x * (1 - parallaxEffect));
        float dist = (cam.position.x * parallaxEffect);

        float targetY = fixYToCamera ? cam.position.y + startOffsetY : transform.position.y;

        transform.position = new Vector3(startPosX + dist, targetY, transform.position.z);

        // ▼ ループ判定には、拡大・縮小を考慮した currentLength を使う
        if (temp > startPosX + currentLength){
            startPosX += currentLength;
        }else if (temp < startPosX - currentLength){
            startPosX -= currentLength;
        }
    }
}