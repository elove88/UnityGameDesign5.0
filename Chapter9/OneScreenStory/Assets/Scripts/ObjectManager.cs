
using System.Collections.Generic;
using UnityEngine;


/// <summary>オブジェクト管理クラス</summary>
class ObjectManager : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>ゲーム開始時に非アクティブ状態にするオブジェクト</summary>
	/// ここに登録したオブジェクトをあらかじめ非アクティブにしておく必要はない (自動的に非アクティブ化する)
	/// アクティブ/非アクティブの切り替えをしないオブジェクトを登録する必要はない
	public BaseObject[] m_initialDeactiveObjects = null;

	/// <summary>アクティブ/非アクティブ切り替え時に表示するエフェクト</summary>
	public ParticleSystem m_switchingEffect = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		foreach ( BaseObject bo in m_initialDeactiveObjects )
		{
			deactivate( bo );
		}
	}


	//==============================================================================================
	// 公開メソッド

	/// <summary>オブジェクトを検索する</summary>
	/// 非アクティブなオブジェクトも検索できるようにしたメソッドである
	public BaseObject find( string name )
	{
		// まずはアクティブ状態のオブジェクトを探してみる
		GameObject go = GameObject.Find( name );
		BaseObject bo = ( go != null ) ? go.GetComponent< BaseObject >() : null;

		if ( bo == null )
		{
			// 見つからない場合は非アクティブオブジェクトを探してみる
			if ( !m_deactiveObjects.TryGetValue( name, out bo ) )
			{
				// 見つからなかった
				return null;
			}
		}

		// 見つかった
		return bo;
	}

	/// <summary>オブジェクトを非アクティブ状態にする</summary>
	/// 戻り値は非アクティブ状態にできたかどうか
	public bool deactivate( BaseObject baseObject )
	{
		BaseObject boInDictionary;
		if ( m_deactiveObjects.TryGetValue( baseObject.name, out boInDictionary ) )
		{
			// 既に同名のオブジェクトがある

			if ( baseObject == boInDictionary )
			{
				// まったく同じオブジェクト
				Debug.LogWarning( "\"" + baseObject.name + "\" has already deactivated." );

				baseObject.gameObject.SetActive( false );	// 念のため再度非アクティブに
				return true;
			}
			else
			{
				// 異なるオブジェクト
				Debug.LogError( "There is already a same name object in the dictionary." );
				return false;
			}
		}

		// 非アクティブにする
		baseObject.gameObject.SetActive( false );
		m_deactiveObjects.Add( baseObject.name, baseObject );
		return true;
	}

	/// <summary>オブジェクトをアクティブ状態にする</summary>
	/// 戻り値はアクティブ状態にできたかどうか
	public bool activate( BaseObject baseObject )
	{
		if ( m_deactiveObjects.ContainsKey( baseObject.name ) )
		{
			// アクティブにする
			baseObject.gameObject.SetActive( true );
			m_deactiveObjects.Remove( baseObject.name );

			return true;
		}

		Debug.LogWarning( "\"" + baseObject.name + "\" is NOT deactivated." );
		return false;
	}

	/// <summary>アクティブ/非アクティブ切り替え時に表示するエフェクトを再生</summary>
	public void playSwitchingEffect( BaseObject baseObject )
	{
		// キャラクターのちょっと手前に表示させるための計算
		Vector3 baseObjectCenter = baseObject.transform.position
		                         + 0.5f * ( baseObject.getYTop() + baseObject.getYBottom() ) * Vector3.up;
		Quaternion qt = Quaternion.AngleAxis( Camera.main.transform.eulerAngles.x, Vector3.right );
		Ray ray = new Ray( baseObjectCenter, qt * -Vector3.forward );

		// 再生
		m_switchingEffect.transform.position = ray.GetPoint( 100.0f );
		m_switchingEffect.Play();
	}


	//==============================================================================================
	// 非公開メンバ変数

	/// <summary>非アクティブ状態にあるオブジェクトの辞書</summary>
	private Dictionary< string, BaseObject > m_deactiveObjects = new Dictionary< string, BaseObject >();
}
