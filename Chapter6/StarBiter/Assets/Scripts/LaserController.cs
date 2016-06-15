using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Laser から出すレーザーの動きを制御する.
// ----------------------------------------------------------------------------
public class LaserController : MonoBehaviour {

	public float bulletSpeed = 1f;					// レーザーのスピード.
	public float laserSize = 100f;					// レーザーの長さ.
	
	private GameObject target;						// ターゲット.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	
	private float breakingDistance = 20f;			// レーザー消滅条件(ターゲットとレーザーの距離が指定以上).
	private bool isStart = false;					// レーザー射出(true:射出).
	
	void Start () {
		
		// 戦闘空間を取得.
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// レーザーを前進させる.
			ForwardBullet();
		
			// ターゲットが消滅したらレーザーも消滅させる.
			IsDestroyTarget();
			
			// レーザーが範囲外の場合は消滅させる.
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// レーザーを前進させる.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 敵機がある時のみ処理.
		if ( target )
		{
			// レーザーを進める.
			transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
			
			// 戦闘空間のスクロール方向を加える.
			transform.position -= battleSpaceContoller.GetAdditionPos();
			
			// レーザーを伸ばす.
			if ( transform.localScale.z < laserSize )
			{
				transform.localScale = new Vector3( 
					transform.localScale.x,
					transform.localScale.y,
					transform.localScale.z + ( bulletSpeed * Time.deltaTime * 30 ) );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ターゲットをセット.
	// ------------------------------------------------------------------------
	public void SetTarget( GameObject target )
	{
		this.target = target;
		
		// レーザーの方向を指定.
		SetDirection();
		
		isStart = true;	// レーザー射出.
	}
	
	// ------------------------------------------------------------------------
	// 進行方向をターゲットに向ける.
	// ------------------------------------------------------------------------
	private void SetDirection()
	{
		// ターゲットがある時のみ処理.
		if ( target )
		{
			// ターゲットの方向を取得.
			Vector3 targetPosition = target.gameObject.transform.position;
			Vector3 relativePosition = targetPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// レーザーの角度を変更.
			transform.rotation = targetRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// 自身の破壊処理.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// レーザーの消去.
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
				// レーザーの消去.
				Destroy( this.gameObject );
			}
		}
	}
}
