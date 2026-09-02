/* ===================================================
 * スクリプト名 : MerryGoRound.cs
 * Version : Ver0.02
 * 用途 : メリーゴーランドギミック
 * 修正 : リフトと同様の親子関係（SetParent）を追加し、上下の置き去りを完全防止
 * =================================================== */
using UnityEngine;

public class MerryGoRound : MonoBehaviour{
    [Header("移動設定")]
    [Tooltip("親のポール全体が右へ進むスピード")]
    public float moveSpeedX = 2f;

    [Tooltip("親オブジェクト（pole001など）をインスペクターからセットしてください")]
    public Transform parentPole;

    [Header("馬の上下運動")]
    public float upDownSpeed = 2f;
    public float upDownHeight = 1f;
    [Tooltip("波のタイミングをずらす値（2匹の馬を交互に動かしたい場合は、片方を1や2などにずらす）")]
    public float timeOffset = 0f;

    private Vector3 startLocalPos;

    void Start(){
        startLocalPos = transform.localPosition;
    }

    void FixedUpdate(){
        if (parentPole != null){
            parentPole.Translate(Vector3.right * moveSpeedX * Time.fixedDeltaTime);
        }

        float newY = startLocalPos.y + Mathf.Sin((Time.time + timeOffset) * upDownSpeed) * upDownHeight;
        transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);
    }

    // リフトと同じ親子関係の処理を追加
    // ▼▼▼ 修正：速度の二重がけ（コンベア現象）を防止 ▼▼▼
    private void OnCollisionStay2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){
            // 親子関係にするだけで、馬の「上下」もポールの「右移動」も両方ついていきます
            other.transform.SetParent(transform);

            // ※ここで pc.platformVelocity を渡すと「親の移動 ＋ スピード追加」の二重がけになり、
            // 前に滑っていくベルトコンベア状態になるため、速度追加の処理を削除しました！
        }
    }

    private void OnCollisionExit2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){
            // 馬から離れたら親子関係を解除し、元の状態に戻す
            other.transform.SetParent(null);

            // ※こちらも platformVelocity のリセットが不要になったため削除しました
        }
    }
}