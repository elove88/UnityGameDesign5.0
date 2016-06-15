using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 索敵レーザーの制御.
// ----------------------------------------------------------------------------
public class ScoutingLaser : MonoBehaviour {
	
	public bool isScanOn = false;				// 索敵開始.
	
	public GameObject lockonLaser;

	public GameObject lockonSight1;				// ロックオンサイト1.
	public GameObject lockonSight2;				// ロックオンサイト2.
	public GameObject lockonSight3;				// ロックオンサイト3.
	public GameObject lockonSight4;				// ロックオンサイト4.
	public GameObject lockonSight5;				// ロックオンサイト5.
	public GameObject lockonSight6;				// ロックオンサイト6.
	
	public float ScoutingLaserTurnRate = 15f;	// プレイヤーの方向に合わせて索敵レーザーが回転する割合.
	public float ScoutingLaserFowardPosition = 5.5f;	// 索敵レーザーの開始位置(プレイヤーからみて前方). 
	
	private GameObject[] lockonSights;			// ロックオンサイト1～6.
	
	private GameObject player;					// プレイヤー.
	private GameObject scoutingLaserMesh;		// 索敵レーザーのコリジョン補間.
	private MeshRenderer scoutingLaserLine;		// プレイヤー前方方向に表示する索敵レーザー.
	private GameObject lockBonus;				// LockBonus.
	private GameObject lockSlot;				// LockSlot.
	private GameObject subScreenMessage;		// SubScreenのメッセージ領域.
	
	private int lockonCount = 0;				// ロックオン数.
	
	private static int MAX_LOCKON_COUNT = 6;	// ロックオン最大数.
	
	private GameObject[] lockedOnEnemys;		// ロックオンした敵機.
	private int[] lockedOnEnemyIds;				// ロックオンした敵機のインスタンスID.
	private int[] lockonLaserIds;				// ロックオンレーザーのインスタンスID.
	private GameObject[] lockedOnSights;		// 敵機をロックオンしたロックオンサイト.
	
	private float[] lockonLaserStartRotation =	// ロックオンレーザー射出時の角度.
	{
		-40f, 40f, -70f, 70f, -100f, 100f
	};
	
	private int	invalidInstanceId = -1;		// lockedOnEnemyIds[] が未使用であることを表す値.
	
	void Start () 
	{
		// ロックオン敵機.
		lockedOnEnemys = new GameObject[MAX_LOCKON_COUNT];
		lockedOnEnemyIds = new int[MAX_LOCKON_COUNT];
		lockonLaserIds = new int[MAX_LOCKON_COUNT];
		lockedOnSights = new GameObject[MAX_LOCKON_COUNT];
		
		// ロックオンサイトのインスタンスを取得.
		lockonSights = new GameObject[MAX_LOCKON_COUNT];
		lockonSights[0] = lockonSight1;
		lockonSights[1] = lockonSight2;
		lockonSights[2] = lockonSight3;
		lockonSights[3] = lockonSight4;
		lockonSights[4] = lockonSight5;
		lockonSights[5] = lockonSight6;
		
		// playerのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// 索敵レーザーのコリジョン補間を取得.
		scoutingLaserMesh = GameObject.FindGameObjectWithTag("ScoutingLaserMesh");

		// 索敵レーザーの前方への線のインスタンスを取得.
		scoutingLaserLine = GameObject.FindGameObjectWithTag("ScoutingLaserLine").GetComponent<MeshRenderer>();
		
		// ロックオンサイトの初期化.
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// LockBonusのインスタンスを取得.
		lockBonus = GameObject.FindGameObjectWithTag("LockBonus");
		
		// LockSlotのインスタンスを取得.
		lockSlot = GameObject.FindGameObjectWithTag("LockSlot");
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// lockedOnEnemyIds[] が未使用であることを表す値.
		// 自分自身はロックオンされることはないので.
		invalidInstanceId = this.gameObject.GetInstanceID();

		for(int i = 0;i < lockedOnEnemyIds.Length;i++) {

			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
		}
		
	}
	
	void Update ()
	{
	
		// 索敵レーザー停止時、ロックオンした敵機にロックオンレーザー発射.
		if ( isScanOn == false ) 
		{
			StartLockonLaser();
		}
		
		// 索敵レーザーの位置を設定.
		UpdateTransformMesh();

	}

