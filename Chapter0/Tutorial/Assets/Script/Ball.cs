using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		this.GetComponent<Rigidbody>().velocity = new Vector3(-10.0f, 9.0f, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// 他のゲームオブジェクトが画面外へ出たとき.
	void OnBecameInvisible()
	{
		// ゲームオブジェクトを削除する.
		// 引数に注意！
		// Destory(this) ではコンポーネントだけが削除されてしまう.
		Destroy(this.gameObject);
	}
}
