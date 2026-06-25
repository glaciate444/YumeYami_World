/* ===================================================
 * スクリプト名 : Boss.cs
 * Version : Ver0.03
 * 用途 : ボスのステータス管理、HPバー連動、登場演出
 * 拡張 : 撃破時に地面に落下し、パーティクルを出して亡骸を残すコルーチンを追加
 * =================================================== */
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour, IDamageable{

    public enum BossType{
        StageBoss,   
        RoomGuarder  
    }

    [Header("ボス基本ステータス")]
    public BossType bossType = BossType.RoomGuarder; 
    public string bossName = "大ボス";
    public int maxHp = 50;
    private int currentHp;

    [Header("UI連携")]
    public Slider bossHpSlider;
    public TMP_Text bossHpText;

    [Header("ルームガーダー用解放設定")]
    public GameObject entranceBlocker;
    public GameObject entranceBlockerR;
    public GameObject bossCameraObj;

    // ▼【新規追加】撃破時のエフェクト設定
    [Header("撃破エフェクト設定")]
    [Tooltip("ボスのHPが0になった時に発生させる爆発エフェクトのプレハブ")]
    public GameObject deathParticlePrefab;

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyTurret[] turrets;

    private bool isBattleStarted = false;
    private bool isDead = false;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        turrets = GetComponentsInChildren<EnemyTurret>();
    }

    void Start(){
        if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);
        foreach (EnemyTurret t in turrets) t.enabled = false;
    }

    public void StartBossBattle(){
        if (isBattleStarted) return;
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine(){
        Debug.Log($"{bossName} が現れた！");

        if (bossHpSlider != null){
            bossHpSlider.gameObject.SetActive(true);
            bossHpSlider.maxValue = maxHp;
            bossHpSlider.value = 0;
            bossHpText.text = bossHpSlider.value.ToString("0");

            float elapsed = 0f;
            float duration = 1.5f; 

            while (elapsed < duration){
                elapsed += Time.deltaTime;
                bossHpSlider.value = Mathf.Lerp(0f, maxHp, elapsed / duration);
                bossHpText.text = bossHpSlider.value.ToString("0");
                yield return null;
            }
            bossHpSlider.value = maxHp;
        }

        currentHp = maxHp;
        isBattleStarted = true;

        foreach (EnemyTurret t in turrets) t.enabled = true;
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (!isBattleStarted || isDead) return;

        currentHp -= damage;

        if (bossHpSlider != null){
            bossHpSlider.value = currentHp;
            bossHpText.text = currentHp.ToString();
        }

        if (anim != null){
            anim.SetTrigger("Damage");
        }

        if (currentHp <= 0){
            Die();
        }
    }

    private void Die(){
        isDead = true;
        isBattleStarted = false;
        
        // UIを隠す
        if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);

        // すべての砲台の攻撃をストップする
        foreach (EnemyTurret t in turrets) t.enabled = false;

        // ▼ 撃破演出のコルーチンをスタート！ ▼
        StartCoroutine(DieRoutine());
    }

    // ==========================================
    // ▼ 撃破演出コルーチン（ここに今後の処理をどんどん追加できます！）
    // ==========================================
    private IEnumerator DieRoutine() {
        // 1. フワフワ移動を強制停止する
        BossHoverMove hoverMove = GetComponent<BossHoverMove>();
        if (hoverMove != null) hoverMove.enabled = false;

        // 2. 当たり判定（トリガー）を消して、プレイヤーが接触ダメージを受けないようにする
        // （※物理的なコライダーは残すため、地面はすり抜けません）
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach(Collider2D col in colliders){
            if (col.isTrigger) col.enabled = false;
        }

        // 3. ダメージモーションで固定する
        if (anim != null) {
            anim.Play("Damage"); 
            // もしアニメーションが元に戻ってしまう場合は、アニメーター自体を止めるのもアリです
            // anim.enabled = false; 
        }

        // 4. 重力をONにして地面にドスンと落とす
        if (rb != null) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f; // ドスンと落ちるように重力を強めに設定
        }

        // 5. 地面に落ちるまで少し待つ（0.5秒くらい）
        yield return new WaitForSeconds(0.5f);

        // 6. 爆発エフェクト（パーティクル）を発生させる
        if (deathParticlePrefab != null) {
            // ボスの少し手前（Z軸）に発生させると綺麗に見えます
            Vector3 effectPos = transform.position + new Vector3(0, 0, -1f);
            Instantiate(deathParticlePrefab, effectPos, Quaternion.identity);
            
            // ドカン！という爆発SEがあればここで鳴らす
            // SoundManager.instance.PlaySE(explosionSE);
        }

        // 亡骸が転がらないように物理演算をピタッと止める
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // 7. その後、部屋のロックを解除する（爆発した後にドアが開く方が気持ちいいです）
        if (bossType == BossType.RoomGuarder){
            if (entranceBlocker != null){
                entranceBlocker.SetActive(false);
                entranceBlockerR.SetActive(false);
            }
            if (bossCameraObj != null){
                bossCameraObj.SetActive(false);
            }
        } else if (bossType == BossType.StageBoss) {
            // ステージボスの場合は、数秒待ってからリザルト画面を呼ぶなどの処理をここに書きます
        }

        // --- 今後やりたい処理の追加スペース ---
        // 例：コインをドロップさせる
        // 例：勝利のBGMを鳴らす
        
        // ※Destroy(gameObject) は書かないため、スプライト（亡骸）は残り続けます！
    }
}