/* ===================================================
 * スクリプト名 : Boss.cs
 * Version : Ver0.02
 * Since : 2026/05/23
 * Update : 2026/05/23
 * 用途 : ボスのステータス管理、HPバー連動、登場演出
* 拡張 : 大ボス/中ボスのenum切り替え、撃破時の部屋ロック解除に対応
 * =================================================== */
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Sliderの操作に必要

[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour, IDamageable{

    // ▼【追加】ボスの種類の定義
    public enum BossType{
        StageBoss,   // 大ボス（倒したらステージクリアなど）
        RoomGuarder  // 中ボス / ルームガーダー（倒したら部屋を開放）
    }

    [Header("ボス基本ステータス")]
    public BossType bossType = BossType.RoomGuarder; // インスペクターで選択
    public string bossName = "大ボス";
    public int maxHp = 50;
    private int currentHp;

    [Header("UI連携")]
    [Tooltip("HUD_Canvas内にある『BossHealthBar』のSliderオブジェクトをセット")]
    public Slider bossHpSlider;
    public TMP_Text bossHpText;

    // ▼【追加】中ボス（RoomGuarder）用の連動オブジェクト
    [Header("ルームガーダー用解放設定")]
    [Tooltip("倒したときに消去する見えない壁（Entrance Blocker）をセット")]
    public GameObject entranceBlocker;
    public GameObject entranceBlockerR;
    [Tooltip("倒したときにオフにするボス部屋カメラ（BossRoomCamera）をセット")]
    public GameObject bossCameraObj;

    private Rigidbody2D rb;
    private Animator anim;

    // ▼【追加】ボスにくっついている全ての砲台を管理する配列
    private EnemyTurret[] turrets;

    // ボス戦が完全に開始されたか（演出中などはfalseにして行動を制限する）
    private bool isBattleStarted = false;
    private bool isDead = false;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        // 自身の子オブジェクトに付いている全ての EnemyTurret を探し出して配列にしまう
        turrets = GetComponentsInChildren<EnemyTurret>();
    }

    void Start(){
        // ゲーム開始時はボスのHPバーを非表示にしておく
        if (bossHpSlider != null){
            bossHpSlider.gameObject.SetActive(false);
        }

        // 登場前は、すべての砲台の電源（enabled）をOFFにしておく
        foreach (EnemyTurret t in turrets){
            t.enabled = false;
        }
    }

    private IEnumerator TestTriggerRoutine(){
        yield return new WaitForSeconds(1.0f);
        StartBossBattle();
    }

    // ▼ ボス戦のスイッチを入れるメインメソッド ▼
    public void StartBossBattle(){
        if (isBattleStarted) return;
        StartCoroutine(IntroRoutine());
    }

    // ▼ 登場演出：HPが0から最大値までグングン増えていくコルーチン ▼
    private IEnumerator IntroRoutine(){
        Debug.Log($"{bossName} が現れた！");

        if (bossHpSlider != null){
            // 1. HPバーを表示し、中身を一旦「0」にする
            bossHpSlider.gameObject.SetActive(true);
            bossHpSlider.maxValue = maxHp;
            bossHpSlider.value = 0;
            bossHpText.text = bossHpSlider.value.ToString("0");

            // 2. 1.5秒かけてゲージを0から最大値まで滑らかに増やす
            float elapsed = 0f;
            float duration = 1.5f; // 演出にかかる時間（秒）

            while (elapsed < duration){
                elapsed += Time.deltaTime;
                // Lerp（補間）を使って数値をスムーズに増加させる
                bossHpSlider.value = Mathf.Lerp(0f, maxHp, elapsed / duration);
                bossHpText.text = bossHpSlider.value.ToString();
                yield return null;
            }

            // 最後に確実に最大値に合わせる
            bossHpSlider.value = maxHp;
        }

        // 3. 内部の数値を満タンにして、いざ戦闘開始！
        currentHp = maxHp;
        isBattleStarted = true;

        Debug.Log("ボス戦、開始！");
        // ここでボスの移動AIや、EnemyTurretなどの射撃許可を出すフラグをONにすると綺麗です。
        // ▼【追加】戦闘開始の合図と共に、すべての砲台の電源をONにする！
        foreach (EnemyTurret t in turrets){
            t.enabled = true;
        }
    }

    // ▼ プレイヤーからの攻撃を受けた時の処理（IDamageable共通） ▼
    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // 登場演出中、または既に倒れているならダメージを受け付けない
        if (!isBattleStarted || isDead) return;

        currentHp -= damage;

        // HPが減ったのでUIを更新
        if (bossHpSlider != null){
            bossHpSlider.value = currentHp;
            bossHpText.text = currentHp.ToString();
        }

        Debug.Log($"{bossName} の残りHP: {currentHp}");

        // 被ダメージ用のアニメーションパラメータがあれば起動
        if (anim != null){
            anim.SetTrigger("Damage");
        }

        // ボスは一般のザコ敵のように大きく吹っ飛ぶと威厳がなくなるため、
        // ノックバック力を弱めるか、あるいは微動だにしない（AddForceしない）のがおすすめです。

        if (currentHp <= 0){
            Die();
        }
    }

    private void Die(){
        isDead = true;
        isBattleStarted = false;
        Debug.Log($"{bossName} を撃破した！");

        // 撃破されたらHPバーを非表示にする（またはリザルトへ）
        if (bossHpSlider != null){
            bossHpSlider.gameObject.SetActive(false);
        }

        // ▼【追加】中ボスだった場合の部屋の未ロック（ギミック解除）処理 ▼
        if (bossType == BossType.RoomGuarder){
            // 1. 閉じ込められていた見えない壁を消す
            if (entranceBlocker != null){
                entranceBlocker.SetActive(false);
                entranceBlockerR.SetActive(false);
                Debug.Log("封鎖が解除された！");
            }

            // 2. ボス部屋カメラをOFFにする
            // ※これをOFFにするだけで、Cinemachineが自動的にプレイヤーを追うメインカメラへ滑らかに戻してくれます！
            if (bossCameraObj != null){
                bossCameraObj.SetActive(false);
                Debug.Log("カメラワークが元に戻りました。");
            }
        }else if (bossType == BossType.StageBoss){
            // 将来的にはここに「ステージクリアのファンファーレ」や「リザルト画面への遷移」などを書きます
            Debug.Log("ステージクリア演出へ！");
        }

        // 死亡アニメーションの再生や、ドアの解錠、クリアアイテムのドロップなどをここに書く
        if (anim != null){
            anim.SetTrigger("Die");
        }

        Destroy(gameObject, 0.5f); // アニメーションの長さに合わせて消滅
    }
}