using UnityEngine;
using System.Collections;

/// <summary>
/// 魚雷の移動
/// </summary>
public class TorpedoBehavior : MonoBehaviour {

    [SerializeField]
    private float speed = 1.0f;

    private bool stop = false;

	void Start () 
    {
	
	}
	
	void Update () 
    {
        // 通常は前に進む
        MoveForward();
	}

    /// <summary>
    /// Noteから削除許可をうける
    /// </summary>
    void OnDestroyLicense()
    {
        // 親に伝える。親から削除してもらう
        transform.parent.SendMessage("OnDestroyChild", gameObject);
    }
    /// <summary>
    /// ヒット通知
    /// </summary>
    void OnHit()
    {
        speed = 0.0f;
        stop = true;
    }

    private void MoveForward()
    {
        if (stop) return;
        Vector3 vec = speed * transform.forward.normalized;
        GetComponent<Rigidbody>().MovePosition(GetComponent<Rigidbody>().position + vec * Time.deltaTime);
    }


    public void SetSpeed( float speed_ ) { speed = speed_; }

}
