using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01ChildController
//  - 「M02 敵機モデル タイプ01」の動き(フォーメーションのリーダーを除くメンバー)を制御する.
//  - 使い方.
//    - 本スクリプトが付いたオブジェクトをリーダーの子オブジェクトとして配置する.
//  - 動き仕様.
//    - リーダーに追従する.
//    - リーダーが破壊された場合、回避行動をとる.
// ----------------------------------------------------------------------------
public class EnemyType01ChildController : MonoBehaviour {
	
	public float speed = 2.7f;							// 敵機の進むスピード.
	public float turnSpeed = 1f;						// 旋回のスピード.
	public float escapeSpeed = 3f;						// 回避時の進むスピード.
	
	public float startDistanceToShoot = 5f;				// 弾を撃つ範囲の開始距離.
	public float endDistanceToShoot = 10f;				// 弾を撃つ範囲の終了距離.

	private bool canShoot = false;						// 弾発射条件(true: 発射可).
	
	private GameObject player;							// プレイヤー.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	private EnemyStatus enemyStatus;					// 敵機の状況.

	void Start () {
	
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");

		// 戦闘空間のインスタンスを取得.
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 敵機の状況のインスタンスを取得.
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// リーダーに追従する.
		enemyStatus.SetIsFollowingLeader( true );
	}

	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// ----------------------------------------------------------------
			// 回避する.
			// ----------------------------------------------------------------
			
			// プレイヤーの方向を取得.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				
			// 敵機の現在の方向からプレイヤーと逆の方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y - 180;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 敵機の角度を変更.
			transform.rotation = tiltedRotation;
			
			// 敵機を進める.
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
			
			// 戦闘空間のスクロール方向を加える.
			transform.position -= battleSpaceContoller.GetAdditionPos();
			
			// 弾の発射確認.
			if ( canShoot )
			{
				IsFireDistance();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻撃対象の距離か.
	// ------------------------------------------------------------------------
	private void IsFireDistance()
	{
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			isFiring = this.GetComponent<ShotMaker>().GetIsFiring();
			if ( !isFiring )
			{
				if ( IsInRange( startDistanceToShoot, endDistanceToShoot ) )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
		
	// ------------------------------------------------------------------------
	// 範囲内かどうか.
	// ------------------------------------------------------------------------
	private bool IsInRange( float fromDistance, float toDisRance )
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance >= fromDistance && distance <= toDisRance )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 弾の発射を許可する.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
	
}
