using UnityEngine;
using System.Collections;

public class CarryBlock : Block {

	public Vector3		position_offset;

	public PlayerControl	player = null;

	// ぽい捨て時の位置.
	public StackBlock.PlaceIndex	place;

	public StackBlock.PlaceIndex	org_place;

	public enum STEP {

		NONE = -1,

		HIDE = 0,				// 非表示.
		CARRY_UP,				// 持ち上げ中（移動中）.
		CARRY,					// 持ち上げられ中（移動がおわった）.
		DROP_DOWN,				// ぽい捨て中.

		NUM,
	};

	public STEP		step       = STEP.NONE;
	public STEP		next_step  = STEP.NONE;

	public float	step_timer = 0.0f;

	// ---------------------------------------------------------------- //

	public bool	isMoving()
	{
		bool	ret = false;

		switch(this.step) {

			case STEP.CARRY_UP:
			case STEP.DROP_DOWN:
			{
				ret = true;
			}
			break;
		}

		return(ret);
	}

	void 	Start()
	{
		this.position_offset = Vector3.zero;

		this.next_step = STEP.HIDE;
	}
	
	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		// 状態遷移のチェック.

		if(this.next_step == STEP.NONE) {

			switch(this.step) {
	
				case STEP.CARRY_UP:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.next_step = STEP.CARRY;
					}
				}
				break;
	
				case STEP.DROP_DOWN:
				{
					if(this.position_offset.y == 0.0f) {
	
						this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
	
						this.next_step = STEP.HIDE;
					}
				}
				break;
			}
		}

		// 状態遷移時の初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {

				case STEP.HIDE:
				{
					this.GetComponent<Renderer>().enabled = false;
				}
				break;

				case STEP.CARRY_UP:
				{
					// 非表示状態から始まったときは、現在位置を求めておく.
					if(this.step == STEP.HIDE) {

						this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
					}

					Vector3	base_position = this.player.transform.position;

					base_position.y += Block.SIZE_Y;

					this.position_offset = this.transform.position - base_position;
			
					this.setVisible(true);
				}
				break;

				case STEP.DROP_DOWN:
				{
					Vector3	base_position = StackBlockControl.calcIndexedPosition(this.place);

					this.position_offset = this.transform.position - base_position;
				}
				break;
			}

			this.step = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// 各状態の実行.

		Vector3		position = this.transform.position;

		switch(this.step) {

			case STEP.CARRY:
			case STEP.CARRY_UP:
			{
				position.x = this.player.transform.position.x;
				position.y = this.player.transform.position.y + Block.SIZE_Y;
				position.z = 0.0f;
			}
			break;

			case STEP.DROP_DOWN:
			{
				position = StackBlockControl.calcIndexedPosition(this.place);
			}
			break;
		}

		// オフセットを補間する.

		if(Mathf.Abs(this.position_offset.y) < 0.1f) {

			this.position_offset.y = 0.0f;

		} else {

			const float		speed = 0.2f;

			if(this.position_offset.y > 0.0f) {

				this.position_offset.y -=  speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Max(this.position_offset.y, 0.0f);

			} else {

				this.position_offset.y -= -speed*(60.0f*Time.deltaTime);

				this.position_offset.y = Mathf.Min(this.position_offset.y, 0.0f);
			}
		}

		position.y += this.position_offset.y;

		this.transform.position = position;
	}

	// 持ち上げ動作を始める。
	public void		startCarry(int place_index_x)
	{
		// ドロップ中にブロックを持ち上げられたときは、一旦着地したときの
		// 処理を行う.
		// そうしないと、最上段のブロックが非表示のままになっちゃうから.
		// （ドロップ中はキャリーブロックが着地するまで、最上段のブロックは
		// 　非表示になっているから）.
		if(this.step == STEP.DROP_DOWN) {

			this.player.scene_control.stack_control.endDropBlockAction(this.place.x);
		}

		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.org_place = this.place;

		this.next_step = STEP.CARRY_UP;
	}

	// ぽい捨て動作を始める.
	public void		startDrop(int place_index_x)
	{
		this.place.x = place_index_x;
		this.place.y = StackBlockControl.GROUND_LINE;

		this.next_step = STEP.DROP_DOWN;
	}

	// 非表示にする.
	// （ケーキを食べた後）.
	public void		startHide()
	{
		this.next_step = STEP.HIDE;
	}
}
