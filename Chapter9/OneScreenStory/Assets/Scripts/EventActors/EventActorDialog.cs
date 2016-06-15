
using System;
using UnityEngine;


/// <summary>dialog コマンドのイベントアクター</summary>
class EventActorDialog : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorDialog( BaseObject baseObject, string text )
	{
		m_object = baseObject;
		m_text   = text;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		// 会話文を表示
		TextManager textman = evman.GetComponent< TextManager >();
		textman.showDialog( m_object, m_text, 50.0f, 10.0f, 15.0f );
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

	/// <summary>会話文表示対象のオブジェクト</summary>
	private BaseObject m_object;

	/// <summary>表示するテキスト</summary>
	private string m_text;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorDialog CreateInstance( string[] parameters, GameObject manager, Event ev )
	{
		// これから作るアクターの名前をセットする (エラーメッセージ用)
		ev.setCurrentActorName( "EventActorDialog" );

		if ( parameters.Length >= 2 )
		{
			// 指定されたオブジェクトを探す
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// テキストを分離
				string[] text = new string[ parameters.Length - 1 ];
				Array.Copy( parameters, 1, text, 0, text.Length );

				// アクターを生成
				EventActorDialog actor = new EventActorDialog( bo, String.Join( "\n", text ) );
				return actor;
			}
			else
			{
				ev.debugLogError( "Can't find BaseObject(" + parameters[ 0 ] + ")" );
				return null;
			}
		}

		ev.debugLogError( "Out of Parameter" );
		return null;
	}
}
