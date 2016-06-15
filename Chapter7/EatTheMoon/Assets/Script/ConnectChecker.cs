using UnityEngine;
using System.Collections;

public class ConnectChecker {

	public StackBlockControl	stack_control = null;

	public StackBlock[,]	blocks;

	public enum CONNECT_STATUS {

		NONE = -1,

		UNCHECKED = 0,
		CONNECTED,

		NUM,
	};

	public CONNECT_STATUS[,]		connect_status = null;

	public StackBlock.PlaceIndex[]	connect_block;

	// ---------------------------------------------------------------- //

	public void create()
	{
		this.connect_status = new CONNECT_STATUS[StackBlockControl.BLOCK_NUM_X, StackBlockControl.BLOCK_NUM_Y];

		this.connect_block = new StackBlock.PlaceIndex[StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y];
	}

	public void clearAll()
	{
		for(int y = 0;y < StackBlockControl.BLOCK_NUM_Y;y++) {

			for(int x = 0;x < StackBlockControl.BLOCK_NUM_X;x++) {

				this.connect_status[x, y] = CONNECT_STATUS.UNCHECKED;
			}
		}

	}

	// (x, y) の位置からつながっているブロックをチェックする.
	public int	checkConnect(int x, int y)
	{
		//

		int connect_num = this.check_connect_recurse(x, y, Block.COLOR_TYPE.NONE, 0);

		for(int i = 0;i < connect_num;i++) {

			StackBlock.PlaceIndex index = this.connect_block[i];

			this.connect_status[index.x, index.y] = CONNECT_STATUS.CONNECTED;
		}

		return(connect_num);
	}
	private bool	is_error_printed = false;

	private int		check_connect_recurse(int x, int y, Block.COLOR_TYPE previous_color, int connect_count)
	{
		StackBlock.PlaceIndex	block_index;

		do {

			// 無限ループ防止チェック.
			if(connect_count >= StackBlockControl.BLOCK_NUM_X*StackBlockControl.BLOCK_NUM_Y) {

				if(!this.is_error_printed) {

					Debug.LogError("Suspicious recursive call");
					this.is_error_printed = true;
				}
				break;
			}

			// 連結対象じゃない（空中にいるとか、非表示中とか）.
			if(!this.blocks[x, y].isConnectable()) {

				break;
			}

			// すでにほかのブロックと連結していたらスキップ.
			//
			if(this.connect_status[x, y] == CONNECT_STATUS.CONNECTED) {

				break;
			}

			//

			block_index.x = x;
			block_index.y = y;

			// 今回すでにチェック済みならスキップ.
			if(this.is_checked(block_index, connect_count)) {

				break;
			}

			//

			if(previous_color == Block.COLOR_TYPE.NONE) {

				// 最初の一個目.

				this.connect_block[0] = block_index;

				connect_count = 1;

			} else {

				// ２個目以降は、前のブロックと同じ色かチェックする.

				if(this.blocks[x, y].color_type == previous_color) {
	
					this.connect_block[connect_count] = block_index;

					connect_count++;
				}
			}

			// 同じ色が続いていたら、さらに隣もチェックする.

			if(previous_color == Block.COLOR_TYPE.NONE || this.blocks[x, y].color_type == previous_color) {
	
				// 左	
				if(x > 0) {
		
					connect_count = this.check_connect_recurse(x - 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 右
				if(x < StackBlockControl.BLOCK_NUM_X - 1) {
		
					connect_count = this.check_connect_recurse(x + 1, y, this.blocks[x, y].color_type, connect_count);
				}
				// 上
				if(y > 0) {
	
					connect_count = this.check_connect_recurse(x, y - 1, this.blocks[x, y].color_type, connect_count);
				}
				// 下
				if(y < StackBlockControl.BLOCK_NUM_Y - 1) {
	
					connect_count = this.check_connect_recurse(x, y + 1, this.blocks[x, y].color_type, connect_count);
				}
			}

		} while(false);

		return(connect_count);
	}

	// すでにチェック済み？.
	private bool	is_checked(StackBlock.PlaceIndex place, int connect_count)
	{
		bool	is_checked = false;

		for(int i = 0;i < connect_count;i++) {

			if(this.connect_block[i].Equals(place)) {

				is_checked = true;
				break;
			}
		}

		return(is_checked);
	}

}
