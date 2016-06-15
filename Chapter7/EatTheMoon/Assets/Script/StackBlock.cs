using UnityEngine;
using System.Collections;

// 下に積まれているブロック.
public class StackBlock : Block {

	public StackBlockControl	stack_control = null;

	// 状態.
	public enum STEP {
		
		NONE = -1,
	
		IDLE = 0,		// 通常.
		VANISHING,		// 消えるアニメーション中（カラー変化）.
		VACANT,			// 空っぽ（連鎖で消えて、灰色になっている）.
		FALL,			// 落下中.

		NUM,
	};
	
	public STEP		step;
	public STEP		next_step = STEP.NONE;
	public float	step_timer;

	// グリッド上の場所
	public struct PlaceIndex {

		public int		x;
		public int		y;
	};

	public PlaceIndex	place;
	public Vector3		position_offset;
	public Vector3		velocity;

	public RotateAction		swap_action;							// 上下入れ替え時の動きの制御.
	public RotateAction		color_change_action;					// カラーチェンジ時の動きの制御.

	public static float		FALL_START_HEIGHT = 6.5f;

	public static float		OFFSET_REVERT_SPEED = 0.1f*60.0f;		// オフセットが０に向かうスピード.

	public bool		shake_is_active;
	public float	shake_timer;
	public Vector3	shake_offset;

	// ---------------------------------------------------------------- //


	void 	Start()
	{
		this.setColorType(this.color_type);

		this.position_offset = Vector3.zero;

		// 回転動作の初期化.

		this.swap_action.init();
		this.color_change_action.init();

		this.shake_is_active = false;
	}

	// from_block から色や位置などをコピーする.
	public void	relayFrom(StackBlock from_block)
	{
		this.setColorType(from_block.color_type);

		this.step        = from_block.step;
		this.next_step   = from_block.next_step;
		this.step_timer  = from_block.step_timer;
		this.swap_action = from_block.swap_action;
		this.color_change_action = from_block.color_change_action;

		this.velocity = from_block.velocity;

		// グローバルの位置がかわらないよう、オフセットを計算する.
		this.position_offset = StackBlockControl.calcIndexedPosition(from_block.place) + from_block.position_offset - StackBlockControl.calcIndexedPosition(this.place);

		//this.position_offset = from_block.transform.position - StackBlockControl.calcIndexedPosition(this.place);
		// 上こちらにすると、回転の中心をずらしたことの影響を受けてしまうのでだめ.
	}

	// ブロック上下交換のアクションを開始する.
	static public void		beginSwapAction(StackBlock upper_block, StackBlock under_block)
	{
		Block.COLOR_TYPE	upper_color;
		StackBlock.STEP		upper_step;
		RotateAction		upper_color_change;

		upper_color        = upper_block.color_type;
		upper_step         = upper_block.step;
		upper_color_change = upper_block.color_change_action;

		upper_block.setColorType(under_block.color_type);
		upper_block.step                = under_block.step;
		upper_block.color_change_action = under_block.color_change_action;

		under_block.setColorType(upper_color);
		under_block.step                = upper_step;
		under_block.color_change_action = upper_color_change;

		// 回転動作を開始する.
		upper_block.swap_action.start(RotateAction.TYPE.SWAP_UP);
		under_block.swap_action.start(RotateAction.TYPE.SWAP_DOWN);
	}

