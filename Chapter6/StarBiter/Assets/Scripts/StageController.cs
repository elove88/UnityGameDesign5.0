using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// StageController
//  - ステージを制御.
//  - 使い方.
//    - 本スクリプトが付いたゲームオブジェクトを配置する.
//  - 動き仕様.
//    - ステージは以下の通りに進行する.
//      1.  スタート
//      2.  斥候の敵と遭遇
//          - 敵機種: TYPE 01 + TYPE 04
//      3.  第1波攻撃
//          - 敵機種: TYPE 01 + TYPE 01 バトルステーション
//      4.  斥候の敵と遭遇
//          - 敵機種: TYPE 01 + TYPE 02 + TYPE 04
//      5.  第2波攻撃
//          - 敵機種: TYPE 01 + TYPE 02 + TYPE 02 バトルステーション
//      6.  斥候の敵と遭遇
//          - 敵機種: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 04
//      7.  第3波攻撃
//          - 敵機種: TYPE 01 + TYPE 02 + TYPE 03 + TYPE 03 バトルステーション
//      8.  一瞬の静寂
//      9.  BOSS登場
//      10. BOSS戦闘
//      11. ゲームオーバー
//    - ※バトルステーション: 敵機のフォーメーション攻撃
// ---------------------------------------------------------------------------
public class StageController : MonoBehaviour {

	public GameObject EnemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION
	public GameObject EnemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION
	public GameObject EnemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION
	public GameObject EnemyMakerType01;		// ENEMY MAKER TYPE 01
	public GameObject EnemyMakerType02;		// ENEMY MAKER TYPE 02
	public GameObject EnemyMakerType03;		// ENEMY MAKER TYPE 03
	public GameObject EnemyMakerType04;		// ENEMY MAKER TYPE 04
	public GameObject Boss;					// BOSS
	public GameObject GameOver;
	
	private enum Level
	{
		DEBUG,
		EASY,
		NORMAL,
		HARD
	}
	private Level level = Level.NORMAL;
	
	// レベル別作成数.
	//  - 0: TYPE01.
	//  - 1: TYPE01 FORMATION.
	//  - 2: TYPE02.
	//  - 3: TYPE02 FORMATION.
	//  - 4: TYPE03.
	//  - 5: TYPE03 FORMATION.
	private int[,] maxEnemyInSceneByLevel =
	{
		{ 0, 0, 0, 0, 0, 0 },
		{ 1, 1, 1, 1, 1, 1 },
		{ 3, 4, 3, 2, 1, 2 },
		{ 6, 6, 6, 4, 6, 4 },
	};

	private enum EnemyType
	{
		TYPE01,
		TYPE01FORMATION,
		TYPE02,
		TYPE02FORMATION,
		TYPE03,
		TYPE03FORMATION,
	}
	
	// 各ステージ.
	private enum Stage
	{
		START,								// 開始.
		PATROL1,							// 斥候1.
		ATTACK1,							// 第1波攻撃.
		PATROL2,							// 斥候2.
		ATTACK2,							// 第2波攻撃.
		PATROL3,							// 斥候3.
		ATTACK3,							// 第3波攻撃.
		SILENCE,							// 静寂.
		BOSS,								// ボス.
		GAMECLEAR,							// ゲームクリア.
		END
	}
	
	// ステージの各状態.
	private enum State
	{
		INITIALIZE,							// 準備.
		NOWPLAYING,							// プレイ中.
		END									// 終了.
	}
	
	private Stage stage;					// ステージ.
	private State state;					// ステージの状態.
	
