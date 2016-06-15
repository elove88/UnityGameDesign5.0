using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType01Controller
//  - 「M02 敵機モデル タイプ01」の動き(単独orフォーメーションのリーダー)を制御する.
//  - 使い方.
//    - 本スクリプトをオブジェクトに付ける.
//  - 動き仕様.
//    - プレイヤーの進行方向の左上又は右上より現れる.
//    - プレイヤーの中心に向かっていく.
//    - フォーメーションの場合.
//      - リーダーが破壊された場合はその他のメンバーのステータスをATTACKにする.
// ----------------------------------------------------------------------------
public class EnemyType01Controller : MonoBehaviour {
	
	public float speed = 2.7f;							// 敵機の進むスピード.
	public float turnSpeed = 1f;						// 旋回のスピード.
	
	public float startDistanceToShoot = 5f;				// 弾を撃つ範囲の開始距離.
	public float endDistanceToShoot = 8f;				// 弾を撃つ範囲の終了距離.
	
	private bool canShoot = false;						// 弾発射条件(true: 発射可).
	
	private GameObject player;							// プレイヤー.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	private EnemyStatus enemyStatus;					// 敵機の状況.
	
	private float distanceFromPlayerAtStart = 9.5f;		// 開始時のプレイヤーからの距離.
	
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
			// プレイヤーの方向を取得.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				
			// 敵機の現在の方向からプレイヤーの方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle;
			targetRotationAngle = targetRotation.eulerAngles.y;
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
