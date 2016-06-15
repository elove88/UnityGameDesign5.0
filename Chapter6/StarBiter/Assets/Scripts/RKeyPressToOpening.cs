using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// RKeyPressToOpening
//  - オープニングシーンを呼び出す.
//  - 使い方.
//    - 本スクリプトをメインカメラに付ける.
//  - 動き仕様.
//    - Rキー押下でオープニングシーンを呼び出す.
// ---------------------------------------------------------------------------
public class RKeyPressToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// ゲームシーン名.

	void Update () {
	
		// クリック.
		if ( Input.GetKeyDown(KeyCode.R) )
		{
			// ゲームシーンを呼び出す.
			Application.LoadLevel( gameSceneName );
		}
			
	}
}
