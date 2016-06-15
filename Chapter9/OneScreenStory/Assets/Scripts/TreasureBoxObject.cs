
using UnityEngine;


/// <summary>宝箱オブジェクト専用クラス</summary>
class TreasureBoxObject : BaseObject
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>アニメーションの再生対象オブジェクト</summary>
	public GameObject m_animatingObject = null;

	/// <summary>イベントアクターから来たメッセージを処理する</summary>
	public override bool updateMessage( string message, string[] parameters )
	{
		if ( !m_isAnimated )
		{
			switch ( message )
			{
			// 開ける
			case "open":
				m_animatingObject.GetComponent<Animation>().Play( "M17_treasure_box_open" );
				m_isAnimated = true;
				return true;

			// 閉じる
			case "close":
				m_animatingObject.GetComponent<Animation>().Play( "M17_treasure_box_close" );
				m_isAnimated = true;
				return true;

			// その他
			default:
				return false;  // すぐ終了
			}
		}
		else
		{
			if ( m_animatingObject.GetComponent<Animation>().isPlaying )
			{
				return true;
			}
			else
			{
				// アニメーション終了
				m_isAnimated = false;
				return false;
			}
		}
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>アニメーション中かどうか</summary>
	private bool m_isAnimated = false;
}
