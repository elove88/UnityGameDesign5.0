
/// <summary>イベントの発生条件</summary>
class EventCondition
{
	//==============================================================================================
	// 公開メソッド

	/// <summary>コンストラクタ</summary>
	public EventCondition( BaseObject baseObject, string name, string compareValue )
	{
		m_object       = baseObject;
		m_name         = name;
		m_compareValue = compareValue;
	}

	/// <summary>発生条件を評価する</summary>
	public bool evaluate()
	{
		string value = m_object.getVariable( m_name );
		if ( value == null ) {
			value = "0";
		}

		return m_compareValue == value;
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>ゲーム内変数を参照するオブジェクト</summary>
	private BaseObject m_object;

	/// <summary>ゲーム内変数名</summary>
	private string m_name;

	/// <summary>比較値</summary>
	private string m_compareValue;
}
