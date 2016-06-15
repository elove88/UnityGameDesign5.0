using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	// プレイヤー.
	private GameObject player = null;

	public Vector3 offset;

	public Vector3	interest;

	public Vector3	player_position_prev;

	// Use this for initialization
	void Start () {
		 
		// プレイヤーのインスタンスを探しておく.
		this.player = GameObject.FindGameObjectWithTag("NekoPlayer");
		 
		this.offset = this.transform.position - this.player.transform.position;

		this.interest = new Vector3(0.0f, this.transform.position.y - this.player.transform.position.y, 0.0f);

		this.player_position_prev = this.player.transform.position;
	}
	
	// Update is called once per frame
	void	Update () {

		// プレイヤーと一緒に移動.
		this.transform.position = new Vector3(player.transform.position.x + this.offset.x, player.transform.position.y + this.offset.y, player.transform.position.z + this.offset.z);
	}
}
