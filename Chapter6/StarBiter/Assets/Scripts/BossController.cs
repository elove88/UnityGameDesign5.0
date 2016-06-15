using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// BossController
//  - 「BOSS」の動きを制御する.
//  - 使い方.
//    - BOSSに付ける.
//  - 動き仕様.
//    - 画面上部より登場.
//    - 回避->攻撃前移動->攻撃の順で動く(繰り返し)
// ----------------------------------------------------------------------------
public class BossController : MonoBehaviour {
	
	public float startSpeed = 5f;						// BOSSの登場時のスピード.
	public float turnRate = 5f;							// BOSSの間合いを取っている時の方向転換の割合.
	public float escapeRate = 5f;						// BOSSが左右に回避する際の現在位置から移動先へ動く時の割合.
	public float jumpSpeed = 30f;						// BOSSが画面外へ逃げるスピード.
	public float escapeSpeed = 5f;						// BOSSが左右に回避する際の現在位置から移動先へ動く時のスピード.
	public float escapeTime = 5f;						// 回避している時間.
	public float waitTimeBeforeAttack = 3f;				// 攻撃前の待ち時間.
	public float waitTimeJustBeforeAttack = 0.7f;		// 攻撃直前の待ち時間.
	public float escapeSpeedJustBeforeAttack = 4f;		// 攻撃直前に距離を取る際のスピード.
	public float Attack1Time = 7f;						// 攻撃1の時間.
	public float Attack2Time = 4f;						// 攻撃2の時間.
	public float Attack3Time = 3f;						// 攻撃3の時間.
	
	private GameObject player;							// プレイヤー.
	private PlayerStatus playerStatus;					// プレイヤーステータスのインスタンス.
	private BattleSpaceController battleSpaceContoller;	// 戦闘空間.
	private float startPositionZ = -12.0f;				// 出現位置(プレイヤー(Z=0)からの距離).
	private float distanceToPlayer = 7.0f;				// プレイヤーとの距離.
	private float distanceToPlayerJustBeforeAttack = 8.0f;	// 攻撃直前のプレイヤーとの距離.
	private float distanceToPlayerMove1 = 4.0f;			// プレイヤーとの距離.
	private float distanceToPlayerMove2 = 5.0f;			// プレイヤーとの距離.
	private float distanceToPlayerMove3 = 4.0f;			// プレイヤーとの距離.
	
	private BulletMaker bulletMakerLeft;
	private BulletMaker bulletMakerRight;
	private LaserMaker laserMakerLeft;
	private LaserMaker laserMakerRight;
	private ShotMaker shotMaker;
	private EnemyStatusBoss enemyStatusBoss;
	private Animation bossAnimation;
	
	private Vector3 destinationPosition = Vector3.zero;	// 左右へ回避する先.
	private bool isEscape = false;						// 左右へ回避中.
	
	private enum State
	{
		START,				// BOSS戦闘開始.
		TOPLAYER,			// プレイヤーへ向かう.
		ESCAPE1START,		// 回避開始.
		ESCAPE1END,			// 回避中～終了.
		ESCAPE2START,
		ESCAPE2END,
		ESCAPE3START,
		ESCAPE3END,
		MOVE1START,			// 攻撃位置へ移動開始.
		MOVE1END,			// 移動中～終了.
		MOVE2START,
		MOVE2END,
		MOVE3START,
		MOVE3END,
		JUSTBEFORE1START,	// 攻撃直前動作の開始.
		JUSTBEFORE1END,		// 攻撃直前動作中～終了.
		JUSTBEFORE2START,
		JUSTBEFORE2END,
		JUSTBEFORE3START,
		JUSTBEFORE3END,
		ATTACK1START,		// 攻撃開始.
		ATTACK1END,			// 攻撃中～終了.
		ATTACK2START,
		ATTACK2END,
		ATTACK3START,
		ATTACK3END
	}
	private State state = State.TOPLAYER;
	
