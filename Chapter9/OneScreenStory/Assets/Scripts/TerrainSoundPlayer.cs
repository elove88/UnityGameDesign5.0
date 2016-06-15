
using UnityEngine;


/// <summary>Terrain と DraggableObject が衝突したときのサウンドを再生するクラス</summary>
class TerrainSoundPlayer : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>イベントマネージャオブジェクト</summary>
	public EventManager m_manager = null;

	/// <summary>水面エフェクト</summary>
	public ParticleSystem m_waterEffect = null;

	/// <summary>オブジェクトとの衝突</summary>
	private void OnCollisionEnter( Collision collision )
	{
		if ( m_manager.isExecutingEvents() ) return;  // イベント実行中は鳴らさない

		DraggableObject draggable = collision.gameObject.GetComponent< DraggableObject >();
		if ( draggable != null && GetComponent<AudioSource>() != null )
		{
			GetComponent<AudioSource>().Play();
		}

		// 水面エフェクトはここで再生
		if( m_waterEffect != null )
		{
			Vector3 position = draggable.transform.position;

			// 水面と同じ高さになるように
			position.y = 70.0f;
			// 水柱がキャラに隠れないよう少し前に出す
			position.z -= 70.0f;

			// 再生
			m_waterEffect.transform.position = position;
			m_waterEffect.Play();
		}
	}
}
