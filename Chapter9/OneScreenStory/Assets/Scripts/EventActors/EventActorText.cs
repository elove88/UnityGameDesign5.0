
using System;
using UnityEngine;


/// <summary>text コマンドのイベントアクター</summary>
class EventActorText : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorText( string text )
	{
		m_text = text;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		// 地の文を表示
		TextManager tad = evman.gameObject.GetComponent< TextManager >();
		tad.showText( m_text, new Vector2( 320.0f, 50.0f ), 50.0f, 10.0f, 15.0f );
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// 次のアクターが選択肢 choice のときはクリックを待たない
		Event ev = evman.getActiveEvent();
		if ( ev != null && ev.getNextKind() == "choice" )
		{
			return false;
		}

		// そうでなければ待つ
		return true;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>表示するテキスト</summary>
	private string m_text;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorText CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// アクターを生成
			EventActorText actor = new EventActorText( String.Join( "\n", parameters ) );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
