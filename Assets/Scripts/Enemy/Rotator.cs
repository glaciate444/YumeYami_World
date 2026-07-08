/* ===================================================
 * スクリプト名 : Rotator.cs
 * Version : Ver0.02
 * Since : 2026/05/14
 * Update : 2026/05/14
 * 用途 : 単なる「回るための機械」のスクリプト
 * 更新 : 反時計回りを実装
 * =================================================== */
using UnityEngine;

public class Rotator : MonoBehaviour {
    // ▼ インスペクターで選べるようにするためのリストを作成
    public enum RotationDirection {
        Clockwise,      // 時計回り
        CounterClockwise // 反時計回り
    }

    [Header("回転設定")]
    [Tooltip("1秒間に何度回るか（例：360なら1秒で1周）")]
    public float rotationSpeed = 100f;

    [Tooltip("回転する方向を選択してください")]
    public RotationDirection direction = RotationDirection.Clockwise;

    void Update() {
        // Unityの仕様に合わせて、時計回りならマイナス、反時計回りならプラスにする
        float dirMultiplier = (direction == RotationDirection.Clockwise) ? -1f : 1f;
        
        // Z軸を毎フレーム回し続ける
        transform.Rotate(0, 0, rotationSpeed * dirMultiplier * Time.deltaTime);
    }
}
/*========================================================
 * エディタでの組み立て方
 * ヒエラルキーで空のオブジェクト（Create Empty）を作り、名前を FireBarPivot（中心軸）などにします。
 * その FireBarPivot に、先ほど作った Rotator.cs をアタッチします。
 * すでに作成済みの「敵のプレハブ（重力を0にしたもの、またはトゲなどの罠）」をシーンに出し、FireBarPivot の子オブジェクト に入れます。
 * 子にした敵を、中心から少しずつズラして一列に並べます。
 * これだけで、ゲームを再生すると FireBarPivot が回転し、子である敵たちも勝手に大車輪のように振り回されます！
 * マリオのファイアーバーなど、多くの2Dゲームの回転ギミックはこの手法で作られています。
 * =======================================================*/