	void Start () {
	
		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// プレイヤーステータスのインスタンスを取得.
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// BOSSの各パーツを取得.
		bulletMakerLeft = GameObject.Find("BossVulcanLeft").GetComponent<BulletMaker>();
		bulletMakerRight = GameObject.Find("BossVulcanRight").GetComponent<BulletMaker>();
		laserMakerLeft = GameObject.Find("BossLaserLeft").GetComponent<LaserMaker>();
		laserMakerRight = GameObject.Find("BossLaserRight").GetComponent<LaserMaker>();
		shotMaker = GameObject.Find("BossCore").GetComponent<ShotMaker>();
		enemyStatusBoss = GetComponent<EnemyStatusBoss>();
		
		// アニメーションを取得.
		bossAnimation = GetComponent<Animation>();
		
		// 戦闘空間を取得.
		battleSpaceContoller = GameObject.FindGameObjectWithTag("BattleSpace").GetComponent<BattleSpaceController>();
		
		// --------------------------------------------------------------------
		// 出現位置を指定.
		// --------------------------------------------------------------------
		
		// まずプレイヤーの位置に.
		transform.position = player.transform.position;
		transform.rotation = Quaternion.Euler( 0, 180, 0 );
		
		// 位置を調整.
		transform.Translate ( new Vector3( 0f, 0f, startPositionZ ) );
		
	}
	
