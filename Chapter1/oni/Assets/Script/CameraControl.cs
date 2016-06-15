using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	// プレイヤー.
	private GameObject	player = null;

	public Vector3		offset;

	// Use this for initialization
	void Start () {

		// プレイヤーのインスタンスを探しておく.
		this.player = GameObject.FindGameObjectWithTag("Player");

		this.offset = this.transform.position - this.player.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		// プレイヤーと一緒に移動.
		this.transform.position = new Vector3(player.transform.position.x + this.offset.x, this.transform.position.y, this.transform.position.z);

	}
}
