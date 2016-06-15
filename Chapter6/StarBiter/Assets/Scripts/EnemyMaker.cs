using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// EnemyMaker
//  - 指定の間隔で指定の敵機を作成する.
//  - シーンでの敵機の作成上限を指定する.
// ----------------------------------------------------------------------------
public class EnemyMaker : MonoBehaviour {
	
	public float creationInterval = 5.0f;		// 敵機作成間隔.
	public GameObject enemyGameObject;			// 敵機ゲームオブジェクト.
	
	public int maxEnemysInScene = 6;			// シーン内の作成上限.
	public int maxEnemys = 1;					// 総作成数.
	
	public bool canShoot = false;				// 弾を撃つことができる.
	public bool addToSpeed = false;				// スピードを上げる.
	
	public bool isBoss = false;					// 最終ボスかどうか.
	
	private bool isMaking = false;				// 敵機を作成中.
	private int enemyCount = 0;					// 現在の敵機作成数.
	private GameObject[] enemyGameObjects;		// 作成した敵機のインスタンス.
	private int[] enemyIds;						// 作成した敵機のインスタンスID.
	
	private PlayerStatus playerStatus;			// プレイヤーステータスのインスタンス.
	
	private int destroyedEnemyCount = 0;		// 破壊された敵機の数.
	
	private GameObject stageController;			// ステージコントローラーのインスタンス.
	
	private int stageIndex = 0;					// ステージの進行状況.
	
	void Start () {
		
		// ステージコントローラーのインスタンスを取得.
		stageController = GameObject.FindGameObjectWithTag("StageController");
		
		// プレイヤーステータスのインスタンスを取得.
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();

		// 作成する敵機の情報を保持する領域を確保.
		enemyGameObjects = new GameObject[maxEnemysInScene];
		enemyIds = new int[maxEnemysInScene];

	}
	
	void Update () {
	
		// プレイヤーが操作可能な時のみ処理する.
		if ( playerStatus.GetIsNOWPLAYING() )
		{
		
			// 敵機の作成上限すべて破壊されたか?
			if ( destroyedEnemyCount == maxEnemys )
			{
				// StageControllerへステージ終了の連絡.
				stageController.SendMessage( "SetStateEnd", stageIndex );
				
				// EnemyMakerを停止する.
				SetMakingStop();
			}
		
			// 敵機はまだ作れるか?
			if ( enemyCount < maxEnemysInScene ) {
				
				// 作成中ではないか?
				if ( !isMaking ) {
					
					// 作成中.
					isMaking = true;
					
					// 指定した時間(CreationInterval)経過後に作成する.
					StartCoroutine( CreateEnemy() );
				}
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 指定した時間(CreationInterval)経過後に敵機を作成する.
	// ------------------------------------------------------------------------
	IEnumerator CreateEnemy()
	{
		// 作成数をカウントアップ.
		enemyCount++;
		
		// 一定間隔待つ.
		yield return new WaitForSeconds( creationInterval );
		
		// 敵機を作成.
		GameObject tmpEnemy = Instantiate(
			enemyGameObject,
			Vector3.zero,
			new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		tmpEnemy.SendMessage("SetEmenyMaker", this.gameObject, SendMessageOptions.DontRequireReceiver );
		
		// ショットの設定.
		if ( canShoot )
		{
			tmpEnemy.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
			// 子があれば全て送る.
			Transform[] children = tmpEnemy.GetComponentsInChildren<Transform>();
      		foreach ( Transform child in children )
			{
    			if ( child.tag == "Enemy" )
				{
					child.SendMessage( "SetCanShoot", true, SendMessageOptions.DontRequireReceiver );
				}
			}
		}
		
		// スピードの設定.
		if ( addToSpeed && tmpEnemy.GetComponent<EnemyType02Controller>() )
		{
			tmpEnemy.SendMessage( "SpeedUp", null, SendMessageOptions.DontRequireReceiver );
		}
		
		// 作成したgameObjectを保持
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] == null )
			{
				enemyGameObjects[i] = tmpEnemy;
				enemyIds[i] = tmpEnemy.GetInstanceID();
				break;
			}
	    }
		
		// 作成要求を終了.
		isMaking = false;

	}
	
	// ------------------------------------------------------------------------
	// 敵機作成数から一つ減らす
	// ------------------------------------------------------------------------
	public void DecreaseEnemyCount( int instanceId )
	{
		// 敵機数を減らす.
		if ( enemyCount > 0 ) {
			enemyCount--;
		}
		
		// 作成済み敵機情報を消去.
		for( int i = 0; i < enemyIds.Length; i++ )
		{
			if ( enemyIds[i] == instanceId )
			{
				enemyIds[i] = 0;
				enemyGameObjects[i] = null;
			}
		}
		
		// 破壊した敵機の数を増やす.
		destroyedEnemyCount++;
	}
	
	// ------------------------------------------------------------------------
	// 作成した全ての敵機を消去.
	// ------------------------------------------------------------------------
	public void DestroyEnemys()
	{
		// 最終ボスの場合は何もしない.
		if ( isBoss ) { return; }
		
		// 作成した敵機をすべて消去する.
	    for ( int i = 0; i < enemyGameObjects.Length; i++)
		{
			if ( enemyGameObjects[i] != null )
			{
				Destroy( enemyGameObjects[i] );
				enemyGameObjects[i] = null;
				enemyIds[i] = 0;
			}
	    }
		
		// 敵機作成数をリセット
		enemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 弾を撃つことができるようにする.
	// ------------------------------------------------------------------------
	public void SetCanShoot( bool canShoot )
	{
		this.canShoot = canShoot;
	}
	
	// ------------------------------------------------------------------------
	// スピードを上げる.
	// ------------------------------------------------------------------------
	public void SetAddToSpeed( bool addToSpeed )
	{
		this.addToSpeed = addToSpeed;
	}
	
	// ------------------------------------------------------------------------
	// 敵機の作成機能を停止する.
	// ------------------------------------------------------------------------
	private void SetMakingStop()
	{
		maxEnemysInScene = 0;
		destroyedEnemyCount = 0;
	}
	
	// ------------------------------------------------------------------------
	// 敵機作成間隔を設定する.
	// ------------------------------------------------------------------------
	public void SetCreateInterval( float creationInterval )
	{
		this.creationInterval = creationInterval;
	}
	
	// ------------------------------------------------------------------------
	// シーン内の作成上限を設定する.
	// ------------------------------------------------------------------------
	public void SetMaxEnemysInScene( int maxEnemysInScene )
	{
		this.maxEnemysInScene = maxEnemysInScene;
	}
	
	// ------------------------------------------------------------------------
	// ステージの進行状況を保持する.
	// ------------------------------------------------------------------------
	public void SetStage( int stageIndex )
	{
		this.stageIndex = stageIndex;
	}

}
