using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SinkingPlatform : MonoBehaviour{
    [Header("沈み込み設定")]
    public float sinkDistance = 0.2f; // どのくらい下がるか
    public float sinkSpeed = 2.0f;    // 沈むときの速さ
    public float returnSpeed = 1.0f;  // 元に戻るときの速さ

    [Tooltip("チェックを入れると、プレイヤーが降りた際に元の高さに戻ります")]
    public bool autoReturn = true;

    private Rigidbody2D rb;
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private bool isPlayerOn = false;

    void Start(){
        rb = GetComponent<Rigidbody2D>();

        // 落下しないため、常にKinematicにしておく
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 初期位置と沈み込んだ後の位置を記憶
        initialPosition = transform.position;
        targetPosition = initialPosition - new Vector2(0, sinkDistance);
    }

    // 物理的な移動を伴うため、UpdateではなくFixedUpdateを使用
    void FixedUpdate(){
        // プレイヤーが乗っているか、autoReturnがオフで一度でも乗ったことがあるなら沈む位置へ
        Vector2 currentTarget = (isPlayerOn || (!autoReturn && transform.position.y < initialPosition.y && !isPlayerOn && transform.position.y == targetPosition.y)) ? targetPosition : initialPosition;

        // autoReturnがfalseの場合、プレイヤーが降りても戻らない処理
        if (!autoReturn && !isPlayerOn){
            currentTarget = targetPosition;
        }else if (autoReturn && !isPlayerOn){
            currentTarget = initialPosition;
        }else if (isPlayerOn){
            currentTarget = targetPosition;
        }

        float currentSpeed = isPlayerOn ? sinkSpeed : returnSpeed;

        // 目標位置へ指定速度で移動させる
        if (rb.position != currentTarget){
            Vector2 newPos = Vector2.MoveTowards(rb.position, currentTarget, currentSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        // プレイヤーが「上から」乗った時だけ作動させる（既存コード踏襲）
        if (collision.gameObject.CompareTag("Player")){
            if (collision.transform.position.y > transform.position.y){
                isPlayerOn = true;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            // 床が沈む際、プレイヤーを一緒に動かすための親子関係設定（既存コード踏襲）
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Player")){
            isPlayerOn = false;

            // 床から離れたら親子関係を解消する（既存コード踏襲）
            collision.transform.SetParent(null);
        }
    }
}