using UnityEngine;
using System.Collections;

public class Launcher : MonoBehaviour {

	// ボールのプレハブ.
	// 値はインスペクターでセットする.
	public GameObject	ballPrefab;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// マウスの左ボタンが押されたら（押された瞬間）…….		
		if(Input.GetMouseButtonDown(1)) {

			// ボールのプレハブをインスタンス化する（ゲームオブジェクトを作成する）.
			Instantiate(this.ballPrefab);
		}	
	}
}
