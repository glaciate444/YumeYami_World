/* ===================================================
 * スクリプト名 : BossTeleportMove.cs
 * 用途 : ボス用の瞬間移動（ワープ）スクリプト
 * =================================================== */
using UnityEngine;
using System.Collections;

public class BossTeleportMove : MonoBehaviour {
    public enum TeleportMode { Sequential, Random }

    [Header("瞬間移動設定")]
    [Tooltip("Sequential: 登録順(A→B→C), Random: ランダム")]
    public TeleportMode mode = TeleportMode.Sequential;

    [Tooltip("何秒ごとにワープするか")]
    public float teleportInterval = 3f;

    [Tooltip("ワープ先のポイント（空のオブジェクトを登録）")]
    public Transform[] teleportPoints;

    [Header("エフェクト（任意）")]
    [Tooltip("ワープ時と出現時に出すパーティクル")]
    public GameObject teleportEffectPrefab;

    private int currentIndex = 0;
    private float timer = 0f;
    private Animator anim;

    void Start(){
        anim = GetComponent<Animator>();
        timer = teleportInterval;
    }

    void Update(){
        timer -= Time.deltaTime;
        if (timer <= 0f){
            Teleport();
            timer = teleportInterval;
        }
    }

    private void Teleport(){
        if (teleportPoints == null || teleportPoints.Length == 0) return;

        // 1. 消える瞬間のエフェクト
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

        // 2. 次の移動先を決定する
        if (mode == TeleportMode.Sequential){
            currentIndex++;
            if (currentIndex >= teleportPoints.Length) currentIndex = 0; // 最後まで行ったら最初に戻る
        }else if (mode == TeleportMode.Random){
            int nextIndex = currentIndex;
            // 同じ場所に連続でワープしないように再抽選
            while (nextIndex == currentIndex && teleportPoints.Length > 1){
                nextIndex = Random.Range(0, teleportPoints.Length);
            }
            currentIndex = nextIndex;
        }

        // 3. 座標を移動させる
        transform.position = teleportPoints[currentIndex].position;

        // 4. 出現した瞬間のエフェクト
        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

        // 5. 向きをプレイヤーの方へ自動補正する（おまけ機能）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null){
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            transform.localScale = new Vector3(direction, transform.localScale.y, transform.localScale.z);
        }
    }
}