	// ------------------------------------------------------------------------
	// 索敵レーザーの有効無効切り替え.
	//  - 索敵レーザーの開始時間に待ちは無いバージョン.
	// ------------------------------------------------------------------------	
	public void SetIsScanOn( bool isScanOn )
	{		
		// 索敵レーザーの(有効/無効)切り替え.
		this.isScanOn = isScanOn;
		
		// 索敵レーザーの(表示/非表示)切り替え.
		this.GetComponent<TrailRenderer>().enabled = isScanOn;
		scoutingLaserLine.enabled = isScanOn;
		
		// メッセージ表示.
		if ( isScanOn == true )
		{
			StartCoroutine( "SetSearchingMessage" );
		}
		else
		{
			StopCoroutine( "SetSearchingMessage" );
		}
		
		// 索敵レーザーの音.
		if ( isScanOn == true )
		{
			this.GetComponent<AudioSource>().Play();
		} 
		else
		{
			this.GetComponent<AudioSource>().Stop();
		}
	}
	IEnumerator SetSearchingMessage()
	{
		yield return new WaitForSeconds( 0.5f );
		subScreenMessage.SendMessage( "SetMessage", "SEARCHING ENEMY..." );
	}
	
	// ------------------------------------------------------------------------
	// 索敵レーザーの位置設定 MeshCollider バージョン.
	//  - 常にプレイヤーの前方に表示.
	//  - 索敵レーザーのスピード調整(遅く)をする.
	//    - プレイヤーの向きを変えるスピードが速い場合に発生する以下の問題を回避する為
	//      - 当たらない位置ができてしまう.
	//      - TrailRendererが綺麗な円にならない.
    //  - TrailRendererの為の調整.
	//    - TrailRendererはPositionの変更前と後の間に描かれるので、回転方向に合わせて位置を変更する.
	// ------------------------------------------------------------------------
	private void UpdateTransformMesh()
	{
		
		// 現在の方向からプレイヤーの方向へ、指定したスピードで傾けた後の角度を取得.
		float targetRotationAngle = player.transform.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		currentRotationAngle = Mathf.LerpAngle(
			currentRotationAngle,
			targetRotationAngle,
			ScoutingLaserTurnRate * Time.deltaTime );
		Quaternion tiltedRotation = Quaternion.Euler( 0, currentRotationAngle, 0 );
		
		// コリジョンに使うMeshを作成.	
		if ( isScanOn )
		{
			float[] tmpAngle = new float[]{ player.transform.eulerAngles.y, transform.eulerAngles.y };
			scoutingLaserMesh.SendMessage("makeFanShape", tmpAngle);
		}
		else
		{
			scoutingLaserMesh.SendMessage("clearShape");
		}
		
		// 角度を設定.
		transform.rotation = tiltedRotation;
		
		// 位置を変更.
		transform.position = new Vector3(
			ScoutingLaserFowardPosition * Mathf.Sin( Mathf.Deg2Rad * currentRotationAngle ),
			0,
			ScoutingLaserFowardPosition * Mathf.Cos( Mathf.Deg2Rad * currentRotationAngle )
		);

	}
	
