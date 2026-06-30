/* ===================================================
 * スクリプト名 : PlayerOneWay.cs
 * 用途 : 下キーで一方通行床をすり抜けて降りる処理
 * 拡張 : 「StrictOneWay」タグが付いた床は降りられないように修正
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerOneWay : MonoBehaviour{
    [Header("すり抜け床設定")]
    public LayerMask oneWayLayer; 
    public Transform groundCheck; 
    public float checkRadius = 0.2f;

    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Collider2D playerCollider;

    void Awake(){
        inputActions = new PlayerControls();
        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;

        playerCollider = GetComponent<Collider2D>();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update(){
        if (moveInput.y < -0.5f){
            TryDrop();
        }
    }

    private void TryDrop(){
        Collider2D platform = Physics2D.OverlapCircle(groundCheck.position, checkRadius, oneWayLayer);

        if (platform != null)
        {
            // ▼【新規追加】もし足元の床が「絶対に降りられない床(StrictOneWay)」だったら、降りるのをやめる！
            if (platform.CompareTag("StrictOneWay")) {
                return; 
            }

            StartCoroutine(DisableCollision(platform));
        }
    }

    private IEnumerator DisableCollision(Collider2D platformCollider){
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.3f);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
}