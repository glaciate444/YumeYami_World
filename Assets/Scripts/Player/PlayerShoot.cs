using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // SPゲージ用

public class PlayerShoot : MonoBehaviour{
    [Header("射撃設定")]
    public GameObject projectilePrefab; // さきほど作ったBulletプレハブを入れる
    public Transform firePoint;         // 弾が出る位置（銃口）

    [Header("SP設定")]
    public int maxSp = 6;
    public int currentSp;
    public int spCost = 1;              // 1発あたりの消費SP
    public Slider spSlider;             // キャンバスに作ったSPゲージ

    private PlayerControls inputActions;


    void Awake(){
        RecoverSp(maxSp);
        currentSp = maxSp;
        UpdateUI();

        inputActions = new PlayerControls();
        // Shootアクションが呼ばれたら Shoot() メソッドを実行
        inputActions.Player.Shoot.performed += context => Shoot();
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
        // SPが足りているかチェック
        if (currentSp >= spCost){
            currentSp -= spCost;
            UpdateUI();

            // 弾を生成
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // プレイヤーが向いている方向（スケールのX）を取得
            float facingDirection = Mathf.Sign(transform.localScale.x);
            Vector2 shootDir = new Vector2(facingDirection, 0);

            // 弾に方向を渡して飛ばす
            bullet.GetComponent<Bullet>().Initialize(shootDir);
        }else{
            Debug.Log("SP不足で撃てない！");
            // ここで「ブブッ」という音を鳴らしたり、SPゲージを赤く点滅させたりします
        }
    }

    private void UpdateUI(){
        if (spSlider != null){
            spSlider.maxValue = maxSp;
            spSlider.value = currentSp;
        }
    }
}