using UnityEngine;
using System.Collections;

public class SyoujiPaperControl : MonoBehaviour {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		PAPER = 0,			//
		BROKEN,
		STEEL,
	};

	public STEP			step      = STEP.NONE;
	public STEP			next_step = STEP.NONE;
	public float		step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	private SceneControl	scene_control = null;

	public ShojiControl	shoji_control = null;

	public ShojiControl.HoleIndex	hole_index;

	public GameObject	paper_object = null;	// 紙.
	public GameObject	broken_object = null;	// やぶれ紙.
	public GameObject	steel_object = null;	// 鉄板.

	// Audio
	public AudioClip SUCCESS_SOUND = null;

	public int		through_count = 0;			// 穴が開いた後に通りぬけた回数.

	// ---------------------------------------------------------------- //

	// Use this for initialization
	void Start () {

		this.scene_control = GameObject.FindWithTag("MainCamera").GetComponent<SceneControl>();

		for(int i = 0;i < this.transform.childCount;i++) {

			GameObject	child = this.transform.GetChild(i).gameObject;

			switch(child.tag) {

				case "SyoujiPaper":
				{
					this.paper_object = child;
				}
				break;
				case "SyoujiPaperBroken":
				{
					this.broken_object = child;
				}
				break;
				case "SyoujiSteel":
				{
					this.steel_object = child;
				}
				break;

			}
		}

		this.paper_object.SetActive(false);
		this.broken_object.SetActive(false);
		this.steel_object.SetActive(false);

		this.next_step = STEP.PAPER;
	}
	
	// Update is called once per frame
	void Update () {
	
		// ---------------------------------------------------------------- //

		this.step_timer += Time.deltaTime;

		// ---------------------------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {

				case STEP.PAPER:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.PAPER:
				{
					this.paper_object.SetActive(true);
					this.broken_object.SetActive(false);
					this.steel_object.SetActive(false);
				}
				break;

				case STEP.BROKEN:
				{
					this.paper_object.SetActive(false);
					this.broken_object.SetActive(true);
					this.steel_object.SetActive(false);
	
					this.GetComponent<AudioSource>().PlayOneShot(SUCCESS_SOUND);
				}
				break;

				case STEP.STEEL:
				{
					this.paper_object.SetActive(false);
					this.broken_object.SetActive(false);
					this.steel_object.SetActive(true);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// ---------------------------------------------------------------- //
		// 各状態での実行処理.


		switch(this.step) {

			case STEP.PAPER:
			{
			}
			break;

		}
	}

	// 鉄板にする.
	public void	beginSteel()
	{
		this.next_step = STEP.STEEL;
	}

	// 鉄板？.
	public bool	isSteel()
	{
		bool	ret;

		ret = (this.step == STEP.STEEL);

		return(ret);
	}

	// 空振りカウンター（破れた後に通過した回数）をリセットする.
	public void	resetThroughCount()
	{
		this.through_count = 0;
	}

	// プレイヤーが当たった（通過した）ときに呼ばれる.
	public void	onPlayerCollided()
	{
		switch(this.step) {

			case STEP.PAPER:
			{
				this.next_step = STEP.BROKEN;

				this.scene_control.addComboCount();

				this.shoji_control.onPaperBreak();

				// 紙をやぶったときのエフェクト.
				this.scene_control.neko_control.effect_control.createBreakEffect(this, this.scene_control.neko_control);
			}
			break;

			case STEP.BROKEN:
			{
				this.through_count++;

				this.scene_control.clearComboCount();
			}
			break;
		}
	}
}
