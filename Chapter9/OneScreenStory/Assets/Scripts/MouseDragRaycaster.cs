
using System.Collections;
using UnityEngine;


/// <summary>マウスドラッグのレイキャストを行うクラス</summary>
class MouseDragRaycaster : MonoBehaviour
{
	//==============================================================================================
	// MonoBehaviour 関連のメンバ変数・メソッド

	/// <summary>レイの衝突対象となるオブジェクトのレイヤー</summary>
	public LayerMask m_detectionLayer;

	/// <summary>イベントマネージャオブジェクト</summary>
	public EventManager m_eventManager = null;

	/// <summary>マジックハンドオブジェクト</summary>
	public GameObject	m_magicHand = null;
	public Vector3 		m_magicHandOffset = Vector3.zero;
	private bool 		m_isReleasing = false;

	/// <summary>マジックハンド表示/非表示切り替え時に表示するエフェクト</summary>
	public ParticleSystem m_magicHandEffect = null;

	/// <summary>スタートアップメソッド</summary>
	private void Start()
	{
		// フレームごとの処理はコルーチン内で行う
		StartCoroutine( "updateByCoroutine" );
	}


	//==============================================================================================
	// 非公開メソッド

	/// <summary>コルーチンによるフレーム毎更新メソッド</summary>
	private IEnumerator updateByCoroutine()
	{
		while ( true )
		{
			// 左ボタン押下待ち
			while ( !Input.GetMouseButtonDown( 0 ) || m_eventManager.isExecutingEvents() )
			{
				if ( m_isReleasing && !m_magicHand.GetComponentInChildren<Animation>().isPlaying )
				{
					m_magicHand.SetActive( false );
					m_isReleasing = false;
				}

				yield return 0;
			}

			// マウス位置にレイを飛ばしてオブジェクトを検出する
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if ( Physics.Raycast( ray, out hit, float.PositiveInfinity, m_detectionLayer.value ) )
			{
				// DraggableObject コンポーネントを取得
				DraggableObject draggable = hit.rigidbody.gameObject.GetComponent< DraggableObject >();

				if ( draggable != null )
				{
					// マジックハンドを表示・該当オブジェクトへ移動・持ち上げアニメーション再生
					Vector3 position = m_magicHandOffset + draggable.transform.position + draggable.getYTop() * Vector3.up;
					m_magicHand.SetActive( true );
					m_magicHand.transform.position = position;
					m_magicHand.GetComponentInChildren<Animation>().Play( "M20_magichand_pick" );

					// ドラッグ開始音を鳴らす
					GetComponent<AudioSource>().Play();

					// ドラッグ開始を通知
					draggable.onDragBegin( hit );

					// onDragBegin と onDragUpdate が同じフレームで起こらないようにここで yield を挟む
					yield return 0;

					// 左ボタンを押している間はドラッグ中であることを通知
					while ( Input.GetMouseButton( 0 ) )
					{
						// マジックハンド移動
						position = m_magicHandOffset + draggable.transform.position + draggable.getYTop() * Vector3.up;
						m_magicHand.transform.position = position;

						draggable.onDragUpdate();
						yield return 0;
					}

					// マジックハンドリリースアニメーション再生
					m_magicHand.GetComponentInChildren<Animation>().Play( "M20_magichand_releasing" );
					m_isReleasing = true;

					// ドラッグ終了を通知
					draggable.onDragEnd();
				}
			}

			// フレームを進める
			yield return 0;
		}
	}
}
