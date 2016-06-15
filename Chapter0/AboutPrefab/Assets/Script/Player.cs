using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	private bool	is_landed;				// 着地中？.
	public float	JumpHeight = 4.0f;		// ジャンプの高さ.

	// ---------------------------------------------------------------- //

	void Start ()
	{
		this.is_landed = false;
	}

	void Update ()
	{
		// 着地中だったら…….
		if(this.is_landed) {

			// マウスの右ボタンがクリックされたら…….
			if(Input.GetMouseButtonDown(0)) {

				this.is_landed = false;
	
				// ジャンプの高さから、初速度を求める.
				float	y_speed = Mathf.Sqrt(2.0f*Mathf.Abs(Physics.gravity.y)*this.JumpHeight);

				this.GetComponent<Rigidbody>().velocity = Vector3.up*y_speed;
			}
		}
	}

	// 引数まであっていないとちゃんと呼び出されないので注意.
	void OnCollisionEnter(Collision collision)
	{
		// Debug.Log の使い方
		// どんなオブジェクトも Debug.Log するときは "ToString()" メソッドで
		// float なども ToString() で.
		Debug.Log(collision.gameObject.ToString());

		// これをやらないと、ボールとヒットしたときに this.is_landed が true になってしまう）.
		if(collision.gameObject.tag == "Floor") {

			this.is_landed = true;
		}
	}

	// 着地中？.
	public bool	isLanded()
	{
		return(this.is_landed);
	}
}
