/* ===================================================
 * スクリプト名 : HazardTrigger.cs
 * 用途 : プレイヤーが近づいた（領域に入った）時に、指定したオブジェクトをアクティブにする
 * 修正 : StartをAwakeに変更し、対象のStartがフライング発動するのを防ぐ
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HazardTrigger : MonoBehaviour{
    [Header("出現させる対象")]
    [Tooltip("ここに鉄球や敵など、隠しておきたいオブジェクトをセットします")]
    public GameObject targetObject;

    // ▼【超重要】Start() ではなく Awake() に変更します ▼
    private void Awake(){
        // AwakeはStartより先に呼ばれるため、鉄球のプログラムが動く前に完全に眠らせることができます！
        if (targetObject != null){
            targetObject.SetActive(false);
        }
        
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            
            if (targetObject != null){
                // ここで初めてアクティブになり、鉄球のStart()（10秒タイマーと初速の計算）が開始されます！
                targetObject.SetActive(true);
            }

            Destroy(gameObject);
        }
    }
}