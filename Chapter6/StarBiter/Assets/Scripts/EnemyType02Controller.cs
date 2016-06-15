using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType02Controller
//  - 「M03 敵機モデル タイプ02」の動き(単独orフォーメーションのリーダー)を制御する.
//  - 使い方.
//    - 本スクリプトをオブジェクトに付ける.
//  - 動き仕様.
//    - プレイヤーの進行方向の左上又は右上より現れる.
//    - 直進する.
//    - フォーメーションの場合.
//      - リーダーが破壊された場合はその他のメンバーのステータスをATTACKにする.
// ----------------------------------------------------------------------------
public class EnemyType02Controller : MonoBehaviour {

	public float speed = 4f;							// 敵機のスピード.
	public float secondSpeed = 6f;						// 敵機の速いスピード.
	
	private bool canShoot = false;						// 弾発射条件(true: 発射可).
	
	private GameObject player;							// プレイヤー.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	private EnemyStatus enemyStatus;					// 敵機の状況.
	
	private float distanceFromPlayerAtStart = 10.5f;		// 開始時のプレイヤーからの距離.
	
	void Start () {
	
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");

		// 戦闘空間のインスタンスを取得.
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// 敵機の状況のインスタンスを取得.
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
		// 出現位置を指定.
		// --------------------------------------------------------------------
		
		// 発生方向を計算.(プレイヤーの角度からプラスマイナス45度).
		float playerAngleY = player.transform.rotation.eulerAngles.y;
		float additionalAngle = (float)Random.Range( -45, 45 );
		
		// 方向を設定.
		transform.rotation = Quaternion.Euler( 0f, playerAngleY + additionalAngle, 0f );
		
		// 位置を設定.
		transform.position = new Vector3( 0, 0, 0 );
		transform.position = transform.forward * distanceFromPlayerAtStart;
		
		// 進行方向をプレイヤーに向ける.
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		transform.rotation = targetRotation;
		
		// 敵機を動かす.
		enemyStatus.SetIsAttack( true );
	}
	
	void Update () {
	
		if ( enemyStatus.GetIsAttack() )
		{
			// 弾を撃つ.
			Shoot();
			
			// 敵機を進める.
			transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
			
			// 戦闘空間のスクロール方向を加える.
			transform.position -= battleSpaceContoller.GetAdditionPos();
		}
	}
	
	// ------------------------------------------------------------------------
	// 弾を撃つ.
	// ------------------------------------------------------------------------
	private void Shoot()
	{
		if ( !canShoot ) { return; }
		
		//
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		
		//
		if ( !isFiring )
		{
			if ( enemyStatus.GetIsPlayerBackArea() )
			{
				if ( this.GetComponent<ShotMaker>() )
				{
					this.GetComponent<ShotMaker>().SetIsFiring();
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// スピードアップ.
	// ------------------------------------------------------------------------
	public void SpeedUp()
	{
		speed = secondSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 弾の発射を許可する.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}

}
