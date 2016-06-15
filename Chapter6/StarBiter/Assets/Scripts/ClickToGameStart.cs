using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// クリックでゲーム開始.
//  - 使い方.
//    - 本スクリプトをメインカメラに付ける.
//    - メインカメラにクリック音を付ける.
//  - 動き仕様.
//    - クリックでゲームシーンを呼び出す.
// ----------------------------------------------------------------------------
public class ClickToGameStart : MonoBehaviour {
	
	private string gameSceneName = "game";	// ゲームシーン名.
	private GameObject mainCamera;			// メインカメラ.
	
	void Start ()
	{	
		// メインカメラのインスタンスを取得.
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	void Update ()
	{	
		// マウス左ボタンクリック.
		if ( Input.GetButtonDown("Fire1") )
		{
			// ボタン押下時の音を再生.
			mainCamera.GetComponent<AudioSource>().Play();
			
			// ゲームシーンを呼び出す.
			Application.LoadLevel( gameSceneName );
		}
	}
}
