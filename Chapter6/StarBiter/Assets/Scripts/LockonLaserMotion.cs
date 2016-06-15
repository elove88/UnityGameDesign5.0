using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ロックオンレーザーの動きの制御.
// ----------------------------------------------------------------------------
public class LockonLaserMotion : MonoBehaviour {
	
	public float laserSpeed = 20f;					// ロックオンレーザーのスピード.
	public float turnRate = 5f;						// ロックオンレーザーの回転の割合.
	public float turnRateAcceleration = 18.0f;		// ロックオンレーザーの回転の割合の増加量.
	public GameObject targetEnemy;					// ターゲットの敵機.
	public float power = 3;							// 攻撃力.
	
	private int lockonBonus = 1;					// ロックオンレーザー発射時のロックオンボーナス.
	
	private int targetId;							// ターゲットのインスタンスID.
	private	GameObject scoutingLaser;				// 索敵レーザー.
	private AudioMaker audioMaker;					// ロックオンレーザー音メーカー.
	
	private bool isStart = false;					// ロックオンレーザー射出.
	private bool isClear = false;					// ロックオンレーザー消去.
	
	void Start () 
	{
		// 索敵レーザーのインスタンスを取得.
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// ロックオンレーザー音メーカーのインスタンスを取得.
		audioMaker = GameObject.FindGameObjectWithTag("LockonLaserAudioMaker").GetComponent<AudioMaker>();
		
		// 音の再生.
		if ( audioMaker )
		{
			audioMaker.Play( this.gameObject );
		}
	}
	
	void Update () 
	{
		// ロックオンレーザー射出開始?.
		if ( isStart )
		{
			// ロックオンレーザーを前進させる.
			ForwardLaser();
		
			// ターゲットの消滅確認.
			IsDestroyTarget();
		}
	}
	
	// ------------------------------------------------------------------------
	// ロックオンレーザーを前進させる.
	// ------------------------------------------------------------------------
	private void ForwardLaser()
	{
		// 敵機がある時のみ処理.
		if ( targetEnemy )
		{
			// 敵機の方向を取得.
			Vector3 enemyPosition = targetEnemy.gameObject.transform.position;
			Vector3 relativePosition = enemyPosition - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
			
			// ロックオンレーザーの現在の方向から敵機の方向へ、指定した割合で回転した後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
			
			// ターンの割合を徐々に大きくする(レーザーが敵機にたどり着かずにループする状態の抑制).
			turnRate += turnRateAcceleration * Time.deltaTime;
			
			// レーザーの角度を変更.
			transform.rotation = tiltedRotation;

			// レーザーを進める.
			transform.Translate ( new Vector3( 0f, 0f, laserSpeed * Time.deltaTime ) );
		
		}
	}
	
	// ------------------------------------------------------------------------
	// ターゲットである敵機をセット.
	// ------------------------------------------------------------------------
	private void SetTargetEnemy( GameObject targetEnemy )
	{
		this.targetEnemy = targetEnemy;
		this.targetId = targetEnemy.GetInstanceID();
		
		isStart = true;	// ロックオンレーザー射出.
	}

	// ------------------------------------------------------------------------
	// ロックオンレーザー発射時のロックオンボーナスをセット.
	// ------------------------------------------------------------------------
	private void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	// ------------------------------------------------------------------------
	// 敵機への当たり判定.
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		
		// 敵機に当たった時は敵機の破壊を指示.
		int colliderId = collider.gameObject.GetInstanceID();
		
		if ( colliderId == targetId )
		{			
			// 敵機へ破壊の指示.
			isClear = true;
			collider.gameObject.SendMessage( "SetLockonBonus", lockonBonus );
			collider.gameObject.SendMessage( "SetIsBreakByLaser", power );
		}
	}
	
	// ------------------------------------------------------------------------
	// 敵機が破壊された後の処理.
	// ------------------------------------------------------------------------
	private void IsDestroyTarget()
	{
		
		if ( !targetEnemy || isClear )
		{
			// ロックオンレーザーの消去.
			Destroy( this.gameObject );
			
			// ロックオン数の減少.
			scoutingLaser.SendMessage( "DecreaseLockonCount", targetId );
		}
		
	}

}
