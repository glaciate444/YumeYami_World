using UnityEngine;
public class AutoDestroy : MonoBehaviour{
    public float lifetime = 0.4f; // 煙が消えるまでの秒数
    void Start(){
        Destroy(gameObject, lifetime);
    }
}