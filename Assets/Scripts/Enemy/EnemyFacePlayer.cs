using UnityEngine;

public class EnemyFacePlayer : MonoBehaviour {
    [Header("向き設定")]
    [Tooltip("元々のスプライト画像が『右』を向いて描かれている場合はチェックを入れます")]
    public bool isFacingRightDefault = false;

    private Transform playerTransform;

    void Start(){
        // シーン内のプレイヤーを探して記憶
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null){
            playerTransform = player.transform;
        }
    }

    void Update(){
        // プレイヤーが見つからない、または死んで消えている場合は何もしない
        if (playerTransform == null) return;

        // 自分とプレイヤーのX座標の差を計算
        float directionX = playerTransform.position.x - transform.position.x;

        // プレイヤーと完全に重なっている時などにガタガタ震えるのを防ぐため、少し差がある時だけ向きを変える
        if (Mathf.Abs(directionX) > 0.1f){
            // プレイヤーが右にいれば 1、左にいれば -1 になる
            float sign = Mathf.Sign(directionX);

            // 元の画像が左向きか右向きかで、反転の計算を変える
            float scaleX = isFacingRightDefault ? sign : -sign;

            // Xのスケールだけを書き換えて反転させる（YとZは元のまま）
            transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        }
    }
}