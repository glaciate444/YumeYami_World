using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallingBlock : MonoBehaviour {
    [Header("ちくわブロック設定")]
    public float fallDelay = 1.0f;        // 乗ってから落ちるまでの時間
    public float shakeAmount = 0.05f;     // 震える幅（X軸のブレ）
    public float initialFallSpeed = -2f;  // 落下時の初速

    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private float currentTimer = 0f;
    private bool isFalling = false;
    private bool isPlayerOn = false;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // 落下までは物理演算を無効化
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 元の位置を記憶（震えた後に戻すため）
        initialPosition = transform.position;
    }

    void Update(){
        if (isFalling) return;

        if (isPlayerOn)
        {
            // 乗っている間はタイマーを進める
            currentTimer += Time.deltaTime;

            // ブルブル震える演出（元の位置を基準に左右へランダムにズラす）
            transform.position = initialPosition + new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                0f,
                0f
            );

            // タイマーが限界を超えたら落下開始
            if (currentTimer >= fallDelay)
            {
                StartFalling();
            }
        }else if (currentTimer > 0f){
            // 落ちる前に降りた場合、タイマーと位置をリセットする（重要）
            currentTimer = 0f;
            transform.position = initialPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        CheckPlayerStand(collision);
    }

    private void OnCollisionStay2D(Collision2D collision){
        // 常に乗り続けているかチェック
        CheckPlayerStand(collision);
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (isFalling) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOn = false;
        }
    }

    // プレイヤーが上から乗っているかを判定する共通処理
    private void CheckPlayerStand(Collision2D collision){
        if (isFalling) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーの足元が、ブロックの中心より上にあるか
            if (collision.transform.position.y > transform.position.y)
            {
                isPlayerOn = true;
            }
        }
    }

    private void StartFalling(){
        isFalling = true;
        isPlayerOn = false;

        // 震えによるズレを真っ直ぐに直す
        transform.position = initialPosition;

        // プレイヤーを親子関係から引き剥がす（既存の知見を流用）
        transform.DetachChildren();

        // 物理演算をONにして落下させる
        rb.bodyType = RigidbodyType2D.Dynamic;

        // スムーズに離れるように少し下向きの初速をつける
        rb.linearVelocity = new Vector2(0f, initialFallSpeed);

        // 3秒後にオブジェクトを消去
        Destroy(gameObject, 3f);
    }
}