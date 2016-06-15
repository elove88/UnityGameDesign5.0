using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 敵の状態を管理する.
// ----------------------------------------------------------------------------
public class EnemyStatus : MonoBehaviour {
	
	public float breakingDistance = 20f;				// 敵機消滅条件(プレイヤーと敵機の距離).
	public GameObject effectBomb;
	public GameObject effectDamage;
	public int life = 1;								// ライフ.
	public bool isEnbleByCollisionStone = false;		// 岩石への衝突は有効(=true).
	public int score = 0;								// 点数.
	public string enemyTypeId = "";						// 敵機の(*)-TYPE.
	
	private enum State
	{
		INITIALIZE,				// 初期化.
		FOLLOWINGLEADER,		// リーダーに従う(BOSSの場合はcoreに従う).
		ATTACK,					// 攻撃.
		BREAK,					// 破壊.
		DESTROY					// 消滅.
	}
	
	private State enemyState = State.INITIALIZE;		// 敵機の状態.
	
	protected int lockonBonus = 1;						// ロックオンボーナス.
	
	private GameObject player;							// プレイヤーのインスタンス.
	private GameObject enemyMaker;						// enemyMakerのインスタンス.
	private PrintScore printScore;						// printScoreのインスタンス.
	
	private bool isMadeInEnemyMaker = false;			// enemyMakerから作成された(=true).
	private bool isPlayerBackArea = false;				// プレイヤーの背後にいる(=true).
	private bool isBreakByPlayer = false;				// プレイヤーによって破壊された(=true).
	private bool isBreakByStone = false;				// 岩石によって破壊された(=true).
	
	private GameObject txtEnemyStatus;
	private GameObject subScreenMessage;				// SubScreenのメッセージ領域.
	
	private string enemyTypeString = "";				// 敵機のTYPE名.
	
	private Vector3 beforePosition;						// 爆発エフェクト用: 動く前の位置.
	private bool isMoving = false;						// 爆発エフェクト用: 動いているか?
	private float speed = 0f;							// 爆発エフェクト用: スピード.
	
	void Start () {

		// プレイヤーのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// printScoreのインスタンスを取得.
		printScore = GameObject.FindGameObjectWithTag("Score").GetComponent<PrintScore>();
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// 敵機のTYPE名をセット.
		enemyTypeString = SetEnemyType();
		
		// 追加の初期化処理.
		StartSub();
	}
	
	public virtual void StartSub()
	{
		// ここは派生クラス用.	
	}
	
	void LateUpdate()
	{
		// 敵機が動いているかどうか(爆発エフェクト用).
		if ( enemyState == State.INITIALIZE ||
			 enemyState == State.FOLLOWINGLEADER ||
			 enemyState == State.ATTACK )
		{
			if ( beforePosition != transform.position )
			{
				isMoving = true;
				speed = Vector3.Distance( beforePosition, transform.position );
			}
			else
			{
				isMoving = false;
				speed = 0f;
			}
			beforePosition = transform.position;
		}
	}
	
	void Update ()
	{
		// 敵機消滅判定.
		IsOverTheDistance();
		
		// 敵機の破壊確認.
		IsBreak();
		
		// 敵機の消滅.
		DestroyEnemy();
		
		// 追加の更新処理.
		UpdateSub();
	}
	
