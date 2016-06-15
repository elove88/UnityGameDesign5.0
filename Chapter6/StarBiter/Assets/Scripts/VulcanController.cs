using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss vulcan から出すバルカン弾の動きを制御する.
// ----------------------------------------------------------------------------
public class VulcanController : MonoBehaviour {

	public float bulletSpeed = 5f;					// バルカン弾のスピード.
	
	private GameObject target;						// ターゲット.

	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	
	private float breakingDistance = 20f;			// バルカン弾消滅条件(ターゲットとバルカン弾の距離が指定以上).
	private bool isStart = false;					// バルカン弾射出(true:射出).
	
	void Start () {
		
		// 戦闘空間を取得.
		battleSpaceContoller = 
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
	}
	
	void Update () {
		
		if ( isStart )
		{
			// バルカン弾を前進させる.
			ForwardBullet();
		
			// ターゲットが消滅したらバルカン弾も消滅させる.
			IsDestroyTarget();
			
			// バルカン弾が範囲外の場合は消滅させる.
			IsOverTheDistance();
		}
	}
	
	// ------------------------------------------------------------------------
	// バルカン弾を前進させる.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 敵機がある時のみ処理.
		if ( target )
		{
			// バルカン弾を進める.
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

		isStart = true;	// バルカン弾射出.
	}
	
	// ------------------------------------------------------------------------
	// バルカン弾の進行方向をターゲットに向ける.
	// ------------------------------------------------------------------------
	private void SetRotation( float rate )
	{
		// ターゲットの方向を取得.
		Vector3 targetPosition = target.gameObject.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		
		// バルカン弾の現在の方向から敵機の方向へ、指定したスピードで傾けた後の角度を取得.
		float targetRotationAngle = targetRotation.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			rate * Time.deltaTime );
		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// レーザーの角度を変更.
		transform.rotation = tiltedRotation;
	}
	
	// ------------------------------------------------------------------------
	// 自身の破壊処理.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		if ( !target )
		{
			// バルカン弾の消去.
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
				// バルカン弾の消去.
				Destroy( this.gameObject );
			}
		}
	}
}