	void 	Update()
	{
		this.step_timer += Time.deltaTime;

		const float	vanish_time = 1.0f;

		// -------------------------------------------- //
		// 次の状態に移るかどうかを、チェックする.

		switch(this.step) {

			case STEP.VANISHING:
			{
				if(this.step_timer > vanish_time) {

					this.next_step = STEP.VACANT;
				}
			}
			break;

			case STEP.FALL:
			{
				// 着地したらおしまい.
				if(this.position_offset.y == 0.0f) {

					this.next_step = STEP.IDLE;
				}
			}
			break;
		}

		// -------------------------------------------- //
		// 状態が遷移したときの初期化.

		if(this.next_step != STEP.NONE) {

			switch(this.next_step) {
	
				case STEP.VACANT:
				{
					this.setColorType(COLOR_TYPE.GRAY);
				}
				break;

				case STEP.FALL:
				{
					this.velocity = Vector3.zero;
				}
				break;

				case STEP.VANISHING:
				{
					this.shake_start();

					this.stack_control.scene_control.vanish_fx_control.createEffect(this);
				}
				break;
			}

			this.step      = this.next_step;
			this.next_step = STEP.NONE;

			this.step_timer = 0.0f;
		}

		// -------------------------------------------- //
		// 各状態での実行処理.

		switch(this.step) {

			case STEP.VANISHING:
			{
				// ブロックの色を
				//
				// 元の色→赤→灰色
				//
				// に変える.

				float	rate;

				if(this.step_timer < vanish_time*0.1f) {

					rate = this.step_timer/(vanish_time*0.1f);

				} else if(this.step_timer < vanish_time*0.3f) {

					rate = 1.0f;

				} else if(this.step_timer < vanish_time*0.6f) {

					this.setColorType(COLOR_TYPE.RED);

					rate = (this.step_timer - vanish_time*0.3f)/(vanish_time*0.3f);

				} else {

					rate = 1.0f;
				}

				this.GetComponent<Renderer>().material.SetFloat("_BlendRate", rate);
			}
			break;

		}

		// -------------------------------------------------------------------------------- //
		// マス目上の位置（常に固定）、回転は０で初期化.

		this.transform.position = StackBlockControl.calcIndexedPosition(this.place);
		this.transform.rotation = Quaternion.identity;

		// -------------------------------------------- //
		// スライド（オフセットの補間）.

		if(this.step == STEP.FALL) {

			this.velocity.y += -9.8f*Time.deltaTime;

			this.position_offset.y += this.velocity.y*Time.deltaTime;

			if(this.position_offset.y < 0.0f) {

				this.position_offset.y = 0.0f;
			}

			// 下にあるブロックを追い抜いてしまわないように.
			// （処理の順番が下→上とも限らないので、厳密ではない）.
			//
			if(this.place.y < StackBlockControl.BLOCK_NUM_Y - 1) {

				StackBlock	under = this.stack_control.blocks[this.place.x, this.place.y + 1];

				if(this.position_offset.y < under.position_offset.y) {

					this.position_offset.y = under.position_offset.y;
					this.velocity.y        = under.velocity.y;
				}
			}

		} else {

			float	position_offset_prev = this.position_offset.y;

			if(Mathf.Abs(this.position_offset.y) < 0.1f) {

				// オフセットが十分小さくなったらおしまい.
	
				this.position_offset.y = 0.0f;
	
			} else {

				if(this.position_offset.y > 0.0f) {
	
					this.position_offset.y -=  OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Max(0.0f, this.position_offset.y);
	
				} else {
	
					this.position_offset.y -= -OFFSET_REVERT_SPEED*Time.deltaTime;
					this.position_offset.y = Mathf.Min(0.0f, this.position_offset.y);
				}
			}

			// 上から落ちてくるブロックがぶつかったときのために、速度を計算しておく.
			this.velocity.y = (this.position_offset.y - position_offset_prev)/Time.deltaTime;
		}

		this.transform.Translate(this.position_offset);

		// -------------------------------------------- //
		// スワップ動作.

		this.swap_action.execute(this);

		// ケーキは回転しない.
		if(this.isCakeBlock()) {

			this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0.0f);

			this.transform.rotation = Quaternion.identity;
		}

		// -------------------------------------------- //
		// カラーチェンジ.

		this.color_change_action.execute(this);

		if(this.color_change_action.is_active) {

			// 半周回ったところで色が変わる.

			if(this.color_change_action.rate > 0.5f) {

				this.setColorType(this.color_change_action.target_color);
			}
		}

		// -------------------------------------------- //
		// ブロックが消えるときの振動.

		this.shake_execute();
	}
#if false
	// マウスボタンが押されたとき.
	// （使用するときは、StackBlockPrefab. の BoxCollider を有効にしてください）.
	void 	OnMouseDown()
	{
		// クリックされたら色が変わる（デバッグ用）.

		if(this.step == STEP.IDLE) {

			/*COLOR_TYPE	color = this.color_type;

			color = (COLOR_TYPE)(((int)color + 1)%Block.NORMAL_COLOR_NUM);

			this.setColorType(color);*/
			/*if(this.color_type == Block.COLOR_TYPE.PINK) {

				this.setColorType(Block.COLOR_TYPE.CYAN);

			} else {

				this.setColorType(Block.COLOR_TYPE.PINK);
			}*/
			this.stack_control.block_feeder.cake.x = this.place.x;
			this.stack_control.block_feeder.cake.is_enable = true;
			this.setColorType(Block.COLOR_TYPE.CAKE0);
		}
	}
