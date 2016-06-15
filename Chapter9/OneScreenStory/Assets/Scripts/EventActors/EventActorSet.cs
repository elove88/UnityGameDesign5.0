
using UnityEngine;


/// <summary>set コマンドのイベントアクター</summary>
class EventActorSet : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorSet( BaseObject baseObject, string name, string value )
	{
		m_object = baseObject;
		m_name   = name;
		m_value  = value;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		// ゲーム内変数をセット
		m_object.setVariable( m_name, m_value );
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// すぐ終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>ゲーム内変数を操作するオブジェクト</summary>
	private BaseObject m_object;

	/// <summary>ゲーム内変数名</summary>
	private string m_name;

	/// <summary>値</summary>
	private string m_value;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorSet CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 3 )
		{
			// 指定されたオブジェクトを探す
			BaseObject bo = manager.GetComponent< ObjectManager >().find( parameters[ 0 ] );
			if ( bo != null )
			{
				// アクターを生成
				EventActorSet actor = new EventActorSet( bo, parameters[ 1 ], parameters[ 2 ] );
				return actor;
			}
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
