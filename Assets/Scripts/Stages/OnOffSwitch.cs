/* ===================================================
 * スクリプト名 : OnOffSwitch.cs
 * 用途 : 叩かれると状態を反転させるスイッチ本体
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OnOffSwitch : MonoBehaviour, IDamageable {
    [Header("スイッチの画像")]
    public Sprite redStateSprite;  // 赤がONの時の画像（赤いスイッチ等）
    public Sprite blueStateSprite; // 青がONの時の画像（青いスイッチ等）

    private SpriteRenderer sr;
    private Animator anim;

    // 連打防止用のタイマー
    private float cooldownTimer = 0f;
    private float cooldownLimit = 0.2f;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start() {
        if (SwitchManager.Instance != null) {
            SwitchManager.Instance.OnSwitchToggled += UpdateSwitchAppearance;
            UpdateSwitchAppearance(SwitchManager.Instance.isRedOn);
        }
    }

    void OnDestroy() {
        if (SwitchManager.Instance != null) {
            SwitchManager.Instance.OnSwitchToggled -= UpdateSwitchAppearance;
        }
    }

    void Update() {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
    }

    // 1. 【武器や魔法で攻撃された時】（IDamageableの機能）
    public void TakeDamage(int damage, Vector2 knockbackDirection) {
        HitSwitch();
    }

    // 2. 【マリオのように下から頭突きした時】
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            // プレイヤーの頭の高さと、スイッチの底面の高さを比べる
            float playerTopY = other.collider.bounds.max.y;
            float switchBottomY = GetComponent<Collider2D>().bounds.min.y;

            // プレイヤーの頭が、スイッチの底面より上にあれば「下から叩いた」と判定
            if (playerTopY > switchBottomY - 0.2f) {
                HitSwitch();
            }
        }
    }

    // スイッチを叩いた時の共通処理
    private void HitSwitch() {
        if (cooldownTimer > 0f) return; // 連続で叩かれるのを防ぐ
        cooldownTimer = cooldownLimit;

        if (SwitchManager.Instance != null) {
            SwitchManager.Instance.Toggle();
        }

        // ※もし叩かれた時にアニメーションさせたい場合はここでTriggerを呼びます
        if (anim != null) anim.SetTrigger("Hit");
    }

    // 見た目の変更
    private void UpdateSwitchAppearance(bool isRedOn) {
        if (isRedOn) {
            sr.sprite = redStateSprite;
        } else {
            sr.sprite = blueStateSprite;
        }
    }
}