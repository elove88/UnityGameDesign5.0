
using UnityEngine;


/// <summary>evaluate-event コマンドのイベントアクター</summary>
class EventActorEvaluateEvent : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorEvaluateEvent( int eventIndex )
	{
		m_eventIndex = eventIndex;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		evman.setNextEvaluatingEventIndex( m_eventIndex );
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// すぐ終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>イベント終了後に継続して評価するイベントのインデックス</summary>
	private int m_eventIndex;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorEvaluateEvent CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			int eventIndex = manager.GetComponent< EventManager >().getEventIndexByName( parameters[ 0 ] );

			// アクターを生成
			EventActorEvaluateEvent actor = new EventActorEvaluateEvent( eventIndex );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
