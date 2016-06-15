using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType03ChildController
//  - 「M04 敵機モデル タイプ03」の動き(フォーメーションのリーダーを除くメンバー)を制御する.
//  - 使い方.
//    - 本スクリプトが付いたオブジェクトをリーダーの子オブジェクトとして配置する.
//  - 動き仕様.
//    - リーダーに追従する.
//    - リーダーが破壊された場合、回避行動をとる.
// ----------------------------------------------------------------------------
public class EnemyType03ChildController : MonoBehaviour {

	public float speed = 6f;							// 敵機のスピード.
	public float speedUTurn = 6f;						// Uターン中のスピード.
	public float turnSpeed = 5f;						// 敵機の方向転換のスピード.
	
	private bool canShoot = false;						// 弾発射条件(true: 発射可).
	
	private GameObject player;							// プレイヤー.
	private EnemyStatus enemyStatus;					// 敵機の状況.
	
	private float distanceToUTurnPoint = 5.0f;			// Uターンするプレイヤーまでの距離.
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
		
		// ----------------------------------------------------------------
		// 敵機の進行方向を決める.
		// ----------------------------------------------------------------
		
		// プレイヤーの方向を取得.
		Vector3 playerPosition = player.transform.position;
		Vector3 relativePosition = playerPosition - transform.position;
		Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
		
		// 敵機の角度を変更.
		transform.rotation = targetRotation;

		// リーダーに追従する.
		enemyStatus.SetIsFollowingLeader( true );

	}
	
	void Update () {
	
		if ( enemyStatus.GetIsFollowingLeader() )
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