	public virtual void UpdateSub()
	{
		// ここは派生クラス用.	
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーからの距離が一定以上の場合は消去する.
	// ------------------------------------------------------------------------
	private void IsOverTheDistance()
	{
		if ( enemyState == State.ATTACK )
		{
			float distance = Vector3.Distance(
				player.transform.position,
				transform.position );
			
			if ( distance > breakingDistance )
			{
				enemyState = State.DESTROY;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 敵機の破壊確認.
	// ------------------------------------------------------------------------
	private void IsBreak()
	{
		if ( enemyState == State.BREAK )
		{			
			// 破壊アニメーション.
			if ( effectBomb )
			{
				// 角度を調整(rotateの値をそのまま使うとParticleでは意図しない方向に進んでしまう).
				Quaternion tmpRotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y / 2,0);
				// 爆発エフェクト作成.
				GameObject tmpGameObject = Instantiate( 
					effectBomb,
					transform.position,
					tmpRotation ) as GameObject;
				// エフェクトを敵機が動いていた方向に動かす.
				if ( isMoving )
				{
					tmpGameObject.SendMessage( "SetIsMoving", speed );
				}
			}
			
			// 敵機を停止.
			enemyState = State.DESTROY;
		}
	}
	
	
	protected virtual void DestroyEnemyEx()
	{
		// 敵機を消滅させる.
		Destroy( this.gameObject );
	}
	
	// ------------------------------------------------------------------------
	// 敵機の破壊.
	// ------------------------------------------------------------------------
	private void DestroyEnemy()
	{
		if ( enemyState == State.DESTROY )
		{
			// 破壊対象がフォーメーションのリーダーだった場合、フォーメーションを解除する(それぞれ単独行動させる).
			Transform[] children = this.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					if ( child.GetComponent<EnemyStatus>() )
					{
						// フォーメーションのリーダー以外のメンバー(敵機)に対し処理する.
						if ( child.GetComponent<EnemyStatus>().GetIsFollowingLeader() == true )
						{
							// 単独で行動を開始する.
							child.SendMessage( "SetIsAttack", true );
							child.transform.parent = null;
						}
					}
				}
			}
			
			// 敵機作成数から一つ減らす.
			if ( isMadeInEnemyMaker )
			{
				enemyMaker.SendMessage( "DecreaseEnemyCount", this.GetInstanceID() );
			}
			
			// 追加の破壊時の処理.
			DestroyEnemySub();
			
			// 敵機を消滅させる.
			Destroy( this.gameObject );
			
		}
	}
	
	public virtual void DestroyEnemySub()
	{
		// ここは派生クラス用.	
	}
	
	// ------------------------------------------------------------------------
	// 敵機を破壊状態にする(ダメージを親へ伝播).
	// ------------------------------------------------------------------------
	public void SetIsBreak( bool isBreak )
	{
		
		if ( isBreak )
		{
			if (
				enemyState == State.FOLLOWINGLEADER ||
				enemyState == State.ATTACK )
			{
				if ( life > 0 )
				{
					life--;
				}
				
				if ( life <= 0 )
				{
					// 敵機を破壊した.
					enemyState = State.BREAK;
					isBreakByPlayer = true;
					
					// 点数加算.
					printScore.SendMessage( "AddScore", score * lockonBonus );
					
					// サブスクリーンへメッセージ出力.
					subScreenMessage.SendMessage(
						"SetMessage",
						"DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus );
				}
				else
				{
					// ダメージアニメーション.
					if ( effectDamage )
					{
						Instantiate( 
							effectDamage,
							transform.position,
							transform.rotation );
					}
				}
				isBreak = false;
			}
		}
	}
	
	private void SetIsBreakEx( int damage, int lockonBonus )
	{
		if (
			enemyState == State.FOLLOWINGLEADER ||
			enemyState == State.ATTACK )
		{
			if ( life > 0 )
			{
				life -= damage;
			}
			
			if ( life <= 0 )
			{
				// 敵機を破壊した.
				enemyState = State.BREAK;
				isBreakByPlayer = true;
				
				// 点数加算.
				printScore.SendMessage( "AddScore", score * lockonBonus );
				
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage(
					"SetMessage",
					"DESTROYED " + enemyTypeString + " BY LOCK BONUS X " + lockonBonus );
			}
			else
			{
				// ダメージアニメーション.
				if ( effectDamage )
				{
						Instantiate(
							effectDamage,
							transform.position, 
							new Quaternion(0f, 0f, 0f, 0f) );
				}
			}
		}
	}
	
	public void SetLockonBonus( int lockonBonus )
	{
		this.lockonBonus = lockonBonus;
	}
	
	public void SetIsBreakByLaser( int damage )
	{
		SetIsBreakEx( damage, lockonBonus );
	}
	
	public void SetIsBreakByShot( int damage )
	{
		SetIsBreakEx( damage, 1 );
	}
	
	public void SetIsBreakEx2()
	{
		if ( enemyState != State.BREAK )
		{
			life = 0;
			enemyState = State.BREAK;
		}
	}
	
	// ------------------------------------------------------------------------
	// 敵機を攻撃状態にする.
	// ------------------------------------------------------------------------
	public void SetIsAttack( bool isAttack )
	{
		if ( isAttack )
		{
			if ( enemyState == State.INITIALIZE ||
				 enemyState == State.FOLLOWINGLEADER )
			{
				enemyState = State.ATTACK;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// (フォーメーション)リーダーに追従する.
	// ------------------------------------------------------------------------
	public void SetIsFollowingLeader( bool isFollowingLeader )
	{
		if ( isFollowingLeader )
		{
			if ( enemyState == State.INITIALIZE )
			{
				enemyState = State.FOLLOWINGLEADER;
			}
		}
	}
	
	public void SetEmenyMaker( GameObject enemyMaker )
	{
		this.enemyMaker = enemyMaker;
		isMadeInEnemyMaker = true;
	}
	public GameObject GetEmenyMaker()
	{
		return enemyMaker;
	}
	
	// ------------------------------------------------------------------------
	// 敵機が攻撃状態か確認する.
	// ------------------------------------------------------------------------
	public bool GetIsAttack()
	{
		if ( enemyState == State.ATTACK )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 敵機が待機状態か確認する.
	// ------------------------------------------------------------------------
	public bool GetIsFollowingLeader()
	{
		if ( enemyState == State.FOLLOWINGLEADER )
		{
			return true;
		}
		return false;
	}

	// ------------------------------------------------------------------------
	// 衝突判定.
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		// 岩石との衝突判定.
		if ( isEnbleByCollisionStone )
		{
			if ( collider.tag == "Stone" )
			{
				isBreakByStone = true;
				SetIsBreakEx2();
			}
		}
		
		// プレイヤーの背後判定.
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = true;
		}

	}
	
	void OnTriggerExit( Collider collider )
	{
		if ( collider.tag == "PlayerBackArea" )
		{
			isPlayerBackArea = false;
		}
	}
	
	public bool GetIsPlayerBackArea()
	{
		return isPlayerBackArea;
	}
	
	public bool GetIsBreakByPlayer()
	{
		return isBreakByPlayer;
	}
	public bool GetIsBreakByStone()
	{
		return isBreakByStone;
	}
	
	private string SetEnemyType()
	{
		if ( enemyTypeId.Length == 0 )
		{
			return "";
		}
		
		string tmpString;
		if ( enemyTypeId.Length == 1  )
		{
			tmpString = enemyTypeId + "-TYPE";
		}
		else
		{
			tmpString = enemyTypeId;
		}
		return tmpString;
	}
	
	public int GetLife()
	{
		return life;
	}
	
}
