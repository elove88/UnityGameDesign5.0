using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// オープニング画面で表示するメッセージを画面外の下方向から上へスクロール.
// ----------------------------------------------------------------------------
public class OpeningInformationBoardController : MonoBehaviour {
	
	public float scrollSpeed = 0.2f;				// タイトルがスクロールするスピード.
	public float startPosition = -0.2f;				// スクロール開始位置.
	
	private float stopPositionY;					// タイトルが停止する位置.
	private GameObject informationBoard;			// メッセージ.
	
	void Start () 
	{
		// メッセージのインスタンスを取得.
		informationBoard = GameObject.FindGameObjectWithTag("InformationBoard");
		
		// スクロール停止位置を取得.
		stopPositionY = informationBoard.transform.position.y;
		
		// メッセージを初期表示位置に移動.
		Vector3 tmpPosition = informationBoard.transform.position;
		informationBoard.transform.position = new Vector3( tmpPosition.x, startPosition, 0 );
	}
	
	void Update () 
	{
		// 停止位置までスクロール.
		Vector3 position = informationBoard.transform.position;
		if ( position.y < stopPositionY )
		{
			position.y += scrollSpeed * Time.deltaTime;
			informationBoard.transform.position = new Vector3( position.x, position.y, 0 );
		}
	}
}
