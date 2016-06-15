
using UnityEngine;


/// <summary>load コマンドのイベントアクター</summary>
class EventActorLoad : EventActor
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventActorLoad( string[] fileNames )
	{
		m_fileNames = fileNames;
	}

	/// <summary>アクターが生成された際に最初に実行されるメソッド</summary>
	public override void start( EventManager evman )
	{
		// イベントマネージャに次のスクリプトファイルを指定する
		// (実際にロードされるのは一連のイベントの実行が終わった後)
		evman.setNextScriptFiles( m_fileNames );
	}

	/// <summary>実行終了後にクリックを待つかどうかを返す</summary>
	public override bool isWaitClick( EventManager evman )
	{
		// すぐ終了
		return false;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>スクリプトファイル名</summary>
	private string[] m_fileNames;


	//==============================================================================================
	// 静的メソッド

	/// <summary>イベントアクターのインスタンスを生成する</summary>
	public static EventActorLoad CreateInstance( string[] parameters, GameObject manager )
	{
		if ( parameters.Length >= 1 )
		{
			// アクターを生成
			EventActorLoad actor = new EventActorLoad( parameters );
			return actor;
		}

		Debug.LogError( "Failed to create an actor." );
		return null;
	}
}
