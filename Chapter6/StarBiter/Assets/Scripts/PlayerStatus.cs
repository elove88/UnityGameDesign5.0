using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// プレイヤーの状態.
// ----------------------------------------------------------------------------
public class PlayerStatus : MonoBehaviour {

	public int playerCount = 3;						// プレイヤー数.
	
	public GameObject effectBomb;
	public GameObject GameOver;
	
	private enum State
	{
		INITIALIZE,
		INVISIBLE,			// 開始時は無敵.
		NOWPLAYING,
		STARTDESTRUCTION,	// プレイヤー破壊.
		DESTRUCTION,		// プレイヤー破壊中.
		WAITING,			// 待ち.
		RESTART				// 再開.
	}

	private State programState = State.INITIALIZE;	// 内部処理状態.
	private int waitTimeAfterExplosion = 2;			// 爆発アニメーション再生後の待ち時間.
	
	private bool isGAMEOVER	= false;				// ゲーム終了.

	private GameObject scoutingLaser;
	private MeshRenderer invisibleZone;
	private GameObject printScore;
	
	void Start () 
	{
		// scoutingLaserのインスタンスを取得.
		scoutingLaser = GameObject.FindGameObjectWithTag("ScoutingLaser");
		
		// 無敵表示のインスタンスのMeshRendererを取得.
		invisibleZone = GameObject.FindGameObjectWithTag("InvisibleZone").GetComponent<MeshRenderer>();
		
		// scoreのインスタンスを取得.
		printScore = GameObject.FindGameObjectWithTag("Score");
		
		// ゲーム開始の為、プレイヤーを表示する.
		ShowPlayer();
		
		programState = State.NOWPLAYING;
		
	}
	
	void Update ()
	{
	
		if ( !isGAMEOVER )
		{
			// 破壊確認.
			if ( programState == State.STARTDESTRUCTION )
			{
				programState = State.DESTRUCTION;
				
				// プレイヤーを破壊する.
				DestructPlayer();
			}
	
			// 待ち.
			if ( programState == State.DESTRUCTION )
			{
				if ( !this.GetComponent<AudioSource>().isPlaying )
				{
					programState = State.WAITING;
					
					// 一定時間待つ.
					StartCoroutine("Waiting");
				}
			}
			
			// ゲーム再開.
			if ( programState == State.RESTART )
			{
				programState = State.INITIALIZE;
				
				// ゲーム再開の為、プレイヤーを再表示する.
				bool ret = ShowPlayer();
				if ( ret )
				{
					// 再開直後は無敵.
					programState = State.INVISIBLE;
					// 無敵表示.
					invisibleZone.enabled = true;
					// 無敵の解除時間をセット.
					StartCoroutine( WaitAndUpdateState( 2f, State.NOWPLAYING ) );
				}
				else
				{
					isGAMEOVER = true;
				}
			}
		}
	}