	// ------------------------------------------------------------------------
	// ロックオン処理.
	// ------------------------------------------------------------------------
	public void Lockon( Collider collider )
	{
		// 敵機をロックオン.
		if ( collider.gameObject.tag == "Enemy" ) {
			
			// ロックオンをしていない場合にロックオンする.
			int targetId = collider.gameObject.GetInstanceID();
			bool isLockon = IncreaseLockonCount( targetId );
			if ( isLockon ) {
				
				// ------------------------------------------------------------
				// ロックオン
				// ------------------------------------------------------------
				
				// ロックオン番号を決定.
				int lockonNumber = getLockonNumber();
							
				if ( lockonNumber >= 0 ) {
					
					// ロックオンサイトの表示位置はロックオンした敵機の位置に表示.
					Vector3 targetPosition = collider.gameObject.transform.position;
					Quaternion tagetRotation = new Quaternion( 0f, 180f, 0f, 0f );
					
					// ロックオンサイトのインスタンス生成.
					GameObject lockonSight;
					lockonSight = Instantiate( lockonSights[lockonNumber], targetPosition, tagetRotation ) as GameObject;
					lockonSight.SendMessage( "SetLockonEnemy", collider.gameObject );
					
					// ロックオンリストにロックオンした敵機を追加.
					lockedOnEnemyIds[lockonNumber] = targetId;
					
					// ロックオンしたオブジェクトを保持しておく.
					lockedOnEnemys[lockonNumber] = collider.gameObject;
					
					// ロックオンサイトを保持する.
					lockedOnSights[lockonNumber] = lockonSight;
					
					// メッセージ表示.
					subScreenMessage.SendMessage("SetMessage", "LOCKED ON SOME ENEMIES." );
					lockSlot.SendMessage("SetLockCount", lockonCount );
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ロックオン数更新.
	//  - true:加算成功、false:加算失敗.
	// ------------------------------------------------------------------------
	public bool IncreaseLockonCount( int enemyId )
	{
		// 最大ロックオン数以下か.
		if ( lockonCount < MAX_LOCKON_COUNT )
		{
			// ロックオンしていない敵機か.
			if ( !IsLockon( enemyId ) )
			{
				// ロックオン数+1.
				lockonCount++;
				return true;
			}
		}
		// ロックオン済みだった.
		return false;
	}
	
	// ------------------------------------------------------------------------
	// ロックオンの空き領域の添え字を返す
	//  - 空きあり: 0以上
	//  - 空きなし: -1
	// ------------------------------------------------------------------------
	private int getLockonNumber()
	{
		// ロックオンレーザーを管理する配列で空きの領域を探し、その領域の添え字を返す.
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == invalidInstanceId )
			{
				return i;
			}
		}
		return -1;
	}
	
	// ------------------------------------------------------------------------
	// ロックオン数減らす.
	// ------------------------------------------------------------------------
	public void DecreaseLockonCount( int instanceId )
	{
		// ロックオン数を減らす.
		if ( lockonCount > 0 ) {
			lockonCount--;
			lockSlot.SendMessage("SetLockCount", lockonCount );		
		}
		
		// ロックオン済みの情報を消去.
		for( int i = 0; i < lockedOnEnemyIds.Length; i++ )
		{
			if ( lockedOnEnemyIds[i] == instanceId )
			{
				if ( lockedOnSights[i] )
				{
					lockedOnSights[i].SendMessage( "Destroy" );
				}
				lockedOnEnemyIds[i] = invalidInstanceId;
				lockedOnEnemys[i] = null;
				lockonLaserIds[i] = invalidInstanceId;
				lockedOnSights[i] = null;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ロックオン状況確認.
	// ------------------------------------------------------------------------
	public bool IsLockon( int enemyId )
	{
		// ロックオン済み敵機のリストに未登録か.
		int existIndex = System.Array.IndexOf( lockedOnEnemyIds, enemyId );

		if ( existIndex == -1 )
		{
			// ロックオン未
			return false;
		}
		// ロックオン済み
		return true;
	}
	
	// ------------------------------------------------------------------------
	// ロックオンレーザー発射.
	// ------------------------------------------------------------------------
	private void StartLockonLaser()
	{
		int countLockon = 0;
		
		// ロックオン数を数える.
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
					// ロックオンボーナス.
					countLockon++;
					lockBonus.SendMessage("SetLockCount", countLockon );
				}
			}
		}

		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			if ( lockedOnEnemyIds[i] != invalidInstanceId )
			{
				// ロックオンレーザーをまだ作成していない時のみ.
				if ( lockonLaserIds[i] == invalidInstanceId )
				{
				
					// ------------------------------------------------------------
					// ロックオンレーザー作成.
					// ------------------------------------------------------------
					
					// プレイヤーの座標取得.
					Vector3 playerPos = player.transform.position;
					Quaternion playerRot = player.transform.rotation;
					
					// レーザーの射出角度を決める.
					Quaternion startRotation = player.transform.rotation;
					float laserRotationAngle = startRotation.eulerAngles.y;

					laserRotationAngle += lockonLaserStartRotation[i];
					Quaternion tiltedRotation = Quaternion.Euler( 0, laserRotationAngle, 0 );
					playerRot = tiltedRotation;
					
					// ロックオンレーザー作成.
					GameObject tmpLockonLaser;
					tmpLockonLaser = Instantiate( lockonLaser, playerPos, playerRot ) as GameObject;
					tmpLockonLaser.SendMessage( "SetLockonBonus", Mathf.Pow (2, countLockon ) );
					tmpLockonLaser.SendMessage( "SetTargetEnemy", lockedOnEnemys[i] );
					lockonLaserIds[i] = tmpLockonLaser.GetInstanceID();
				}
			}
		}
		
		// メッセージ消去.
		if ( countLockon == 0 )
		{
		}

	}
	
	public void Reset()
	{
		// 索敵レーザーを無効.
		this.isScanOn = false;
		
		// 索敵レーザーを非表示.
		this.GetComponent<TrailRenderer>().enabled = false;
		scoutingLaserLine.enabled = false;
		
		// 索敵レーザー音を停止.
		if ( this.GetComponent<AudioSource>().isPlaying )
		{
			this.GetComponent<AudioSource>().Stop();
		} 
		
		// ロックオンに関する情報を初期値に.
		for( int i = 0; i < MAX_LOCKON_COUNT; i++ )
		{
			lockedOnEnemys[i] = null;
			lockedOnEnemyIds[i] = invalidInstanceId;
			lockonLaserIds[i] = invalidInstanceId;
			if ( lockedOnSights[i] )
			{
				lockedOnSights[i].SendMessage("Destroy");
			}
			lockedOnSights[i] = null;
		}
		lockonCount = 0;
		
		// ロックスロットの表示を初期値に.
		lockSlot.SendMessage( "SetLockCount", lockonCount );
		
		// ロックオンボーナスの表示を初期値に.
		lockBonus.SendMessage( "SetLockCount", lockonCount );
		
	}

}