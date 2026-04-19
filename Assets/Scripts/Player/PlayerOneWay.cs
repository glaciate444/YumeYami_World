using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerOneWay : MonoBehaviour{
    [Header("すり抜け床設定")]
    public LayerMask oneWayLayer; // インスペクターで OneWayPlatform レイヤーを指定！
    public Transform groundCheck; // プレイヤーの足元にある空オブジェクトをセット！
    public float checkRadius = 0.2f;

    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Collider2D playerCollider;

    void Awake(){
        inputActions = new PlayerControls();
        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;

        // プレイヤー自身のコライダーを取得（種類を問わないよう Collider2D にしています）
        playerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update(){
        // 下入力判定（Sキーやスティック下）
        if (moveInput.y < -0.5f){
            TryDrop();
        }
    }

    private void TryDrop(){
        // 自分の足元に「oneWayLayer」が設定されたコライダーがあるか探す
        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, checkRadius, oneWayLayer);

        if (platform != null)
        {
            // 見つかったら、すり抜け処理のコルーチンを開始
            StartCoroutine(DisableCollision(platform));
        }
    }

    private IEnumerator DisableCollision(Collider2D platformCollider){
        // プレイヤーと、見つけた足場（Tilemap全体も可）との衝突を無視する
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

        // 床を通り抜けるまで少し待つ（落下速度により調整してください）
        yield return new WaitForSeconds(0.3f);

        // 衝突を元に戻す
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}