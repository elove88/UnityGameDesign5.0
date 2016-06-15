using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// ClickToOpening
//  - オープニングシーンを呼び出す.
//  - 使い方.
//    - 本スクリプトをメインカメラに付ける.
//  - 動き仕様.
//    - クリックでオープニングシーンを呼び出す.
// ---------------------------------------------------------------------------
public class ClickToOpening : MonoBehaviour {

	private string gameSceneName = "opening";	// ゲームシーン名.

	void Update ()
	{
		// クリック.
		if ( Input.GetButtonDown("Fire1") ) {
			// ゲームシーンを呼び出す.
			Application.LoadLevel( gameSceneName );
		}
			
	}
}
