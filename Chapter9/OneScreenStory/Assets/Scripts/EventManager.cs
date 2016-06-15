
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>ゲーム内イベント管理クラス</summary>
class EventManager : MonoBehaviour
{
	//==============================================================================================
	// 内部データ型

	/// <summary>イベントの実行状態</summary>
	private enum STEP
	{
		NONE = -1,
		LOAD_SCRIPT = 0,  // スクリプトファイルをロードする
		WAIT_TRIGGER,     // イベント発動待ち
		START_EVENT,      // イベント開始
		EXECUTE_EVENT,    // イベント実行
		NUM
	}


	//==============================================================================================
	// MonoBehaviour 関連のメソッド・メンバ変数

	/// <summary>ゲーム開始時に読み込むスクリプトのファイル名</summary>
	public string[] m_firstScriptFiles = new string[ 0 ];

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		// インスペクタでスクリプトを指定していない場合はタイトル画面で選択したものにする
		if ( m_firstScriptFiles.Length == 0 )
		{
			m_firstScriptFiles = GlobalParam.getInstance().getStartScriptFiles();
		}

		// メンバ初期化
		m_isPrologue = true;
		m_nextStep = STEP.LOAD_SCRIPT;
		m_nextScriptFiles = m_firstScriptFiles;
		m_nextEvaluatingEventIndex = -1;

