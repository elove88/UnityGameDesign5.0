
using System;
using UnityEngine;


/// <summary>イベント</summary>
class Event
{
	//==============================================================================================
	// 内部データ型

	/// <summary>アクターの実行状態</summary>
	private enum STEP
	{
		NONE = -1,
		EXEC_ACTOR = 0,  // アクターの実行中
		WAIT_INPUT,      // マウスクリック待ち
		DONE,            // 完了
		NUM
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public Event( string[] targets, EventCondition[] conditions, string[][] actions, bool isPrologue, bool doContinue, string name )
	{
		Array.Sort( targets );	// 後々の比較のために事前にソートしておく

		m_targets    = targets;
		m_conditions = conditions;
		m_actions    = actions;
		m_isPrologue = isPrologue;
		m_doContinue = doContinue;
		m_name       = name;
	}

	/// <summary>イベントを評価する</summary>
	public bool evaluate( string[] contactingObjects, bool isPrologue )
	{
		if ( isPrologue )
		{
			if ( !m_isPrologue ) return false;
		}
		else
		{
			// 発生対象オブジェクトと接触オブジェクトの比較
			Array.Sort( contactingObjects );

			if ( m_targets.Length == contactingObjects.Length )
			{
				for ( int i = 0; i < m_targets.Length; ++i )
				{
					// "*" のときは誰でもよい
					if ( m_targets[ i ] == "*" ) continue;

					if ( m_targets[ i ] != contactingObjects[ i ] )
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
		}

		// 発生条件のチェック
		foreach ( EventCondition ec in m_conditions )
		{
			if ( !ec.evaluate() ) return false;
		}

		return true;
	}

	/// <summary>イベントのスタートアップメソッド</summary>
	public void start()
	{
		m_step           = STEP.NONE;
		m_nextStep       = STEP.EXEC_ACTOR;
		m_currentActor   = null;
		m_nextActorIndex = 0;
	}

	/// <summary>イベントのフレーム毎更新メソッド</summary>
	public void execute( EventManager evman )
	{
		// ------------------------------------------------------------ //

		switch ( m_step ) {
			case STEP.WAIT_INPUT:
			{
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					m_currentActor = null;
					m_nextStep = STEP.EXEC_ACTOR;
				}
			}
			break;

			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor.isDone() )
				{
					// 入力待ちをする？
					if ( m_currentActor.isWaitClick( evman ) )
					{
						m_nextStep = STEP.WAIT_INPUT;
					}
					else
					{
						// しない場合はすぐに次のアクターへ
						m_nextStep = STEP.EXEC_ACTOR;
					}
				}
			}
			break;
		}

		// ------------------------------------------------------------ //

		while ( m_nextStep != STEP.NONE )
		{
			m_step     = m_nextStep;
			m_nextStep = STEP.NONE;

			switch ( m_step )
			{
				case STEP.EXEC_ACTOR:
				{
					m_currentActor = null;

					// 次に実行するアクターまでスクリプトの走査を進める
					while ( m_nextActorIndex < m_actions.Length )
					{
						m_currentActor = createActor( evman, m_nextActorIndex );

						++m_nextActorIndex;

						if ( m_currentActor != null )
						{
							break;
						}
					}

					if ( m_currentActor != null )
					{
						m_currentActor.start( evman );
					}
					else
					{
						// 実行するアクターがなかったらイベントのおしまい
						m_nextStep = STEP.DONE;
					}
				}
				break;
			}
		}

		// ------------------------------------------------------------ //

		switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				m_currentActor.execute( evman );
			}
			break;
		}
	}

	/// <summary>イベントの GUI ハンドリングメソッド</summary>
	public void onGUI( EventManager evman )
	{
#if UNITY_EDITOR
		// 現在実行中のアクターのテキストファイル中の行番号を表示する
		if ( m_currentActor != null )
		{
			if ( m_actionLineNumbers != null )
			{
				string text = "";
				text += m_actionLineNumbers[ m_nextActorIndex - 1 ];
				text += " :";
				text += m_currentActor.ToString();

				GUI.Label( new Rect( 10, 40, 200, 20 ), text );
			}
		}
#endif

		switch ( m_step )
		{
			case STEP.EXEC_ACTOR:
			{
				if ( m_currentActor != null )
				{
					m_currentActor.onGUI( evman );
				}
			}
			break;
		}
	}

	/// <summary>イベントが完了しているかどうかを返す</summary>
	public bool isDone()
	{
		return m_step == STEP.DONE;
	}

	/// <summary>このイベントの後継続して評価・実行が可能かどうかを返す</summary>
	public bool doContinue()
	{
		return m_doContinue;
	}

	/// <summary>このイベントの名前を取得する</summary>
	public string getEventName()
	{
		return m_name;
	}

	/// <summary>次に実行されるアクターの種類を取得する</summary>
	public string getNextKind()
	{
		string kind = "";

		if ( m_nextActorIndex < m_actions.Length )
		{
			kind = m_actions[ m_nextActorIndex ][ 0 ];
			kind = kind.ToLower();
		}

		return kind;
	}

	/// <summary>このイベントが記述されているスクリプトの行番号を取得する</summary>
	public int getLineNumber()
	{
		return m_lineNumber;
	}

	/// <summary>このイベントが記述されているスクリプトの行番号を設定する</summary>
	public void setLineNumber( int n )
	{
		m_lineNumber = n;
	}

	/// <summary>このイベントの各アクションが記述されているスクリプトの行番号を設定する</summary>
	public void setActionLineNumbers( int[] numbers )
	{
		m_actionLineNumbers = numbers;
	}

	/// <summary>現在のアクターの名前を設定する</summary>
	public void setCurrentActorName( string name )
	{
		m_actorName = name;
	}

	/// <summary>アクターの生成中のエラーを出力する</summary>
	public void debugLogError( string message )
	{
		Debug.LogError( m_actorName + ":" + message + " at " + m_actionLineNumbers[ m_actorIndex ] + "." );
	}

	/// <summary>イベントアクターを生成する</summary>
	public EventActor createActor( EventManager manager, int index )
	{
		string[] action     = m_actions[ index ];
		string   kind       = action[ 0 ];
		string[] parameters = new string[ action.Length - 1 ];
		Array.Copy( action, 1, parameters, 0, parameters.Length );

		m_actorName  = "";
		m_actorIndex = index;
		EventActor actor = null;

		switch ( kind.ToLower() )
		{
		// [evaluate-event]
		// イベントの終了時に指定したイベントを連続して実行する
		case "evaluate-event":
			actor = EventActorEvaluateEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [set]
		// ゲーム内変数に文字列を代入する
		case "set":
			actor = EventActorSet.CreateInstance( parameters, manager.gameObject );
			break;

		// [move]
		// オブジェクトを移動する
		case "move":
			actor = EventActorMove.CreateInstance( parameters, manager.gameObject );
			break;

		// [hide]
		// 指定されたオブジェクトを非表示にする
		case "hide":
			actor = EventActorVisibility.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [show]
		// 指定されたオブジェクトを表示する
		case "show":
			actor = EventActorVisibility.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [text]
		// 地の文を表示する
		case "text":
			actor = EventActorText.CreateInstance( parameters, manager.gameObject );
			break;

		// [dialog]
		// 会話文を表示する
		case "dialog":
			actor = EventActorDialog.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [choice]
		// 選択肢を表示して選んだものに応じてゲーム内変数に値を代入する
		case "choice":
			actor = EventActorChoice.CreateInstance( parameters, manager.gameObject );
			break;

		// [play]
		// サウンドを再生する
		case "play":
			actor = EventActorPlay.CreateInstance( parameters, manager.gameObject, this );
			break;

		// [fadeout]
		// フェードアウトを行う
		case "fadeout":
			actor = EventActorFading.CreateInstance( parameters, false, manager.gameObject );
			break;

		// [fadein]
		// フェードインを行う
		case "fadein":
			actor = EventActorFading.CreateInstance( parameters, true, manager.gameObject );
			break;

		// [load]
		// スクリプトを読み込んでイベントを入れ替える
		case "load":
			// ToDo: load コマンドがあったときは継続評価を打ち切る？ (現在は打ち切っていない)
			actor = EventActorLoad.CreateInstance( parameters, manager.gameObject );
			break;

		// [call-event]
		// コマンドが実行されるタイミングで強引に別のイベントを実行する
		case "call-event":
			actor = EventActorCallEvent.CreateInstance( parameters, manager.gameObject );
			break;

		// [message]
		// オブジェクトにメッセージを送信して固有の処理を行わせる
		case "message":
			actor = EventActorMessage.CreateInstance( parameters, manager.gameObject );
			break;

		default:
			Debug.LogError( "Invalid command \"" + kind + "\"" );
			break;
		}

		return actor;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>イベントの発生対象となるオブジェクト</summary>
	private string[]         m_targets;

	/// <summary>イベントの発生条件</summary>
	private EventCondition[] m_conditions;

	/// <summary>イベントで行うアクション</summary>
	private string[][]       m_actions;

	/// <summary>プロローグイベントフラグ</summary>
	private bool             m_isPrologue;

	/// <summary>イベントの継続評価フラグ</summary>
	private bool             m_doContinue;

	/// <summary>イベントの名前</summary>
	private string           m_name;

	/// <summary>現在のアクターの実行状態</summary>
	private STEP             m_step = STEP.NONE;

	/// <summary>次に遷移するアクターの実行状態</summary>
	private STEP             m_nextStep = STEP.EXEC_ACTOR;

	/// <summary>現在実行中のアクター</summary>
	private EventActor       m_currentActor = null;

	/// <summary>次に実行するアクターのインデックス</summary>
	private int              m_nextActorIndex = 0;

	/// <summary>最後に生成が試みられたアクターの名前</summary>
	private string           m_actorName = "";

	/// <summary>最後に生成が試みられたアクターのインデックス</summary>
	private int              m_actorIndex = 0;

	/// <summary>イベントが記述されているスクリプトの行番号</summary>
	private int              m_lineNumber = 0;

	/// <summary>イベントの各アクションが記述されているスクリプトの行番号</summary>
	private int[]            m_actionLineNumbers = null;
}