#endif
	// 通常動作を開始する.
	public void beginIdle(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.next_step = STEP.IDLE;
	}

	// 消える動作を開始する.
	public void	beginVanishAction()
	{
		this.next_step = STEP.VANISHING;
	}

	// 落下動作を開始する.
	public void	beginFallAction(Block.COLOR_TYPE color_type)
	{
		this.setColorType(color_type);
		this.setVisible(true);

		this.position_offset.y = FALL_START_HEIGHT;
		this.velocity = Vector3.zero;

		this.next_step = STEP.FALL;
	}

	// 色がかわる動作を始める.
	public void	beginColorChangeAction(Block.COLOR_TYPE	color_type)
	{
		this.color_change_action.target_color = color_type;
		this.color_change_action.start(RotateAction.TYPE.COLOR_CHANGE);
	}

	// 連鎖チェックの対象となる？.
	public bool isConnectable()
	{
		bool	ret;

		ret = false;

		if(this.step == STEP.IDLE || this.next_step == STEP.IDLE) {

			ret = true;
		}

		if(this.color_type < Block.NORMAL_COLOR_FIRST || Block.NORMAL_COLOR_LAST < this.color_type) {

			ret = false;
		}

		// 最下段は連鎖チェックの対象外（画面外に出ているので）.
		if(this.place.y >= StackBlockControl.BLOCK_NUM_Y - 1) {

			ret = false;
		}

		return(ret);
	}

	// からっぽのブロック？（連鎖で消えた後）.
	public bool isVacant()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);

		return(ret);
	}

	// からっぽのブロック？（連鎖で消えた後）.
	public bool isCarriable()
	{
		bool	ret;

		do {

			ret = false;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = true;

		} while(false);

		return(ret);
	}

	// 入れ替え動作中？.
	public bool isNowSwapAction()
	{
		bool	ret = false;

		ret = this.swap_action.is_active;

		return(ret);
	}

	// 落下中？.
	public bool isNowFallAction()
	{
		bool	ret = (this.step == STEP.FALL || this.next_step == STEP.FALL);

		return(ret);
	}

	// 消えた後？.
	public bool	isVanishAfter()
	{
		bool	ret;

		do {

			ret = true;

			//

			if((this.step == STEP.VANISHING || this.next_step == STEP.VANISHING)) {

				break;
			}
			if((this.step == STEP.VACANT || this.next_step == STEP.VACANT)) {

				break;
			}

			//

			ret = false;

		} while(false);


		return(ret);
	}
	// 未使用中にする.
	public void	setUnused()
	{
		this.setColorType(Block.COLOR_TYPE.NONE);
		this.setVisible(false);
	}

	// ---------------------------------------------------------------- //

	// 振動スタート.
	private void	shake_start()
	{
		this.shake_is_active = true;
		this.shake_timer = 0.0f;
	}

	// 振動のコントロール.
	private void	shake_execute()
	{
		if(this.shake_is_active) {

			float	shake_time = 0.5f;

			float	t = this.shake_timer/shake_time;


			//

			float	amplitude = 0.05f*Mathf.Lerp(1.0f, 0.0f, t);

			// 隣あったブロックが同じ動きになってしまわないよう、
			// 微妙に周期をオフセットする.

			float	t_offset = (float)((this.place.y*StackBlockControl.BLOCK_NUM_X + this.place.x)%(StackBlockControl.BLOCK_NUM_X - 1));

			t_offset /= (float)(StackBlockControl.BLOCK_NUM_X - 2);

			this.shake_offset.x = amplitude*Mathf.Cos(10.0f*(t + t_offset)*2.0f*Mathf.PI);

			//

			Vector3	p = this.transform.position;

			p.x += this.shake_offset.x;

			this.transform.position = p;

			//

			this.shake_timer += Time.deltaTime;

			if(this.shake_timer >= shake_time) {

				this.shake_is_active = false;
			}
		}
	}

}
