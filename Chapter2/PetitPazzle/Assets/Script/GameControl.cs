using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		PLAY = 0,		// プレイ中.
		CLEAR,			// クリアー.

		NUM,
	};

	private STEP	step      = STEP.PLAY;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	// -------------------------------------------------------- //

	public GameObject		pazzlePrefab = null;

	public	PazzleControl	pazzle_control = null;

	private	SimpleSpriteGUI	finish_sprite = null;
	public	Texture			finish_texture = null;

	private RetryButtonControl	retry_button = null;

	// -------------------------------------------------------- //

	public enum SE {

		NONE = -1,

		GRAB = 0,		// ピースをつかんだとき.
		RELEASE,		// ピースを離したとき（正解じゃないとき）.

		ATTACH,			// ピースを離したとき（正解のとき）.

		COMPLETE,		// パズルが完成したときのジングル.

		BUTTON,			// GUI のボタン.

		NUM,
	};

	public AudioClip[]	audio_clips;

	// -------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.pazzle_control = (Instantiate(this.pazzlePrefab) as GameObject).GetComponent<PazzleControl>();

		// 『やりなおす』ボタンの描画順をセットする.
		// （ピースの後ろ）.

		this.retry_button = GameObject.FindGameObjectWithTag("RetryButton").GetComponent<RetryButtonControl>();

		this.retry_button.GetComponent<Renderer>().material.renderQueue = this.pazzle_control.GetDrawPriorityRetryButton();


		this.finish_sprite = new SimpleSpriteGUI();
		this.finish_sprite.create();
		this.finish_sprite.setTexture(this.finish_texture);
	}

	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 状態遷移チェック.

		switch(this.step) {

			case STEP.PLAY:
			{
				if(this.pazzle_control.isCleared()) {

					this.next_step = STEP.CLEAR;
				}
			}
			break;

			case STEP.CLEAR:
			{
				if(this.step_timer >this.audio_clips[(int)SE.COMPLETE].length + 0.5f) {

					if(Input.GetMouseButtonDown(0)) {

						Application.LoadLevel("TitleScene");
					}
				}
			}
			break;
		}


		// ---------------------------------------------------------------- //
		// 遷移時の初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.CLEAR:
				{
					this.retry_button.GetComponent<Renderer>().enabled = false;
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 実行処理.

		switch(this.step) {

			case STEP.PLAY:
			{
			}
			break;
		}
	}
	void	OnGUI()
	{
		if(pazzle_control.isDispCleard()) {

			this.finish_sprite.draw();
		}
	}

	public void	playSe(SE se)
	{
		this.GetComponent<AudioSource>().PlayOneShot(this.audio_clips[(int)se]);
	}

	// 『やりなおす』ボタンを押したときの処理.
	public void	OnRetryButtonPush()
	{
		if(!this.pazzle_control.isCleared()) {

			this.playSe(GameControl.SE.BUTTON);

			this.pazzle_control.beginRetryAction();
		}
	}
}
