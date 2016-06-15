using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Shot から出す弾の動きを制御する.
// ----------------------------------------------------------------------------
public class ShotController : MonoBehaviour {

	public float bulletSpeed = 5f;					// 弾のスピード.
	
	private GameObject target;						// ターゲット.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	
	private float breakingDistance = 20f;			// 弾消滅条件(ターゲットと弾の距離が指定以上).
	private bool isStart = false;					// 弾射出(true:射出).
	
	void Start () {
		
		// 戦闘空間を取得.
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// 弾を前進させる.
			ForwardBullet();
		
			// ターゲットが消滅したら弾も消滅させる.
			IsDestroyTarget();
			
			// 弾が範囲外の場合は消滅させる.
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// 弾を前進させる.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 敵機がある時のみ処理.
		if ( target )
		{
			// 弾を進める.
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 戦闘空間のスクロール方向を加える.
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// ターゲットをセット.
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;
		
		isStart = true;	// 弾射出.
	}
	
	// ------------------------------------------------------------------------
	// 自身の破壊処理.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// 弾の消去.
			Destroy( this.gameObject );
		}
	}
	
	// ------------------------------------------------------------------------
	// ターゲットとの距離が一定以上の場合は消去する.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( target )
		{
			float distance = Vector3.Distance(
				target.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				// 弾の消去.
				Destroy( this.gameObject );
			}
		}
	}
}
