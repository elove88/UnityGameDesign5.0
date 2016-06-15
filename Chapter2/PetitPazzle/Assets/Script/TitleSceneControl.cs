using UnityEngine;
using System.Collections;

public class TitleSceneControl : MonoBehaviour {

	enum STEP {

		NONE = -1,

		WAIT = 0,		// クリックまち中.
		PLAY_JINGLE,	// スタート音再生中.

		NUM,
	};

	private STEP	step      = STEP.WAIT;
	private STEP	next_step = STEP.NONE;

	private float		step_timer = 0.0f;

	// -------------------------------------------------------- //

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 状態遷移チェック.

		switch(this.step) {

			case STEP.WAIT:
			{
				if(Input.GetMouseButtonDown(0)) {

					this.next_step = STEP.PLAY_JINGLE;
				}
			}
			break;

			case STEP.PLAY_JINGLE:
			{
				// SE の再生が終わったら、ゲームシーンをロードして終了.

				if(this.step_timer > this.GetComponent<AudioSource>().clip.length + 0.5f) {

					Application.LoadLevel("GameScene0");
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 遷移時の初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PLAY_JINGLE:
				{
					this.GetComponent<AudioSource>().Play();
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

			case STEP.WAIT:
			{
			}
			break;
		}

	}

	void OnGUI()
	{
	}
}
