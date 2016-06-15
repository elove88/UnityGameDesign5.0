using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyType04Controller
//  - 「敵機モデル タイプ04」の動きを制御する.
//  - 使い方.
//    - 本コントローラーが付いたオブジェクトは、EnemyMakerより一定間隔で作成される.
//  - 動き仕様(仮).
//    - プレイヤーの進行方向の左上又は右上より現れる.
//    - プレイヤーに回り込む.
//    - プレイヤーと同じスピードで一定時間張り付く.
//    - 画面外へ.
// ----------------------------------------------------------------------------
public class EnemyType04Controller : MonoBehaviour {

	public float speed = 10f;							// 敵機のスピード.
	public float turnSpeed = 1.6f;						// 敵機の方向転換のスピード.
	public float followingSpeed = 10f;					// 監視中のスピード.
	public float uTurnSpeed = 20f;						// 戻る時のスピード.
	public float distanceFromPlayer = 10f;				// プレイヤーと一定の距離を保つ.
	public float followingTime = 5f;					// 監視時間.
	
	private GameObject player;							// プレイヤー.
	private float distanceToPlayer = 10.0f;				// 出現位置からプレイヤーまでの距離.
	private bool isUTurn = false;
	
	private GameObject subScreenMessage;				// SubScreenのメッセージ領域.
	
	void Start () {
	
		// プレイヤーを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// --------------------------------------------------------------------
		// 出現位置を指定.
		// --------------------------------------------------------------------
		
		// まずプレイヤーの位置に.
		transform.position = player.transform.position;
		float tmpAngle = Random.Range( 0f, 360f );
		transform.rotation = Quaternion.Euler( 0, tmpAngle, 0 );
		
		// 位置を調整.
		transform.Translate ( new Vector3( 0f, 0f, distanceToPlayer ) );
		
		// ----------------------------------------------------------------
		// 敵機の進行方向を決める.
		// ----------------------------------------------------------------
		
		// プレイヤーがある時のみ処理.
		if ( player )
		{
			// プレイヤーの方向を取得.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			Quaternion tiltedRotation = Quaternion.Euler( 0, targetRotation.eulerAngles.y + 30f, 0 );
			
			// 敵機の角度を変更.
			transform.rotation = tiltedRotation;
		}
		
		// 敵機を動かす.
		this.GetComponent<EnemyStatus>().SetIsAttack( true );
		
	}
	
	void Update () {
	
		bool isAttack = this.GetComponent<EnemyStatus>().GetIsAttack();
		if ( isAttack )
		{
			if ( !isUTurn )
			{
				// 敵機を進める.
				Forward();
			
				// プレイヤーに近づいたか.
				IsNear();
			}
			else
			{
				// 逃げる.
				Backward();
			}	
		}
	}
	
	// ------------------------------------------------------------------------
	// 敵機を前進させる.
	// ------------------------------------------------------------------------
	private void Forward()
	{
		// プレイヤーがある時のみ処理.
		if ( player )
		{
			// プレイヤーの方向を取得.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 敵機の現在の方向からプレイヤーの方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y;
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
	
	private void IsNear()
	{
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		
		if ( distance < distanceFromPlayer )
		{
			// スピードを落とす(プレイヤーとの距離を一定に保つ).
			speed = followingSpeed;
			
			// 一定時間後に回避行動をとる為のタイマーセット.
			StartCoroutine("SetTimer");
		}
	}
	
	IEnumerator SetTimer()
	{
		// 一定時間待つ.
		yield return new WaitForSeconds( followingTime );
		
		// 回避行動開始.
		isUTurn = true;
	}
	
	private void Backward()	
	{
		// プレイヤーがある時のみ処理.
		if ( player )
		{
			// プレイヤーの方向を取得.
			Vector3 playerPosition = player.transform.position;
			Vector3 relativePosition = playerPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// 敵機の現在の方向からプレイヤーと逆の方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y * - 1;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnSpeed * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// 敵機の角度を変更.
			transform.rotation = tiltedRotation;
		
			// 敵機を進める.
			transform.Translate ( new Vector3( 0f, 0f, uTurnSpeed * Time.deltaTime ) );
		}
	}
	
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if ( subScreenMessage != null )
			{
				if (
					this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
					this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "DESTROYED PATROL SHIP." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
				else
				{
					subScreenMessage.SendMessage("SetMessage", " ");
					subScreenMessage.SendMessage("SetMessage", "LOST PATROL SHIP..." );
					subScreenMessage.SendMessage("SetMessage", " ");
				}
			}
		}
	}

}
