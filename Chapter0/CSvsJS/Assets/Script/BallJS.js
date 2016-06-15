
var launcher : LauncherJS;				// ランチャーのプレハブ.
var velocity     : Vector3;				// 初速度（Launcher から受け取る用）.	
var is_touched   : boolean;				// プレイヤーが触った？.

private var time        : float;		// 実行中タイマー.
private var is_launched : boolean;		// 発射された？（false ならフェードイン中）.

function Start() : void
{
	// ランチャーのゲームオブジェクトを探しておく.
	this.launcher = GameObject.FindGameObjectWithTag( "GameController" ).GetComponent.< LauncherJS >();

	// アルファーで見えなくしておく.

	var color : Color = this.GetComponent.<Renderer>().material.color;
	color.a = 0.0f;

	this.GetComponent.<Renderer>().material.color = color;

	//

	this.GetComponent.<Rigidbody>().useGravity = false;

	this.is_touched  = false;
	this.is_launched = false;

	this.time = 0.0f;
}

function Update() : void
{
	// 発射される前だったら（フェードイン中だったら）…….
	if ( !this.is_launched )
	{
		var delay : float = 0.5f;

		// アルファーでフェードインする.

		var color : Color = this.GetComponent.<Renderer>().material.color;

		// [0.0f ～ delay] の範囲を [0.0f ～ 1.0f] に変換する
		var t : float = Mathf.InverseLerp( 0.0f, delay, this.time );

		t = Mathf.Min( t, 1.0f );

		color.a = Mathf.Lerp( 0.0f, 1.0f, t );

		this.GetComponent.<Renderer>().material.color = color;

		// 一定時間が経過したら、発射.
		if ( this.time >= delay )
		{
			this.GetComponent.<Rigidbody>().useGravity = true;
			this.GetComponent.<Rigidbody>().velocity = this.velocity;

			this.is_launched = true;
		}
	}

	this.time += Time.deltaTime;

	DebugPrintJS.print( this.GetComponent.<Rigidbody>().velocity.ToString() );
}

// 画面外に出たとき.
function OnBecameInvisible() : void
{
	// ボールが削除されることをランチャーに通知.
	this.launcher.OnBallDestroy();

	// プレイヤーが触れていなかった（空振り）なら、ミス.
	if(!this.is_touched) {

		if(this.launcher != null) {

			this.launcher.setResult(false);
		}
	}

	// ゲームオブジェクトを削除.
	Destroy( this.gameObject );
}

// 他のオブジェクトと衝突したとき.
function OnCollisionEnter(collision : Collision) : void
{
	// 衝突した相手がプレイヤーだったら…….
	if(collision.gameObject.tag == "Player") {

		if(collision.gameObject.GetComponent.< PlayerJS >().isLanded() ) {

			// プレイヤーが着地中だったらミス.

			this.launcher.setResult(false);

			// プレイヤーが触ったことを覚えておく.
			this.is_touched = true;

		} else {

			// プレイヤーがジャンプ中だったら成功.

			this.launcher.setResult(true);
			this.is_touched = true;
		}
	}
}
