using UnityEngine;
using System.Collections;

// 『やりなおす』ボタン.
public class RetryButtonControl : MonoBehaviour {

	// 表示用簡易スプライト.
	public SimpleSprite	sprite = null;

	public Texture		texture;

	private GameControl	game_control;

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
		this.sprite = this.gameObject.AddComponent<SimpleSprite>();

		this.sprite.setTexture(this.texture);
		this.sprite.setSize(this.texture.width*0.02f/3.0f, this.texture.height*0.02f/3.0f);
		this.sprite.create();

		this.GetComponent<MeshCollider>().sharedMesh = this.GetComponent<MeshFilter>().mesh;

		this.game_control = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameControl>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
		this.game_control.OnRetryButtonPush();
	}
}
