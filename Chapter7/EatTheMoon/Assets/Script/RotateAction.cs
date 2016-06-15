using UnityEngine;

// ブロックの回転（スワップ、カラーチェンジ）.
public struct RotateAction {


	public enum TYPE {

		NONE = -1,

		SWAP_UP = 0,		// スワップ　下から上.
		SWAP_DOWN,			// スワップ　上から下.
		COLOR_CHANGE,		// カラーチェンジ（センターで回る）.

		NUM,
	};

	public bool			is_active;		// 実行中？.
	public float		timer;			// 経過時間.
	public float		rate;			// 経過時間の比率.


	public TYPE			type;

	public Block.COLOR_TYPE	target_color;	// カラーチェンジのときの、変更後の色.

	public static float	rotate_time_swap = 0.25f;

	public static float	ROTATE_TIME_SWAP_MIN = 0.1f;
	public static float	ROTATE_TIME_SWAP_MAX = 1.0f;

	// ---------------------------------------------------------------- //

	// 初期化
	public void init()
	{
		this.is_active = false;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = RotateAction.TYPE.NONE;
	}

	// 回転動作を開始する.
	public void start(RotateAction.TYPE type)
	{
		this.is_active = true;
		this.timer     = 0.0f;
		this.rate      = 0.0f;
		this.type      = type;
	}

	// 回転動作の実行.
	public void	execute(StackBlock block)
	{
		float	x_angle = 0.0f;
		float	rotate_time;

		if(this.type == RotateAction.TYPE.COLOR_CHANGE) {

			rotate_time = 0.5f;

		} else {

			rotate_time = RotateAction.rotate_time_swap;
		}

		if(this.is_active) {

			this.timer += Time.deltaTime;

			// 終了チェック.

			if(this.timer > rotate_time) {

				this.timer     = rotate_time;
				this.is_active = false;
			}

			// 回転の中心.

			Vector3		rotation_center = Vector3.zero;
			
			if(this.is_active) {

				switch(this.type) {
	
					case RotateAction.TYPE.SWAP_UP:
					{
						rotation_center.y = -Block.SIZE_Y/2.0f;
					}
					break;
	
					case RotateAction.TYPE.SWAP_DOWN:
					{
						rotation_center.y =  Block.SIZE_Y/2.0f;
					}
					break;

					case RotateAction.TYPE.COLOR_CHANGE:
					{
						rotation_center.y =  0.0f;
					}
					break;
				}

				// 角度.

				this.rate = this.timer/rotate_time;

				this.rate = Mathf.Lerp(-Mathf.PI/2.0f, Mathf.PI/2.0f, this.rate);
				this.rate = (Mathf.Sin(this.rate) + 1.0f)/2.0f;
				
				x_angle = Mathf.Lerp(-180.0f, 0.0f, this.rate);
			}

			// rotation_center を中心に、相対的に回転する.
			block.transform.Translate(rotation_center);
			block.transform.Rotate(Vector3.right, x_angle);
			block.transform.Translate(-rotation_center);
		}
	}
}
