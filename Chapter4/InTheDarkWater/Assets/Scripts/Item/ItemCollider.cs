using UnityEngine;
using System.Collections;

/// <summary>
/// アイテムの衝突
/// </summary>
public class ItemCollider : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;  // 念のためフラグ管理しておく

    void Start()
    {
    }

    /// <summary>
    /// Noteからの削除許可
    /// </summary>
//    void OnDestroyObject()
    void OnDestroyLicense()
    {
        // 親から消してもらう
        transform.parent.gameObject.SendMessage("OnDestroyObject", gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckPlayer(collision.gameObject);
    }
    void OnCollisionStay(Collision collider)
    {
        CheckPlayer(collider.gameObject);
    }

    void CheckPlayer(GameObject target)
    {
        if (! valid) return;
        if (!target.CompareTag("Player")) return;
        // Colliderを切る
        GetComponent<Collider>().enabled = false;
        valid = false;
        // プレイヤーと接触した際の効果
        SendMessage("OnRecovery");
    }

}
