using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType03Controller
//  - 「M04 敵機モデル タイプ03」の動き(単独)を制御する.
//  - 使い方.
//    - 本スクリプトをオブジェクトに付ける.
//  - 動き仕様.
//    - プレイヤーの進行方向と反対側の方向より現れる.
//    - プレイヤーへ一定距離まで近づく.
//    - 弾を発射後、画面外へ逃げる.
// ----------------------------------------------------------------------------
public class EnemyType03Controller : MonoBehaviour {

	public float speed = 6f;							// 敵機のスピード.
	public float speedUTurn = 6f;						// Uターン中のスピード.
	public float turnSpeed = 5f;						// 敵機の方向転換のスピード.
	
	private bool canShoot = false;						// 弾発射条件(true: 発射可).
	
	private GameObject player;							// プレイヤー.
	private EnemyStatus enemyStatus;					// 敵機の状況.
	
	private float distanceToUTurnPoint = 5.0f;			// Uターンするプレイヤーまでの距離.
	private float distanceFromPlayerAtStart = 9.5f;		// 開始時のプレイヤーからの距離.
	private enum State
	{
		FORWARD,	// 進む.
		STAY,		// 停止.
		UTURN		// Uターン.
	}
	private State state = State.FORWARD;				// 敵機の行動状況.
	
	void Start () {
	
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 敵機の状況のインスタンスを取得.
		enemyStatus = this.GetComponent<EnemyStatus>();
		
		// --------------------------------------------------------------------
		// 出現位置を指定.
		// --------------------------------------------------------------------

		// 発生方向を計算.(プレイヤーの進行方向と反対側).
		float playerAngleY = player.transform.rotation.eulerAngles.y + 180;
		float additionalAngle = (float)Random.Range( -90, 90 );
		
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
			if ( state == State.UTURN )
			{
				// ----------------------------------------------------------------
				// 敵機の進行方向を決める.
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
				transform.Translate ( new Vector3( 0f, 0f, speedUTurn * Time.deltaTime ) );
			}
			
			if ( state == State.FORWARD )
			{
				// 敵機を進める.
				transform.Translate ( new Vector3( 0f, 0f, speed * Time.deltaTime ) );
				
				// ある距離まで近づいた場合は一定時間留まる.
				float distance = Vector3.Distance(
					player.transform.position,
					transform.position );
				if ( distance < distanceToUTurnPoint )
				{
					state = State.STAY;
					StartCoroutine( WaitAndUTurn( 3f ) );
				}
			}
			
			if ( state == State.STAY )
			{
				// 停止中にrigidbodyによる衝突判定を有効にする為、動かして戻す.
				// ※Project Settings->Physics の Sleep Velocity より大きな値にする.
				transform.Translate ( new Vector3( 0f, 0f, 0.2f ) );
				transform.Translate ( new Vector3( 0f, 0f, -0.2f ) );
			}
		}
	}

	// ------------------------------------------------------------------------
	// Uターンまでの待ち処理.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUTurn( float waitForSeconds )
	{
		yield return new WaitForSeconds( waitForSeconds );
		state = State.UTURN;
		SetFire();
	}
	
	// ------------------------------------------------------------------------
	// 弾を発射する.
	// ------------------------------------------------------------------------
	private void SetFire()
	{
		if ( !canShoot ) return;
		
		bool isFiring = false;
		if ( this.GetComponent<ShotMaker>() )
		{
			this.GetComponent<ShotMaker>().GetIsFiring();
		}
		if ( !isFiring )
		{
			if ( this.GetComponent<ShotMaker>() )
			{
				this.GetComponent<ShotMaker>().SetIsFiring();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 弾の発射を許可する.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
}
