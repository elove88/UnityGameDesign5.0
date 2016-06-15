using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// プレイヤーのショット制御.
// ----------------------------------------------------------------------------
public class PlayerShotController : MonoBehaviour {

	public float bulletSpeed = 15f;					// 弾のスピード.
	public int power = 2;							// 攻撃力.

	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	private GameObject player;							// プレイヤーのインスタンス.
	
	private float breakingDistance = 20f;			// 弾消滅条件(プレイヤーと弾の距離が指定以上).

	private bool isClear = false;					// 弾消去.
	
	void Start () 
	{
		// 戦闘空間を取得.
		battleSpaceContoller =
			GameObject.FindGameObjectWithTag("BattleSpace")
				.GetComponent<BattleSpaceController>();
		
		// playerのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update ()
	{
		// 弾を前進させる.
		ForwardBullet();
	
		// 敵機に当たったら弾を消滅させる.
		IsDestroy();
		
		// 弾が範囲外の場合は消滅させる.
		IsOverTheDistance();
	}
	
	// ------------------------------------------------------------------------
	// 弾を前進させる.
	// ------------------------------------------------------------------------
	private void ForwardBullet()
	{
		// 弾を進める.
		transform.Translate ( new Vector3( 0f, 0f, bulletSpeed * Time.deltaTime ) );
		
		// 戦闘空間のスクロール方向を加える.
		transform.position -= battleSpaceContoller.GetAdditionPos();
	}
	
	// ------------------------------------------------------------------------
	// 弾が当たった時の処理.
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		if ( collider.tag == "Enemy" )
		{
			// 敵機へ破壊の指示.
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
		if ( collider.tag == "Stone" )
		{
			// 岩石への破壊の指示.
			isClear = true;
			collider.gameObject.SendMessage( "SetIsBreakByShot", power );
		}
		
	}
		
	
	// ------------------------------------------------------------------------
	// 自身の破壊処理.
	// ------------------------------------------------------------------------
	private void IsDestroy()
	{
		
		if ( isClear )
		{
			// 弾の消去.
			Destroy( this.gameObject );
		}
		
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーとの距離が一定以上の場合は消去する.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance > breakingDistance )
		{
			// 弾の消去.
			Destroy( this.gameObject );
		}
	}
}
