using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour{
    [Header("現在の装備（青枠）")]
    public ItemInventoryData currentSpecialEquip;
    public Transform firePoint;

    [Header("SP設定")]
    public int maxSp = 6;
    public int currentSp;

    private PlayerControls inputActions;
    private Animator anim; 

    void Awake(){
        anim = GetComponent<Animator>();
        inputActions = new PlayerControls();
        inputActions.Player.Shoot.performed += context => Shoot();
    }

    void Start(){
        if (GameManager.Instance != null){
            maxSp = GameManager.Instance.currentMaxSp;
        }
        currentSp = maxSp;
        UpdateUI();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    public void RecoverSp(int amount){
        currentSp += amount;
        currentSp = Mathf.Clamp(currentSp, 0, maxSp);
        UpdateUI();
    }

    private void Shoot(){
        if (currentSpecialEquip == null || currentSpecialEquip.actionPrefab == null) return;
        int cost = currentSpecialEquip.spCost;

        if (currentSp >= cost){
            currentSp -= cost;
            UpdateUI();

            GameObject bullet = Instantiate(currentSpecialEquip.actionPrefab, firePoint.position, firePoint.rotation);
            float facingDirection = Mathf.Sign(transform.localScale.x);
            Vector2 shootDir = new Vector2(facingDirection, 0);

            Bullet b = bullet.GetComponent<Bullet>();
            if (b != null){
                PlayerController pc = GetComponent<PlayerController>();
                if (pc != null) b.damage += pc.passiveAttackBonus;
                b.Initialize(shootDir);
            }

            if (anim != null) anim.SetTrigger("Shoot");
        }
    }

    private void UpdateUI(){
        // ▼ HUDManagerへ数値を送るだけ！
        if (HUDManager.Instance != null){
            HUDManager.Instance.UpdateSP(currentSp, maxSp);
        }
    }
}