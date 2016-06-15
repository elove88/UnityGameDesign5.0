
using System;
using UnityEngine;


/// <summary>message コマンドのイベントアクター</summary>
class EventActorMessage : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorMessage( string message, BaseObject to, string[] parameters )
	{
		m_message    = message;
		m_to         = to;
		m_parameters = parameters;
	}

	/// <summary>アクターが破棄されるまで毎フレーム実行されるメソッド</summary>
	public override void execute( EventManager evman )
	{
		if ( !( m_to.updateMessage( m_message, m_parameters ) ) )
		{
			m_isDone = true;
		}
	}

	/// <summary>アクターで行うべき処理が終わったかどうかを返す</summary>
	public override bool isDone()
	{
		return m_isDone;
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick(EventManager evman)
	{
		// 終了タイミングはメッセージの宛先に委ねる
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>メッセージの種類</summary>
	private string m_message;

	/// <summary>メッセージの宛先</summary>
	private BaseObject m_to;

	/// <summary>メッセージのパラメータ</summary>
	private string[] m_parameters;

	/// <summary>アクターの処理が終了しているかどうか</summary>
	private bool m_isDone = false;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorMessage CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 2 )
		{
			// 指定されたオブジェクトを探す
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				string[] messageParams = new string[ parameters.Length - 2 ];
				Array.Copy( parameters, 2, messageParams, 0, messageParams.Length );

				// アクターを生成
				EventActorMessage actor = new EventActorMessage( parameters[ 1 ], bo, messageParams );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