	// ------------------------------------------------------------------------
	// プレイヤー破壊判定.
	// ------------------------------------------------------------------------
	void OnTriggerEnter( Collider collider )
	{
		if ( programState == State.NOWPLAYING ) {
			// 岩石小に衝突.
			if ( collider.tag == "Stone" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 敵機に衝突.
			if ( collider.tag == "Enemy" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 弾に衝突.
			if ( collider.tag == "EnemyVulcan" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// レーザーに衝突.
			if ( collider.tag == "EnemyLaser" )
			{
				programState = State.STARTDESTRUCTION;
			}
			
			// 弾に衝突.
			if ( collider.tag == "EnemyShot" )
			{
				programState = State.STARTDESTRUCTION;
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーを表示する.
	// ------------------------------------------------------------------------
	private bool ShowPlayer()
	{
		// プレイヤー数が0か確認.
		if ( playerCount <= 0 )
		{
			// ハイスコア更新.
			bool isHiScore = SetHiscore();
			
			// ゲームオーバーの文字を表示.
			GameObject gameOver = Instantiate( GameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
			gameOver.SendMessage( "SetIsHiScore", isHiScore );
			gameOver.SendMessage( "Show" );
			
			// オープニングへのクリックイベントを有効にする.
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 一定時間後にオープニング画面に戻る.
			StartCoroutine( WaitAndCallScene( 5f, "opening" ) );
			
			return false;
		}
		else
		{
		
			// プレイヤー数を1つ減らす.
			playerCount--;
			
			// プレイヤーを表示する.
			Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
	        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
	            meshRenderer.enabled = true;
	        }
			
			// Player Left の表示.
			if ( playerCount < 2 )
			{
				GameObject.FindGameObjectWithTag("PlayerLeft02").GetComponent<GUITexture>().enabled = false;
			}
			if ( playerCount < 1 )
			{
				GameObject.FindGameObjectWithTag("PlayerLeft01").GetComponent<GUITexture>().enabled = false;
			}
			
			// プレイヤーは生きている状態とする.
			this.GetComponent<PlayerController>().SetIsAlive( true );
			
			return true;
		}
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーを隠す.
	// ------------------------------------------------------------------------
	private void HidePlayer()
	{
		// プレイヤーを隠す.
		Component[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach ( MeshRenderer meshRenderer in meshRenderers ) {
            meshRenderer.enabled = false;
        }
		
		// プレイヤーは死んでいる状態とする.
		this.GetComponent<PlayerController>().SetIsAlive( true );
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーを破壊する.
	// ------------------------------------------------------------------------
	private void DestructPlayer()
	{
	
		// プレイヤーを消す.
		HidePlayer();
		
		// プレイヤーのコントロールに関する情報を初期状態に戻す.
		SendMessage( "Reset" );
		
		// 索敵レーザー、ロックオンに関する情報を初期状態に戻す.
		scoutingLaser.SendMessage( "Reset" );
		
		// 爆発
		ShowExplosion();

	}
	
	// ------------------------------------------------------------------------
	// プレイヤーが破壊された後の復活までの待ち.
	// ------------------------------------------------------------------------
	IEnumerator Waiting()
	{
		// 一定時待つ.
		yield return new WaitForSeconds( waitTimeAfterExplosion );

		// 画面をクリア.
		ClearDisplay();
		
		// ゲーム再開.
		programState = State.RESTART;
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーの爆発の表示.
	// ------------------------------------------------------------------------
	private void ShowExplosion()
	{
		// 爆発エフェクトのオブジェクトがあるか?.
		if ( effectBomb )
		{
			// エフェクト再生.
			Instantiate(
				effectBomb,
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) );
		}
		
		// 爆発音の作成.
		this.GetComponent<AudioSource>().Play();
		
	}
	
	// ------------------------------------------------------------------------
	// 画面をクリア.
	// ------------------------------------------------------------------------
	private void ClearDisplay()
	{	
		// EnemyMakerから作成した敵機を全て消去.
		GameObject[] enemyMakers = GameObject.FindGameObjectsWithTag("EnemyMaker");
		foreach( GameObject enemyMaker in enemyMakers )
		{
			enemyMaker.SendMessage("DestroyEnemys");
		}
		
		// 上の処理で消せなかった単独敵機、フォーメーションから外れた敵機の削除.
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		foreach( GameObject enemy in enemys )
		{
			enemy.SendMessage("DestroyEnemyEx");
		}
		
		// 敵機のショットを消去.
		GameObject[] enemyShots = GameObject.FindGameObjectsWithTag("EnemyShot");
		foreach( GameObject enemyShot in enemyShots )
		{
			Destroy( enemyShot );
		}
		
		// BOSSのレーザーを消去.
		GameObject[] enemyLasers = GameObject.FindGameObjectsWithTag("EnemyLaser");
		foreach( GameObject enemyLaser in enemyLasers )
		{
			Destroy( enemyLaser );
		}
		
		// BOSSのBulletを消去.
		GameObject[] enemyVulcans = GameObject.FindGameObjectsWithTag("EnemyVulcan");
		foreach( GameObject enemyVulcan in enemyVulcans )
		{
			Destroy( enemyVulcan );
		}
		
		// エフェクト消去
		GameObject[] tmpGameObjects = GameObject.FindGameObjectsWithTag("EffectBomb");
	    for ( int i = 0; i < tmpGameObjects.Length; i++)
		{
			if ( tmpGameObjects[i] != null )
			{
				Destroy( tmpGameObjects[i] );
				tmpGameObjects[i] = null;
			}
	    }
	}
	
	// ------------------------------------------------------------------------
	// プレイ中かどうかを返す.
	//  - INVISIBLEは含まれない.
	// ------------------------------------------------------------------------
	public bool GetIsNOWPLAYING()
	{
		if ( programState == State.NOWPLAYING )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// プレイヤーが操作可能かを返す.
	//  - INVISIBLEも含まれる.
	// ------------------------------------------------------------------------
	public bool GetCanPlay()
	{
		if ( programState == State.NOWPLAYING ||
			 programState == State.INVISIBLE )
		{
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// 指定した時間後、状態を変更する.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		// 指定した時間を待つ.
		yield return new WaitForSeconds( waitForSeconds );
		
		// 状態を更新.
		programState = state;
		
		// 無敵状態の非表示.
		invisibleZone.enabled = false;
	}
	
	// ------------------------------------------------------------------------
	// 指定した時間後、シーンを読み込む.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 指定した時間を待つ.
		yield return new WaitForSeconds( waitForSeconds );
		
		// ゲームシーンを呼び出す.
		Application.LoadLevel( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// HISCOREを保持する.
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		// SCORE/HISCOREを取得.
		int hiScore = int.Parse(
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text );
		int score = int.Parse(
			GameObject.FindGameObjectWithTag("Score").GetComponent<GUIText>().text );
		
		// HISCOREを超えたか?
		if ( score > hiScore )
		{
			// ハイスコア更新.
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text = score.ToString();
			
			// グローバル領域に保存する.
			GlobalParam.GetInstance().SetHiScore( printScore.GetComponent<PrintScore>().GetScore() );
			
			return true;
		}
		return false;
	}
	
}
