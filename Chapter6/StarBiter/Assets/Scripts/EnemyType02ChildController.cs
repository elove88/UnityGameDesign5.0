using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType02ChildController
//  - 「M03 敵機モデル タイプ02」の動き(フォーメーションのリーダーを除くメンバー)を制御する.
//  - 使い方.
//    - 本スクリプトが付いたオブジェクトをリーダーの子オブジェクトとして配置する.
//  - 動き仕様.
//    - リーダーに追従する.
//    - リーダーが破壊された場合、回避行動をとる.
// ----------------------------------------------------------------------------
public class EnemyType02ChildController : MonoBehaviour {

	public float speed = 4f;							// 敵機のスピード.
	public float turnSpeed = 1f;						// 敵機の方向転換のスピード.
	public float secondSpeed = 6f;						// 敵機の速いスピード.
	
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
	
		// リーダーが破壊されると、ステータスはATTACKになる.
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
			
		}
	}
	
	// ------------------------------------------------------------------------
	// スピードアップ.
	// ------------------------------------------------------------------------
	public void SpeedUp()
	{
		speed = secondSpeed;
	}
}
