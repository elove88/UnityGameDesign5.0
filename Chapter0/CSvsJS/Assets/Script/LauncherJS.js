
var ball_prefab : GameObject;						// ボールのプレハブ.
var player      : PlayerJS;							// プレイヤー.
var result      : String = "";						// 直前の結果.

private var is_launch_ball	: boolean = false;		// 「ボール発射して」フラグ.

private var good_mess		: String[ ];			// 成功のときのメッセージ.
private var good_mess_index : int;					// 次に表示する成功メッセージ.

// ---------------------------------------------------------------- //

function Start() : void
{
	this.player = GameObject.FindGameObjectWithTag( "Player" ).GetComponent.< PlayerJS >();

	this.is_launch_ball = true;

		//

	this.good_mess = new String[4];

	this.good_mess[0] = "Nice!";
	this.good_mess[1] = "Okay!";
	this.good_mess[2] = "Yatta!";
	this.good_mess[3] = "*^o^*v";

	this.good_mess_index = 0;

}

function Update() : void
{
	// 「ボール発射して」フラグが立っていて、プレイヤーが着地中なら…….	
	if ( this.is_launch_ball && this.player.isLanded() )
	{
		//

		var ball : GameObject = Instantiate( this.ball_prefab );

		ball.transform.position = new Vector3( 5.0f, 2.0f, 0.0f );

		//　X方向のスピードと最高到達点の高さをランダムで求める.

		// 前回の値とある程度差がつくよう、あえて整数版を使う.
		var x_speed : float = -Random.Range( 2, 7 ) * 2.5f;
		var height  : float =  Random.Range( 2.0f, 3.0f );

		// ボールの初速度のY成分を、X方向のスピードとプレイヤー位置での高さから求める.

		var y_speed : float = this.calc_ball_y_speed( x_speed, height, ball.transform.position );

		var velocity : Vector3 = new Vector3( x_speed, y_speed, 0.0f );

		ball.GetComponent.< BallJS >().velocity = velocity;

		// 「ボール発射して」フラグを下す.
		this.is_launch_ball = false;
	}
}

// ボールの初速度のY成分を、X方向のスピードとプレイヤー位置での高さから求める.
private function calc_ball_y_speed( x_speed : float, height : float, ball_position : Vector3 ) : float
{
	var		t : float;
	var		y_speed : float;

	// プレイヤーの位置にたどり着くまでの時間.
	t = (this.player.transform.position.x - ball_position.x)/x_speed;

	// y = v*t - 0.5f*g*t*t
	// から v を求める.
	y_speed = ((height - ball_position.y) - 0.5f*Physics.gravity.y*t*t)/t;

	return(y_speed);
}

// 成功/失敗をセットする.
function setResult(is_success : boolean) : void
{
	if(is_success) {

		// 成功なら、成功メッセージを順番に表示.
		this.result = this.good_mess[this.good_mess_index];

		this.good_mess_index = (this.good_mess_index + 1)%4;

	} else {

		this.result = "Miss!";
	}

	// 一定時間経過後に、結果表示を消す.
	StartCoroutine( clearResult() );
}

// 一定時間経過後に、結果表示を消すためのコルーチン.
function clearResult() : IEnumerator
{
	yield WaitForSeconds( 0.5f );

	this.result = "";
}

function OnGUI() : void
{
	GUI.Label( Rect( 200, 200, 200, 20 ), this.result );
}

// ボールが削除されたとき.
function OnBallDestroy() : void
{
	// 「ボール発射して」フラグを立てる
	this.is_launch_ball = true;
}
