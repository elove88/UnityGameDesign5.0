
private var is_landed  : boolean;				// 着地中？.
private var JumpHeight : float   = 4.0f;		// ジャンプの高さ.

// ---------------------------------------------------------------- //

function Start() : void
{
	this.is_landed = false;
}

function Update() : void
{
	// 着地中だったら…….
	if(this.is_landed) {

		// マウスの右ボタンがクリックされたら…….
		if(Input.GetMouseButtonDown(0)) {

			this.is_landed = false;

			// ジャンプの高さから、初速度を求める.
			var y_speed = Mathf.Sqrt(2.0f*Mathf.Abs(Physics.gravity.y)*this.JumpHeight);

			GetComponent.<Rigidbody>().velocity = Vector3.up*y_speed;
		}
	}

	DebugPrintJS.print(this.is_landed.ToString());
}
// 引数まであっていないとちゃんと呼び出されないので注意.
function OnCollisionEnter(collision : Collision) : void
{
	// Debug.Log の使い方
	// どんなオブジェクトも Debug.Log するときは "ToString()" メソッドで
	// （float なども ToString() で）.
	Debug.Log( collision.gameObject.ToString());

	// これをやらないと、ボールとヒットしたときに this.is_landed が true になってしまう）.
	if(collision.gameObject.tag == "Floor") {

		this.is_landed = true;
	}
}

// 着地中？.
function isLanded() : boolean
{
	return(this.is_landed);
}
