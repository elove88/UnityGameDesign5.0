using UnityEngine;
using System.Collections;

/// <summary>
/// 魚雷の衝突
/// </summary>
public class TorpedoCollider : MonoBehaviour {

    public enum OwnerType {
        Player,
        Enemy
    };
    private OwnerType owner;

    [SerializeField]
    private float delayTime = 2.0f;
    [SerializeField]
    private int damageValue = 1;

    [System.Serializable]
    public class Explosion
    {
        [SerializeField]
        private float force = 100.0f;
        [SerializeField]
        private float upwardsModifier = 0.0f;
        [SerializeField]
        private ForceMode mode = ForceMode.Impulse;

        private float radius = 3.0f;
            
        public void Add(Rigidbody target, Vector3 pos) 
        {
            target.AddExplosionForce(force, pos, radius, upwardsModifier, mode);
        }
        public void SetRadius(float value) { radius = value; }
    };
    [SerializeField]
    Explosion explosion = new Explosion(); 

    private GameObject ui = null;

	void Start () 
    {
        ui = GameObject.Find("/UI");

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider) explosion.SetRadius( sphereCollider.radius );

        // 発射した自分にヒットするので、発射数秒だけ衝突判定しない。
        GetComponent<Collider>().enabled = false;
        StartCoroutine("Delay");
	}

    /// <summary>
    /// ゲームオーバー時
    /// </summary>
    void OnGameOver()
    {
        GetComponent<Collider>().enabled = false;
    }
    /// <summary>
    /// ゲームクリア時
    /// </summary>
    void OnGameClear()
    {
        GetComponent<Collider>().enabled = false;
    }

    // Enterだけでは取り逃す可能性があるので、StayでもCollision判定する
    void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }
    void OnCollisionStay(Collision collision)
    {
        CheckCollision(collision.gameObject);
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delayTime);

        GetComponent<Collider>().enabled = true;
        Debug.Log("Wait EndCoroutine");
    }

    private void CheckCollision(GameObject target)
    {
        bool hit = false;
        hit |= CheckPlayer(target);
        hit |= CheckEnemy(target);
        hit |= CheckTorpedo(target);
        if( hit ) {
            // いずれかがヒットした場合、ヒット後の自分の処理
            BroadcastMessage("OnHit");
            // Collider無効化
            GetComponent<Collider>().enabled = false;
        }
    }
    /// <summary>
    /// 魚雷通しの衝突
    /// </summary>
    /// <param name="target">チェック対象</param>
    /// <returns></returns>
    private bool CheckTorpedo(GameObject target)
    {
        if (target.CompareTag("Torpedo"))
        {
            // 自分と同じなら魚雷同士の衝突
            Debug.Log("CheckTorpedo");
            return true;
        }
        return false;
    }
    /// <summary>
    /// プレイヤーとの衝突
    /// </summary>
    /// <param name="target">チェック対象</param>
    /// <returns></returns>
    private bool CheckPlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            Debug.Log("CheckPlayer");
            // 衝撃を与える
            explosion.Add( target.GetComponent<Rigidbody>(), transform.position );
            // ヒット通知だけ流す
            target.BroadcastMessage("OnHit");
            // ダメージ通知
            if (ui) ui.BroadcastMessage("OnDamage", damageValue, SendMessageOptions.DontRequireReceiver);
            return true;
        }
        return false;
    }
    /// <summary>
    /// 敵との衝突
    /// </summary>
    /// <param name="target">チェック対象</param>
    /// <returns></returns>
    private bool CheckEnemy(GameObject target)
    {
        if (target.CompareTag("Enemy"))
        {
            Debug.Log("CheckEnemy");
            if (owner == OwnerType.Player)
            {
                // 自分の魚雷が敵にヒットしたときだけ、敵の持っているスコアを加算する通知
                target.SendMessage("OnAddScore", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // 敵にヒット通知だけ流す
                target.BroadcastMessage("OnHit");
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 魚雷を発射したオーナーをセット
    /// </summary>
    /// <param name="type"></param>
    public void SetOwner(OwnerType type) { owner = type; }
    /// <summary>
    /// ダメージ量をセット。通常する必要なし
    /// </summary>
    /// <param name="value"></param>
    public void SetDamageValue(int value) { damageValue = value; }

}