	private GameObject enemyMakerType01Formation;	// ENEMY MAKER TYPE 01 FORMATION インスタンス.
	private GameObject enemyMakerType02Formation;	// ENEMY MAKER TYPE 02 FORMATION インスタンス.
	private GameObject enemyMakerType03Formation;	// ENEMY MAKER TYPE 03 FORMATION インスタンス.
	private GameObject enemyMakerType01;	// ENEMY MAKER TYPE 01 インスタンス.
	private GameObject enemyMakerType02;	// ENEMY MAKER TYPE 02 インスタンス.
	private GameObject enemyMakerType03;	// ENEMY MAKER TYPE 03 インスタンス.
	private GameObject enemyMakerType04;	// ENEMY MAKER TYPE 04 インスタンス.
	private GameObject boss;				// BOSS インスタンス.
	private PlayerStatus playerStatus;		// プレイヤーステータスのインスタンス.
	private GameObject subScreenMessage;	// SubScreenのメッセージ領域.
	
	private GameObject txtStageController;	// DEBUG
	
	void Start () 
	{
		// --------------------------------------------------------------------
		// 各インスタンスを取得.
		// --------------------------------------------------------------------
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");
		
		// プレイヤーステータスのインスタンスを取得.
		playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
		
		// --------------------------------------------------------------------
		// 初期値.
		// --------------------------------------------------------------------
		
		// ステージを設定.
		stage = Stage.START;
		state = State.INITIALIZE;
		
		// --------------------------------------------------------------------
		// 各インスタンスを生成.
		// --------------------------------------------------------------------

		if ( EnemyMakerType01Formation )
		{
			enemyMakerType01Formation = Instantiate(
				EnemyMakerType01Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02Formation )
		{
			enemyMakerType02Formation = Instantiate(
				EnemyMakerType02Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03Formation )
		{
			enemyMakerType03Formation = Instantiate( 
				EnemyMakerType03Formation,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType01 )
		{
			enemyMakerType01 = Instantiate( 
				EnemyMakerType01,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType02 )
		{
			enemyMakerType02 = Instantiate(
				EnemyMakerType02,
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType03 )
		{
			enemyMakerType03 = Instantiate( 
				EnemyMakerType03, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( EnemyMakerType04 )
		{
			enemyMakerType04 = Instantiate( 
				EnemyMakerType04, 
				Vector3.zero, 
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		if ( Boss )
		{
			boss = Instantiate(
				Boss, 
				Vector3.zero,
				new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
		}
		
	}
	
	void Update () 
	{
		
		// ステージを更新.
		UpdateStage();

	}
	
	// ------------------------------------------------------------------------
	// 各ステージの処理.
	// ------------------------------------------------------------------------
	private void UpdateStage()
	{
		// プレイヤーが操作可能な時のみステージを進める.
		if ( playerStatus.GetIsNOWPLAYING() )
		{			
			// 開始.
			StageStart();
			
			// 開始の終了.
			StageStartEnd();
			
			// 斥候(1).
			StagePatrol1Start();
			
			// 斥候(1)終了.
			StagePatrol1End();
			
			// 第1波攻撃.
			StageAttack1Start();
			
			// 第1波攻撃終了.
			StageAttack1End();
			
			// 斥候(2).
			StagePatrol2Start();
			
			// 斥候(2)終了.
			StagePatrol2End();
	
			// 第2波攻撃.
			StageAttack2Start();
			
			// 第2波攻撃終了.
			StageAttack2End();
			
			// 斥候(3).
			StagePatrol3Start();
			
			// 斥候(3)終了.
			StagePatrol3End();
			
			// 第3波攻撃.
			StageAttack3Start();
			
			// 第3波攻撃終了.
			StageAttack3End();
	
			// つかの間の静寂.
			SilenceStart();
			
			// つかの間の静寂終了.
			SilenceEnd();
			
			// BOSS戦 開始.
			BossAttackStart();
			
			// BOSS戦 終了.
			BossAttackEnd();
			
			// GameClear
			GameClearWait();
			
			// GameClear
			GameClearMessage();
		}
	}

	// ------------------------------------------------------------------------
	// ステージ - 開始.
	// ------------------------------------------------------------------------
	private void StageStart()
	{
		if ( stage == Stage.START && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 の作成を開始.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// ステージの終了時間を設定.
			StartCoroutine( WaitAndUpdateState( 3f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 開始の終了.
	// ------------------------------------------------------------------------
	private void StageStartEnd()
	{
		if ( stage == Stage.START && state == State.END )
		{
			// 次のステージへ.
			stage = Stage.PATROL1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(1).
	// ------------------------------------------------------------------------
	private void StagePatrol1Start()
	{
		if ( stage == Stage.PATROL1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;	

			// TYPE 04 の作成を開始.
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>().
					SetStage( (int)stage );
			
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(1)終了.
	// ------------------------------------------------------------------------
	private void StagePatrol1End()
	{
		if ( stage == Stage.PATROL1 && state == State.END )
		{
			// 次のステージへ.
			stage = Stage.ATTACK1;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第1波攻撃.
	// ------------------------------------------------------------------------
	private void StageAttack1Start()
	{
		if ( stage == Stage.ATTACK1 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
					
			// TYPE 01 FORMATION の作成を開始.
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01FORMATION
					] );
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
			
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage", 
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
			
			// ステージの終了時間を設定.
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );

		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第1波攻撃終了.
	// ------------------------------------------------------------------------
	private void StageAttack1End()
	{
		if ( stage == Stage.ATTACK1 && state == State.END )
		{
			// TYPE 01 FORMATION の作成を停止.
			if ( enemyMakerType01Formation )
			{
				enemyMakerType01Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
		
			// 次のステージへ.
			stage = Stage.PATROL2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(2).
	// ------------------------------------------------------------------------
	private void StagePatrol2Start()
	{
		if ( stage == Stage.PATROL2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 02 の作成を開始.
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
			}
			
			// TYPE 04 の作成を開始.
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(2)終了.
	// ------------------------------------------------------------------------
	private void StagePatrol2End()
	{
		if ( stage == Stage.PATROL2 && state == State.END )
		{
			// 次のステージへ.
			stage = Stage.ATTACK2;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第2波攻撃.
	// ------------------------------------------------------------------------
	private void StageAttack2Start()
	{
		if ( stage == Stage.ATTACK2 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 の作成量を増やす.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// TYPE 02 FORMATION の作成を開始.
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02FORMATION
					] );
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
			
			// ステージの終了時間を設定.
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第2波攻撃終了.
	// ------------------------------------------------------------------------
	private void StageAttack2End()
	{
		if ( stage == Stage.ATTACK2 && state == State.END )
		{
			// TYPE 02 FORMATION の作成を停止.
			if ( enemyMakerType02Formation )
			{
				enemyMakerType02Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 01 の作成量を戻す.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
				// ショットを撃てるようにする.
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetCanShoot( true );
			}

			// 次のステージへ.
			stage = Stage.PATROL3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(3).
	// ------------------------------------------------------------------------
	private void StagePatrol3Start()
	{
		if ( stage == Stage.PATROL3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 03 の作成を開始.
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03
					] );
			}
			
			// TYPE 04 の作成を開始.
			if ( enemyMakerType04 )
			{
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				enemyMakerType04.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
			
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"PATROL SHIP IS COMING AHEAD." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 斥候(3)終了.
	// ------------------------------------------------------------------------
	private void StagePatrol3End()
	{
		if ( stage == Stage.PATROL3 && state == State.END )
		{
			// 次のステージへ.
			stage = Stage.ATTACK3;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第3波攻撃.
	// ------------------------------------------------------------------------
	private void StageAttack3Start()
	{
		if ( stage == Stage.ATTACK3 && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// TYPE 01 の作成量を増やす.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE01
					] );
			}
			
			// TYPE 02 の作成量を増やす.
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE02
					] );
				// スピードをあげる.
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetAddToSpeed( true );
			}
			
			// TYPE 03 FORMATION の作成を開始.
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( maxEnemyInSceneByLevel
					[
						(int)level, 
						(int)EnemyType.TYPE03FORMATION
					] );
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"BATTLE STATIONS HAS APPROACHED." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
			
			// ステージの終了時間を設定.
			StartCoroutine( WaitAndUpdateState( 20f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - 第3波攻撃終了.
	// ------------------------------------------------------------------------
	private void StageAttack3End()
	{
		if ( stage == Stage.ATTACK3 && state == State.END )
		{
			// TYPE 03 FORMATION の作成停止.
			if ( enemyMakerType03Formation )
			{
				enemyMakerType03Formation.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 01 の作成停止.
			if ( enemyMakerType01 )
			{
				enemyMakerType01.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 02 の作成停止.
			if ( enemyMakerType02 )
			{
				enemyMakerType02.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// TYPE 03 の作成停止.
			if ( enemyMakerType03 )
			{
				enemyMakerType03.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 0 );
			}
			
			// 次のステージへ.
			stage = Stage.SILENCE;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - つかの間の静寂.
	// ------------------------------------------------------------------------
	private void SilenceStart()
	{
		if ( stage == Stage.SILENCE && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// サブスクリーンへメッセージ出力.
			subScreenMessage.SendMessage("SetMessage"," ");
			subScreenMessage.SendMessage( "SetMessage", "CAUTION!" );
			subScreenMessage.SendMessage(
				"SetMessage",
				"CAUGHT HIGH ENERGY REACTION AHEAD." );
			subScreenMessage.SendMessage("SetMessage"," ");
			
			// ステージの終了時間を設定.
			StartCoroutine( WaitAndUpdateState( 10f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - つかの間の静寂終了.
	// ------------------------------------------------------------------------
	private void SilenceEnd()
	{
		if ( stage == Stage.SILENCE && state == State.END )
		{	
			// 次のステージへ.
			stage = Stage.BOSS;
			state = State.INITIALIZE;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - BOSS戦 開始.
	// ------------------------------------------------------------------------
	private void BossAttackStart()
	{
		if ( stage == Stage.BOSS && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// BOSSを作成.
			if ( boss )
			{
				boss.GetComponent<EnemyMaker>()
					.SetMaxEnemysInScene( 1 );
				boss.GetComponent<EnemyMaker>()
					.SetStage( (int)stage );
				
				// サブスクリーンへメッセージ出力.
				subScreenMessage.SendMessage("SetMessage"," ");
				subScreenMessage.SendMessage(
					"SetMessage",
					"ACKNOWKEDGED SPIDER-TYPE" );
				subScreenMessage.SendMessage(
					"SetMessage",
					"THE LIMITED-WARFARE ATTACK WEAPON." );
				subScreenMessage.SendMessage("SetMessage"," ");
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージ - BOSS戦 終了.
	// ------------------------------------------------------------------------
	private void BossAttackEnd()
	{
		if ( stage == Stage.BOSS && state == State.END )
		{
			// 次のステージへ.
			stage = Stage.GAMECLEAR;
			state = State.INITIALIZE;
		}
	}

	// ------------------------------------------------------------------------
	// ステージ - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearWait()
	{
		if ( stage == Stage.GAMECLEAR && state == State.INITIALIZE )
		{
			state = State.NOWPLAYING;
			
			// BOSS戦後の待ち時間を設定.
			StartCoroutine( WaitAndUpdateState( 2f, State.END ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// SCOREがHISCOREを超えた場合、保持する.
	// ------------------------------------------------------------------------
	private bool SetHiscore()
	{
		int hiScore = int.Parse(
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text );
		int score = int.Parse(
			GameObject.FindGameObjectWithTag("Score").GetComponent<GUIText>().text );
		
		if ( score > hiScore )
		{
			// ハイスコア更新.
			GameObject.FindGameObjectWithTag("HighScore").GetComponent<GUIText>().text = score.ToString();
			
			// グローバル領域に保存する.
			GlobalParam.GetInstance().SetHiScore( score );
			
			return true;
		}
		return false;
	}
	
	// ------------------------------------------------------------------------
	// ステージ - GameClear.
	// ------------------------------------------------------------------------
	private void GameClearMessage()
	{
		if ( stage == Stage.GAMECLEAR && state == State.END )
		{
			stage = Stage.END;
				
			// ハイスコア更新.
			bool isHiScore = SetHiscore();
			
			// ゲームオーバーの文字を表示.
			GameObject gameOver = Instantiate( GameOver, Vector3.zero, new Quaternion(0f, 0f, 0f, 0f) ) as GameObject;
			gameOver.SendMessage( "SetIsHiScore", isHiScore );
			gameOver.SendMessage( "SetDefeatedBoss", true );
			gameOver.SendMessage( "Show" );
			
			// オープニングへのクリックイベントを有効にする.
			GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ClickToOpening>().enabled = true;
			
			// 一定時間後にオープニング画面に戻る.
			StartCoroutine( WaitAndCallScene( 5f, "ending" ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 指定した時間経過後にシーンを呼び出す.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndCallScene( float waitForSeconds, string sceneName )
	{
		// 指定した時間を待つ.
		yield return new WaitForSeconds( waitForSeconds );
		
		// ゲームシーンを呼び出す.
		Application.LoadLevel( sceneName );
	}
	
	// ------------------------------------------------------------------------
	// ステージの状態を更新.
	// ------------------------------------------------------------------------
	public void SetStateEnd( int stageIndex )
	{
		// ステージが他の処理で更新されていない時のみ更新する.
		if ( stageIndex == (int)stage )
		{
			state = State.END;
		}
	}
	
	// ------------------------------------------------------------------------
	// ステージの終了を待つ処理.
	//  - 指定した時間が経過した後にステージの状態を更新
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateState( float waitForSeconds, State state )
	{
		Stage tmpStage = stage;
		
		// 指定した時間を待つ.
		yield return new WaitForSeconds( waitForSeconds );
		
		// ステージが変わっていない時のみ処理する.
		if ( tmpStage == stage )
		{
			// ステージの状態を更新.
			this.state = state;
		}
	}
	
	// ------------------------------------------------------------------------
	// DEBUG
	// ------------------------------------------------------------------------
	
	// ステージ名を返す.
	public string GetStage()
	{
		return stage.ToString();
	}
	
	// ステージを強制的に変更する.
	public void SetStage( string stage )
	{
		
		// ステージを設定.
		if ( stage == "START" )   {	this.stage = Stage.START; 	}
		if ( stage == "PATROL1" ) {	this.stage = Stage.PATROL1; }
		if ( stage == "ATTACK1" ) {	this.stage = Stage.ATTACK1; }
		if ( stage == "PATROL2" ) {	this.stage = Stage.PATROL2; }
		if ( stage == "ATTACK2" ) {	this.stage = Stage.ATTACK2; }
		if ( stage == "PATROL3" ) {	this.stage = Stage.PATROL3; }
		if ( stage == "ATTACK3" ) {	this.stage = Stage.ATTACK3; }
		if ( stage == "SILENCE" ) {	this.stage = Stage.SILENCE; }
		if ( stage == "BOSS" ) 	  {	this.stage = Stage.BOSS; 	}
		if ( stage == "GAMECLEAR" ){ this.stage = Stage.GAMECLEAR;}
		
		// ステージの状態は初期.
		this.state = State.INITIALIZE;
	}
	
	// レベル文字を返す.
	public string GetLevelText()
	{
		return level.ToString();
	}
	
	// レベルを返す.
	public int GetLevel()
	{
		return (int)level;
	}
	
	// レベルを保持する.
	public void SetLevel( int level )
	{
		this.level = (Level)level;
	}
}
