
using System.Collections.Generic;
using UnityEngine;


/// <summary>条件変数のウォッチと変更の機能を提供するクラス</summary>
class DebugManager : MonoBehaviour
{
	//==============================================================================================
	// 内部データ型

	/// <summary>ウォッチする条件変数</summary>
	private struct WatchCV
	{
		public string target;     // キャラクター
		public string condition;  // 条件変数
	};


	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

#if UNITY_EDITOR
	/// <summary>フレーム毎更新メソッド</summary>
	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.P)) {

			Debug.Break();
		}
		if ( Input.GetKeyDown( KeyCode.W ) )
		{
			m_isActive = !m_isActive;
		}
	}

	/// <summary>GUI ハンドリングメソッド</summary>
	private void OnGUI()
	{
		if ( m_isActive )
		{
			displayWatchCVS();
		}
	}
#endif //UNITY_EDITOR


	//==============================================================================================
	// 公開メソッド

	/// <summary>ウォッチする条件変数を追加する</summary>
	public void addWatchConditionVariable( string target, string condition )
	{
		WatchCV cv;
		cv.target    = target;
		cv.condition = condition;

		// 既にリストになければ追加する
		if ( m_watchCVS.FindIndex( x => ( x.target == target && x.condition == condition ) ) < 0 )
		{
			m_watchCVS.Add( cv );
		}
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>ウォッチしている条件変数を表示する</summary>
	private void displayWatchCVS()
	{
		// 初期位置
		int x = 100;
		int y =  50;
		int w = 150;
		int h =  20;

		foreach ( WatchCV cv in m_watchCVS )
		{
			GameObject go = GameObject.Find( cv.target );
			BaseObject bo = go != null ? go.GetComponent< BaseObject >() : null;
			if ( bo != null )
			{
				string status = " ";
				string value = bo.getVariable( cv.condition );

				if(value == null) {
					// 条件変数が見つからなかった (未定義) とき
					status = "?";
					value  = "0";
				}

				// キャラクター名と条件変数名を表示
				GUI.Label( new Rect( x, y, w, h ), status + cv.target + " " + cv.condition );
				// 値を変更するためのテキストフィールド
				string newValue = GUI.TextField( new Rect( x + w, y, 50, h ), value );

				// 値が変更されたらゲーム内変数に反映
				if ( newValue != value )
				{
					bo.setVariable( cv.condition, newValue );
				}

				y += h;
			}
		}
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>条件変数のリスト</summary>
	private List< WatchCV > m_watchCVS = new List< WatchCV >();

	/// <summary>この機能がアクティブになっているかどうか</summary>
	private bool m_isActive = false;
}
