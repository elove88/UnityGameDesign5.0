using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 対象とするゲームオブジェクトを横方向にスクロール.
// ----------------------------------------------------------------------------
public class EndingSideScrollController : MonoBehaviour {

	public float scrollSpeed = 0.05f;				// スクロールするスピード.
	public float startPositionX = -0.65f;			// スクロール開始位置.
	public float startPositionY = -0.65f;			// スクロール開始位置.
	public float distanceToStartEaseIn = 0.05f;		// イーズインを開始する停止位置からの距離.
	public float triggerParentPositionY = 0;		// 親の位置を移動開始のトリガーとする.
	
	private bool isScrolling = false;
	private float stopPositionX;					// スクロール停止位置.
	private GameObject targetObject;				// スクロールするゲームオブジェクト.
	
	void Awake () 
	{
		// スクロールするゲームオブジェクトのインスタンスを取得.
		targetObject = this.gameObject;
		
		// スクロール停止位置を取得.
		stopPositionX = targetObject.transform.position.x;
		
		// ゲームオブジェクトを初期表示位置に移動.
		Vector3 tmpPosition = targetObject.transform.position;
		targetObject.transform.position = new Vector3( startPositionX, startPositionY, tmpPosition.z );
	}
	
	void FixedUpdate () 
	{	
		if ( !isScrolling )
		{
			if ( transform.parent.transform.position.y < triggerParentPositionY )
			{
				isScrolling = true;
			}
		}
		else
		{
			// 停止位置までスクロール.
			Vector3 position = targetObject.transform.position;
			
			if ( Mathf.Abs( stopPositionX - position.x ) < distanceToStartEaseIn )
			{
				// イーズイン.
				float additionDistance =
					( Mathf.Abs( stopPositionX - position.x ) / distanceToStartEaseIn )
						*  scrollSpeed * Time.deltaTime;
				position.x += additionDistance;
				position.y += additionDistance;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
			else if ( Mathf.Abs( stopPositionX - position.x ) > 0 )
			{
				// スクロール.
				float additionDistance = scrollSpeed * Time.deltaTime;
				position.x += additionDistance;
				position.y += additionDistance;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
		}
	}
}