	void Update () {
		
		// プレイヤーとの距離を確認.
		float distance = Vector3.Distance(
			player.transform.position,
			transform.position );
		if ( state == State.TOPLAYER && distance < distanceToPlayer )
		{
			state = State.ESCAPE1START;
			destinationPosition = transform.position;
		}
		
		// 画面上部より登場する処理.
		if ( state == State.TOPLAYER )
		{
			transform.position += transform.forward * startSpeed * Time.deltaTime;
		}
		
		// プレイヤーが操作可能な時のみ処理する.
		if ( playerStatus.GetIsNOWPLAYING() ) {
		
			// 攻撃1.
			Attack1();
		
			// 攻撃2.
			Attack2();
		
			// 攻撃3.
			Attack3();
			
			// 攻撃中の処理.
			Attacking();
		
			// 攻撃中にプレイヤーとの間合いを取る.
			SetDistanceToPlayerAtAttack();
			
			// 回避1.
			Escape1();

			// 回避2.
			Escape2();
			
			// 回避3.
			Escape3();
	
			// 攻撃位置へ移動1.
			Move1();

			// 攻撃位置へ移動2.
			Move2();

			// 攻撃位置へ移動3.
			Move3();
			
			// 攻撃直前動作1.
			MotionJustBeforeAttack1();

			// 攻撃直前動作2.
			MotionJustBeforeAttack2();

			// 攻撃直前動作3.
			MotionJustBeforeAttack3();
			
			// 攻撃直前にプレイヤーとの間合いを取る.
			SetDistanceToPlayerJustBefortAttack();
			
			// プレイヤーとの間合いを取る.
			SetDistanceToPlayer();
			
			// 回避行動.
			EscapeFromPlayer();
		
		}
	
		// 戦闘空間のスクロール方向を加える.
		transform.position -= battleSpaceContoller.GetAdditionPos();
		
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻撃1.
	// ------------------------------------------------------------------------
	private void Attack1()
	{
		// 攻撃1.
		if ( state == State.ATTACK1START )
		{
			// ステータスを進める.
			state = State.ATTACK1END;
			
			// 機体が存在している時のみ攻撃可能.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				// アニメーション再生.
				bossAnimation.Play();
					
				// バルカン攻撃.
				if ( bulletMakerLeft ) bulletMakerLeft.SetIsFiring();
				if ( bulletMakerRight ) bulletMakerRight.SetIsFiring();					
				
				StartCoroutine( WaitAndUpdateState( Attack1Time, State.ESCAPE2START ) );
			}
			else
			{
				// 機体がない場合は次の攻撃に移る.
				state = State.ATTACK2START;
			}
			
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻撃2.
	// ------------------------------------------------------------------------
	private void Attack2()
	{
		// 攻撃2.
		if ( state == State.ATTACK2START )
		{
			// ステータスを進める.
			state = State.ATTACK2END;
			
			// 機体が存在している時のみ攻撃可能.
			if ( laserMakerLeft || laserMakerRight )
			{
				// レーザー攻撃.
				if ( laserMakerLeft ) laserMakerLeft.SetIsFiring();
				if ( laserMakerRight ) laserMakerRight.SetIsFiring();
				
				StartCoroutine( WaitAndUpdateState( Attack2Time, State.ESCAPE3START ) );
			}
			else
			{
				// 機体がない場合は次の攻撃に移る.
				state = State.ATTACK3START;
			}
		}
	}

	// ------------------------------------------------------------------------
	// BOSS - 攻撃3.
	// ------------------------------------------------------------------------
	private void Attack3()
	{
		// 攻撃3.
		if ( state == State.ATTACK3START )
		{
			state = State.ATTACK3END;

			// ショット攻撃.
			if ( shotMaker ) shotMaker.SetIsFiring();
					
			StartCoroutine( WaitAndUpdateState( Attack3Time, State.ESCAPE1START ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻撃中の処理.
	// ------------------------------------------------------------------------
	private void Attacking()
	{
		// 攻撃終了直後.
		if ( state == State.ATTACK1END ||
			 state == State.ATTACK2END ||
			 state == State.ATTACK3END )
		{
			// プレイヤーの方向を取得.
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// BOSSの現在の方向から行き先の方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// BOSSの方向をプレイヤーに向ける.
			transform.rotation = tiltedRotation;
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避1.
	// ------------------------------------------------------------------------
	private void Escape1()
	{
		// 回避1.
		if ( state == State.ESCAPE1START )
		{
			state = State.ESCAPE1END;
			
			// 機体が存在している時のみ回避1をする.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE1START ) );
			}
			else
			{
				// 機体がない場合は次の回避に移る.
				state = State.ESCAPE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避2.
	// ------------------------------------------------------------------------
	private void Escape2()
	{
		// 回避2.
		if ( state == State.ESCAPE2START )
		{
			state = State.ESCAPE2END;
			
			// 機体が存在している時のみ回避2をする.
			if ( laserMakerLeft || laserMakerRight )
			{
				SetEscapeTime();
				StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE2START ) );
			}
			else
			{
				// 機体がない場合は次の回避に移る.
				state = State.ESCAPE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 回避3.
	// ------------------------------------------------------------------------
	private void Escape3()
	{
		// 回避3.
		if ( state == State.ESCAPE3START )
		{
			state = State.ESCAPE3END;
			SetEscapeTime();
			StartCoroutine( WaitAndUpdateState( escapeTime, State.MOVE3START ) );			
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃位置へ移動1.
	// ------------------------------------------------------------------------
	private void Move1()
	{
		// 移動1.
		if ( state == State.MOVE1START )
		{
			state = State.MOVE1END;
			
			// 機体が存在している時のみ移動1をする.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE1START ) );
			}
			else
			{
				// 機体がない場合は次の移動に移る.
				state = State.MOVE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃位置へ移動2.
	// ------------------------------------------------------------------------
	private void Move2()
	{
		// 移動2.
		if ( state == State.MOVE2START )
		{
			state = State.MOVE2END;
			
			// 機体が存在している時のみ移動2をする.
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE2START ) );
			}
			else
			{
				// 機体がない場合は次の移動に移る.
				state = State.MOVE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃位置へ移動3.
	// ------------------------------------------------------------------------
	private void Move3()
	{
		// 移動3.
		if ( state == State.MOVE3START )
		{
			state = State.MOVE3END;
			isEscape = false;
			StartCoroutine( WaitAndUpdateState( waitTimeBeforeAttack, State.JUSTBEFORE3START ) );			
		}
	}
	
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃直前動作1.
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack1()
	{
		// 攻撃直前動作1.
		if ( state == State.JUSTBEFORE1START )
		{
			state = State.JUSTBEFORE1END;
			
			// 機体が存在している時のみ攻撃直前動作1をする.
			if ( bulletMakerLeft || bulletMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK1START ) );
			}
			else
			{
				// 機体がない場合は次の攻撃直前動作に移る.
				state = State.JUSTBEFORE2START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃直前動作2.
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack2()
	{
		// 攻撃直前動作2.
		if ( state == State.JUSTBEFORE2START )
		{
			state = State.JUSTBEFORE2END;
			
			// 機体が存在している時のみ攻撃直前動作2をする.
			if ( laserMakerLeft || laserMakerRight )
			{
				isEscape = false;
				StartCoroutine( WaitAndUpdateState( waitTimeJustBeforeAttack, State.ATTACK2START ) );
			}
			else
			{
				// 機体がない場合は次の攻撃直前動作に移る.
				state = State.JUSTBEFORE3START;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// BOSS - 攻撃直前動作3.
	// ------------------------------------------------------------------------
	private void MotionJustBeforeAttack3()
	{
		// 攻撃直前動作3.
		if ( state == State.JUSTBEFORE3START )
		{
			isEscape = false;
			state = State.ATTACK3START;
		}
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーとの間合いを取る.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayer()
	{
		// プレイヤーとの間合いを取る.
		if (
			state == State.MOVE1END ||
			state == State.MOVE2END ||
			state == State.MOVE3END )
		{
			// プレイヤーの方向を取得.
			Vector3 relativePosition = player.transform.position - transform.position;
			Quaternion targetRotation = Quaternion.LookRotation( relativePosition );

			// BOSSの現在の方向から行き先の方向へ、指定したスピードで傾けた後の角度を取得.
			float targetRotationAngle = targetRotation.eulerAngles.y;
			float currentRotationAngle = transform.eulerAngles.y;
			currentRotationAngle = Mathf.LerpAngle(
				currentRotationAngle,
				targetRotationAngle,
				turnRate * Time.deltaTime );
			Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
				
			// BOSSの方向をプレイヤーに向ける.
			transform.rotation = tiltedRotation;
			
			// プレイヤーとの間合いを取る.
			float tmpDistanceToPlayer = 0;
			if ( state == State.MOVE1END ) { tmpDistanceToPlayer = distanceToPlayerMove1; }
			if ( state == State.MOVE2END ) { tmpDistanceToPlayer = distanceToPlayerMove2; }
			if ( state == State.MOVE3END ) { tmpDistanceToPlayer = distanceToPlayerMove3; }
			float distance5 = Vector3.Distance(
				player.transform.position,
				transform.position );

			if ( distance5 < tmpDistanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( tmpDistanceToPlayer - distance5 ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance5 - tmpDistanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻撃直前にプレイヤーとの距離を取る.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerJustBefortAttack()
	{
		// 攻撃中にプレイヤーとの間合いを取る.
		if (
			state == State.JUSTBEFORE1END ||
			state == State.JUSTBEFORE2END ||
			state == State.JUSTBEFORE3END )
		{
			
			// プレイヤーとBOSSの距離を取得.
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSSをプレイヤーに向ける.
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;

			// プレイヤーとの間合いを取る.
			if ( distance < distanceToPlayerJustBeforeAttack )
			{
				transform.position -=
					transform.forward
						* Time.deltaTime
						* ( distanceToPlayerJustBeforeAttack - distance )
						* escapeSpeedJustBeforeAttack;
			}
			else
			{
				transform.position +=
					transform.forward
						* Time.deltaTime
						* ( distance - distanceToPlayerJustBeforeAttack )
						* escapeSpeedJustBeforeAttack;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 攻撃中にプレイヤーとの間合いを取る.
	// ------------------------------------------------------------------------
	private void SetDistanceToPlayerAtAttack()
	{
		// 攻撃中にプレイヤーとの間合いを取る.
		if (
			state == State.ATTACK1END ||
			state == State.ATTACK3END )
		{
			
			// プレイヤーとBOSSの距離を取得.
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSSをプレイヤーに向ける.
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// プレイヤーとの間合いを取る.
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 回避行動.
	// ------------------------------------------------------------------------
	private void EscapeFromPlayer()
	{
		// ロックオンレーザーに当たらないように動く.
		if (
			state == State.ESCAPE1START ||
			state == State.ESCAPE1END ||
			state == State.ESCAPE2START ||
			state == State.ESCAPE2END ||
			state == State.ESCAPE3START ||
			state == State.ESCAPE3END )
		{
			// BOSSが回避中かどうか.
			if ( !isEscape )
			{
				// プレイヤーとの間合いを確認する.
				GetDestinationPosition();
			}
			else
			{
				// BOSSから見た移動先への角度を取得.
				Vector3 destinationRelativePositionByBoss =
					destinationPosition - transform.position;
				Quaternion destinationTargetRotationByBoss =
					Quaternion.LookRotation( destinationRelativePositionByBoss );

				// BOSSの進行方向を移動先に向ける.
				transform.rotation = destinationTargetRotationByBoss;

				// BOSSを進行方向に進める.
				float distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				transform.Translate ( new Vector3( 0f, 0f, distanceToDestination * escapeSpeed * Time.deltaTime ) );

				// 移動後の位置から移動先への距離を求める.
				distanceToDestination = Vector3.Distance(
					destinationPosition,
					transform.position );
				
				// 移動先に到着した時、回避行動を終了する.
				if ( distanceToDestination < 1f )
				{
					isEscape = false;
				}
				
				// BOSSをプレイヤーの方向に向ける.
				Vector3 relativePosition = player.transform.position - transform.position;
				Quaternion targetRotation = Quaternion.LookRotation( relativePosition );
				transform.rotation = targetRotation;
			}
		}
	}

	// ------------------------------------------------------------------------
	// 回避時の移動先を取得.
	// ------------------------------------------------------------------------
	public void GetDestinationPosition()
	{
		// プレイヤーの角度.
		float playerAngle = player.transform.eulerAngles.y;
		
		// プレイヤーから見たBOSSの角度.
		Vector3 bossRelativePositionByPlayer = transform.position - player.transform.position;
		float bossAngleByPlayer = Quaternion.LookRotation( bossRelativePositionByPlayer ).eulerAngles.y;

		// 角度補正.
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 180f )
		{
			// 0度 <-> 359度 を跨いだと見なし、+360度する.
			if ( playerAngle < 180f )
			{
				playerAngle += 360f;
			}
			if ( bossAngleByPlayer < 180f )
			{
				bossAngleByPlayer += 360f;
			}
		}
		
		// プレイヤーの進行方向の角度から一定以上離れている場合は回避しない.
		if ( Mathf.Abs ( playerAngle - bossAngleByPlayer ) > 45f )
		{
			// ----------------------------------------------------------------
			// プレイヤーとの間合いを取る.
			// ----------------------------------------------------------------
			
			// プレイヤーとBOSSの距離を取得.
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			// BOSSをプレイヤーに向ける.
			Vector3 playerRelativePositionByBoss = player.transform.position - transform.position;
			Quaternion playerTargetRotationByBoss = Quaternion.LookRotation( playerRelativePositionByBoss );
			transform.rotation = playerTargetRotationByBoss;
	
			// プレイヤーとの間合いを取る.
			if ( distance < distanceToPlayer )
			{
				transform.position -= transform.forward * Time.deltaTime * ( distanceToPlayer - distance ) * 2;
			}
			else
			{
				transform.position += transform.forward * Time.deltaTime * ( distance - distanceToPlayer ) * 2;
			}

			return;
		}
		
		// ----------------------------------------------------------------
		// 回避先を求める.
		// ----------------------------------------------------------------
		
		// プレイヤーを中心としてBOSSの回避先の角度を求める.
		float transformAngle = Random.Range( -45f, 45f );
		if ( transformAngle > 0 )
		{
			transformAngle += 50f;
		}
		else
		{
			transformAngle -= 50f;
		}
		float targetRotationAngle = bossAngleByPlayer;
		targetRotationAngle += transformAngle;
		
		// 回避先の位置を設定.
		destinationPosition =
			Quaternion.AngleAxis( targetRotationAngle, Vector3.up )
				* Vector3.forward * distanceToPlayer;
		
		// 回避開始.
		isEscape = true;
		
	}
	
	// ------------------------------------------------------------------------
	// BOSSの状態を更新.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		// 待ち.
		yield return new WaitForSeconds( waitForSeconds );
		
		// ステート更新.
		this.state = state;
	}
	
	// ------------------------------------------------------------------------
	// life残量に合わせて逃げている時間を短くする.
	// ------------------------------------------------------------------------
	private void SetEscapeTime()
	{
		int life = enemyStatusBoss.GetLife();
		if ( life > 5 )
		{
			return;
		}
		switch ( life )
		{
		    case 1:
		        escapeTime = 0.5f;
				waitTimeBeforeAttack = 0.5f;
		        break;
		    case 2:
			case 3: 
		        escapeTime = 1f;
				waitTimeBeforeAttack = 1f;
		        break;
		    case 4:
			case 5: 
		        escapeTime = 2f;
				waitTimeBeforeAttack = 2f;
		        break;
		}
	}
}