		// サウンドマネージャを探しておく
		m_soundManager = GameObject.Find( "SoundManager" ).GetComponent< SoundManager >();
	}

	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		// ------------------------------------------------------------ //

		if ( m_nextStep == STEP.NONE )
		{
			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					// スクリプトファイルのロードが終わったら
					if ( m_hasCreatedEvents )
					{
						m_isExecuting = false;
						m_isPrologue = true;
						m_activeEvent = null;
						m_activeEventIndex = -1;
						m_nextEvaluatingEventIndex = -1;
						m_nextScriptFiles = null;

						m_nextStep = STEP.WAIT_TRIGGER;
					}
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					if ( m_isPrologue )
					{
						// プロローグイベントは無条件に発動
						m_nextStep = STEP.START_EVENT;
					}
					else
					{
						if ( m_contactingObjects.Count > 0 )  // 接触オブジェクトあり
						{
							m_isStartedByContact = true;
							m_nextStep = STEP.START_EVENT;
						}
					}
				}
				break;

				case STEP.START_EVENT:
				{
					// 次に実行できるイベントを探す

					// 配列で取り出しておく
					string[] contactingObjectsArray = m_contactingObjects.ToArray();

					// 前回実行した次のイベントから検索を開始する
					int i;
					for ( i = m_activeEventIndex + 1; i < m_events.Length; ++i )
					{
						Event ev = m_events[i];

						if ( ev.evaluate( contactingObjectsArray, m_isPrologue ) )
						{
							break;
						}
					}

					if ( i < m_events.Length )
					{
						// 次のイベントが見つかった

						m_activeEvent      = m_events[ i ];
						m_activeEventIndex = i;
						m_nextStep         = STEP.EXECUTE_EVENT;

						// イベント開始の SE (接触によって始まったイベントのときのみ)
						if ( m_isStartedByContact )
						{
							m_soundManager.playSE( "rpg_system05" );
						}
					}
					else
					{
						// 次のイベントが見つからなかった

						m_activeEvent      = null;
						m_activeEventIndex = -1;

						// 一通りの実行が終わったら、プロローグ終了
						m_isPrologue = false;

						if ( m_nextScriptFiles != null )
						{
							m_nextStep = STEP.LOAD_SCRIPT;
						}
						else
						{
							m_nextStep = STEP.WAIT_TRIGGER;
						}
					}
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					if ( m_activeEvent.isDone() )
					{
						// 地の文・会話文を消す
						GetComponent< TextManager >().hide();

						do
						{
							// 続けて評価するイベントが指定された場合 (evaluate-event)
							if ( m_nextEvaluatingEventIndex >= 0 )
							{
								Event ev = m_events[ m_nextEvaluatingEventIndex ];
								if ( ev.evaluate( m_contactingObjects.ToArray(), m_isPrologue ) )
								{
									m_activeEvent      = ev;
									m_activeEventIndex = m_nextEvaluatingEventIndex;
									m_nextStep         = STEP.EXECUTE_EVENT;
									break;
								}
							}

							if ( !m_activeEvent.doContinue() ) m_activeEventIndex = m_events.Length;

							m_nextStep = STEP.START_EVENT;
						}
						while ( false );

						m_nextEvaluatingEventIndex = -1;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.LOAD_SCRIPT:
				{
					m_isExecuting = false;
					m_hasCreatedEvents = false;

					// コルーチンでファイルの読み込みを開始する.
					StartCoroutine( "createEventsFromFile", m_nextScriptFiles );
				}
				break;

				case STEP.WAIT_TRIGGER:
				{
					m_isExecuting = false;

					// リストはクリア
					m_contactingObjects.Clear();
				}
				break;

				case STEP.EXECUTE_EVENT:
				{
					m_isExecuting = true;
					m_isStartedByContact = false;
					m_activeEvent.start();
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.execute( this );
				}
			}
			break;
		}
	}

	/// <summary>GUI ハンドリングメソッド</summary>
	private void OnGUI()
	{
#if UNITY_EDITOR
		if ( m_activeEvent != null )
		{
			GUI.Label( new Rect( 10, 10, 200, 20 ), m_activeEvent.getLineNumber().ToString() );
		}
#endif //UNITY_EDITOR

		switch ( m_step )
		{
			case STEP.EXECUTE_EVENT:
			{
				if ( m_activeEvent != null )
				{
					m_activeEvent.onGUI( this );
				}
			}
			break;
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>接触オブジェクトを追加する</summary>
	public void addContactingObject( BaseObject baseObject )
	{
		string name = baseObject.name;
		if ( !m_contactingObjects.Contains( name ) )
		{
			m_contactingObjects.Add( name );
		}
	}

	/// <summary>イベントの実行中かどうかを返す</summary>
	public bool isExecutingEvents()
	{
		return m_isExecuting;
	}

	/// <summary>実行中のイベントを取得する</summary>
	public Event getActiveEvent()
	{
		return m_activeEvent;
	}

	/// <summary>イベントのインデックスを取得する</summary>
	public int getEventIndexByName( string eventName )
	{
		return Array.FindIndex( m_events, x => x.getEventName() == eventName );
	}

	/// <summary>次に読み込むスクリプトファイルを設定する</summary>
	public void setNextScriptFiles( string[] fileNames )
	{
		m_nextScriptFiles = fileNames;
	}

	/// <summary>イベント終了後に続けて評価するイベントのインデックスを設定する (evaluate-event)</summary>
	public void setNextEvaluatingEventIndex( int eventIndex )
	{
		m_nextEvaluatingEventIndex = eventIndex;
	}

	/// <summary>イベントを強引に開始する (call-event)</summary>
	public void startEvent( int eventIndex )
	{
		m_activeEvent      = m_events[ eventIndex ];
		m_activeEventIndex = eventIndex;
		m_nextStep         = STEP.EXECUTE_EVENT;
	}

	/// <summary>サウンドマネージャーを返す</summary>
	public SoundManager getSoundManager()
	{
		return m_soundManager;
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>ファイルからイベントを生成する</summary>
	private IEnumerator createEventsFromFile( string[] fileNames )
	{
#if UNITY_EDITOR
		if ( fileNames.Length > 0 )
		{
#endif //UNITY_EDITOR

			// すべてのファイルの行データを保持するリスト
			List< string > linesOfAllFiles = new List< string >();

			foreach ( string file in fileNames )
			{
				// ファイルデータを読み込むコルーチンをチェイン
				yield return StartCoroutine( loadFile( file, linesOfAllFiles ) );
			}

			// パースしてイベントの配列を作る
			ScriptParser parser = new ScriptParser();
			m_events = parser.parseAndCreateEvents( linesOfAllFiles.ToArray() );
			Debug.Log( "Created " + m_events.Length.ToString() + " events." );

#if UNITY_EDITOR
		}
		else
		{
			// イベントを空にしておく
			m_events = new Event[ 0 ];
		}
#endif //UNITY_EDITOR

		// イベント作成済み
		m_hasCreatedEvents = true;
	}

	/// <summary>ファイルを読み込む</summary>
	private IEnumerator loadFile( string fileName, List< string > allLines )
	{
		string[] lines;

		if ( Application.isWebPlayer )
		{
			// WebPlayer ではセキュリティの関係上 File.ReadAllLines() が使用できないので
			// WWW オブジェクトを使ってイベント記述ファイルを読み込む

			WWW www = new WWW( fileName );
			yield return www;	// レスポンスが来ると実行が再開される

			// 改行で区切る
			lines = www.text.Split( '\r', '\n' );
		}
		else
		{
			if ( !File.Exists( fileName ) )
			{
				DebugPrint.setLocate( 10, 10 );
				DebugPrint.print( "File Open Error " + fileName, -1.0f );
			}

			// WebPlayer 以外は System.IO.File クラスを使ってさくっと読み込む
			lines = File.ReadAllLines( fileName );
		}

		allLines.AddRange( lines );
	}

	public static EventManager	get()
	{
		if(instance == null) {

			GameObject	go = GameObject.FindGameObjectWithTag("System");

			if(go == null) {

				Debug.Log("Can't find \"System\" GameObject.");

			} else {

				instance = go.GetComponent<EventManager>();
			}
		}
		return(instance);
	}
	protected static EventManager	instance = null;

	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>イベント作成済みフラグ</summary>
	private bool m_hasCreatedEvents = false;

	/// <summary>イベントの情報を保持するオブジェクト</summary>
	private Event[] m_events = new Event[ 0 ];

	/// <summary>プロローグイベントの評価・実行フラグ</summary>
	private bool m_isPrologue = false;

	/// <summary>接触オブジェクトのリスト</summary>
	private List< string > m_contactingObjects = new List< string >();

	/// <summary>イベントの実行中かどうか</summary>
	private bool m_isExecuting = false;

	/// <summary>現在の状態</summary>
	private STEP m_step = STEP.NONE;

	/// <summary>次に遷移する状態</summary>
	private STEP m_nextStep = STEP.NONE;

	/// <summary>実行中のイベント</summary>
	private Event m_activeEvent = null;

	/// <summary>実行中のイベントのインデックス</summary>
	private int m_activeEventIndex = -1;

	/// <summary>次に読み込むスクリプトファイル</summary>
	private string[] m_nextScriptFiles = null;

	/// <summary>接触によって始まったイベントかどうか</summary>
	private bool m_isStartedByContact = false;

	/// <summary>イベント終了後に続けて評価するイベントのインデックス (evaluate-event)</summary>
	private int m_nextEvaluatingEventIndex = -1;

	/// <summary>サウンドマネージャオブジェクト</summary>
	private SoundManager m_soundManager = null;
}
