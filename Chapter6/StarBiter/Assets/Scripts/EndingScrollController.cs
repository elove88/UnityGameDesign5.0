using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// 対象とするゲームオブジェクトを画面外の下方向から上へスクロール.
// ----------------------------------------------------------------------------
public class EndingScrollController : MonoBehaviour {

	public float scrollSpeed = 0.05f;				// スクロールするスピード.
	public float startPosition = -0.65f;			// スクロール開始位置.
	public float distanceToStartEaseIn = 0.05f;		// イーズインを開始する停止位置からの距離.
	public bool isStoppedStarScroll = true;
	
	private float stopPositionY;					// スクロール停止位置.
	private GameObject targetObject;				// スクロールするゲームオブジェクト.
	private GameObject endingSpace;
	private GameObject fadeOut;
	private bool isEaseIn = false;
	
	void Start () 
	{
		// スクロールするゲームオブジェクトのインスタンスを取得.
		targetObject = this.gameObject;
		
		// スクロール停止位置を取得.
		stopPositionY = targetObject.transform.position.y;
		
		// メッセージを初期表示位置に移動.
		Vector3 tmpPosition = targetObject.transform.position;
		targetObject.transform.position = new Vector3( tmpPosition.x, startPosition, tmpPosition.z );
	
		endingSpace = GameObject.Find("EndingSpace").gameObject;
		fadeOut = GameObject.Find("FadeOut").gameObject;
	}
	
	void FixedUpdate () 
	{	
		// 停止位置までスクロール.
		Vector3 position = targetObject.transform.position;
		
		if ( isEaseIn )
		{
			// イーズイン.
			position.y +=
				( Mathf.Abs( stopPositionY - position.y ) / distanceToStartEaseIn )
					*  scrollSpeed * Time.deltaTime;
			targetObject.transform.position = new Vector3( position.x, position.y, position.z );
		}
		else
		{
			if ( Mathf.Abs( stopPositionY - position.y ) < distanceToStartEaseIn )
			{
				// イーズイン開始.
				isEaseIn = true;
				if ( isStoppedStarScroll )
				{
					endingSpace.SendMessage("SetEaseIn");
					fadeOut.SendMessage("SetEnable");
				}
			}
			else
			{
				// スクロール.
				position.y += scrollSpeed * Time.deltaTime;
				targetObject.transform.position = new Vector3( position.x, position.y, position.z );
			}
		}
	}
}
