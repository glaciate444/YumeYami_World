/* ===================================================
 * スクリプト名 : EnemyMovement.cs
 * Version : Ver0.01
 * Since : 2026/05/07
 * Update : 2026/05/07
 * 用途 : 敵の「動き」を担当する全スクリプトの親クラス
 * =================================================== */
using UnityEngine;

// 敵の「動き」を担当する全スクリプトの親クラス（基底クラス）
public abstract class EnemyMovement : MonoBehaviour{

    // ダメージを受けた時などに、Enemy.cs から呼ばれる共通命令
    public virtual void PauseMovement(bool isPaused){
        // デフォルトでは、このスクリプト自体のオン・オフを切り替える
        this.enabled = !isPaused;
    }
}