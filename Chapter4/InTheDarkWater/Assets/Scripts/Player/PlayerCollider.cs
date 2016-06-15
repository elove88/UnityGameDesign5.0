using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーの衝突
/// </summary>
public class PlayerCollider : MonoBehaviour {

    [SerializeField]
    private float speedDown = 2.0f;

    private PlayerController controller;
    private bool damage = true;

	void Start () 
    {
        // コントローラー
        controller = GetComponent<PlayerController>();
	}

    void OnGameOver()
    {
        damage = false;
    }
    void OnGameClear()
    {
        damage = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
        else CheckTerrainCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        if (damage) CheckDamageCollision(collision.gameObject);
    }

    private void CheckDamageCollision(GameObject target)
    {
        // 若干スピードを落とす微調整(あまりスピードがありすぎるとexplosionがきかない)
        if (target.CompareTag("Torpedo"))
        {
            controller.AddSpeed( -speedDown );
        }
    }

    private void CheckTerrainCollision(GameObject target)
    {
        // テラインに接触したら沈んだ音を再生
        if (target.CompareTag("Terrain"))
        {
            PlayAudioAtOnce();
        }
    }

    private void PlayAudioAtOnce()
    {
        if (!GetComponent<AudioSource>()) return;
        if (GetComponent<AudioSource>().isPlaying) return;
        GetComponent<AudioSource>().Play();
    }
}
