using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // SPゲージ用
using TMPro;

public class PlayerShoot : MonoBehaviour{
    // ▼ 新しくSOの枠を追加
    [Header("現在の装備（青枠）")]
    public ItemInventoryData currentSpecialEquip;
    public Transform firePoint;

    [Header("SP設定")]
    public int maxSp = 6;
    public int currentSp;
    public Slider spSlider;             // キャンバスに作ったSPゲージ
    public TMP_Text spText;

    private PlayerControls inputActions;
    private Animator anim; // ←【追加1】Animator用の変数を用意

    void Awake(){
        anim = GetComponent<Animator>();
        inputActions = new PlayerControls();
        inputActions.Player.Shoot.performed += context => Shoot();
    }

    void Start(){
        // GameManager が存在する場合、成長後の最大SPを読み込む
        if (GameManager.Instance != null){
            maxSp = GameManager.Instance.currentMaxSp;
        }

        // SPを満タンにする
        currentSp = maxSp;
        UpdateUI();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    // PlayerShoot.cs に追加
    public void RecoverSp(int amount){
        currentSp += amount;
        // 最大値を越えないように制限（クランプ）
        currentSp = Mathf.Clamp(currentSp, 0, maxSp);
        UpdateUI();

        Debug.Log($"SPを {amount} 回復しました。現在：{currentSp}");
    }
    private void Shoot(){
        // 装備がない、またはプレハブが設定されていない場合は何もしない
        if (currentSpecialEquip == null || currentSpecialEquip.actionPrefab == null) return;

        // SOから消費SPを取得
        int cost = currentSpecialEquip.spCost;

        if (currentSp >= cost){
            currentSp -= cost;
            UpdateUI();

            // ▼ SOからプレハブを取得して生成するように変更
            GameObject bullet = Instantiate(currentSpecialEquip.actionPrefab, firePoint.position, firePoint.rotation);

            float facingDirection = Mathf.Sign(transform.localScale.x);
            Vector2 shootDir = new Vector2(facingDirection, 0);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null){
                PlayerController pc = GetComponent<PlayerController>();
                if (pc != null){
                    b.damage += pc.passiveAttackBonus;
                }
                b.Initialize(shootDir);
            }

            bullet.GetComponent<Bullet>().Initialize(shootDir);

            if (anim != null) anim.SetTrigger("Shoot");
        }else{
            Debug.Log("SP不足で撃てない！");
        }
    }

    private void UpdateUI(){
        if (spSlider != null){
            spSlider.maxValue = maxSp;
            spSlider.value = currentSp;
        }
        spText.text = currentSp.ToString();
    }
}