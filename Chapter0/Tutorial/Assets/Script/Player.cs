using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	protected float	jump_speed = 12.0f;			// ジャンプの初速度.
	public bool		is_landing = false;			// 着地中？.

	// Use this for initialization
	void Start () {

		this.is_landing = false;	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// 着地中だったら…….
		if(this.is_landing) {

			// マウスの左ボタンが押されたら（押された瞬間）…….		
			if(Input.GetMouseButtonDown(0)) {

				// 着地フラグを false に（着地してない）.
				this.is_landing = false;

				// 上向きの速度でジャンプする.
				this.GetComponent<Rigidbody>().velocity = Vector3.up*this.jump_speed;

				// 左ボタンをクリックした瞬間（ジャンプの瞬間）に、
				// ゲームがポーズ上になる.
				//Debug.Break();
			}
		}	
	}

	// 他のゲームオブジェクトと衝突したとき.
	void OnCollisionEnter(Collision collision)
	{
		// 衝突相手のタグが "Floor" だったら…….
		if(collision.gameObject.tag == "Floor") {

			// 着地した.
			this.is_landing = true;
		}
	}
}
