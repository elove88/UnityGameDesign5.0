using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Vulcan バルカン弾の発射制御.
// Boss Vulcan Unitの状態管理.
// ----------------------------------------------------------------------------
public class BulletMaker : MonoBehaviour {
	
	public float fireInterval = 0.3f;		// 発砲間隔.
	public int numberOfBullets = 15;		// 一回の発砲数.
	
	public GameObject Bullet;				// バルカン弾のPrefab.
	
	private GameObject player;				// プレイヤーのインスタンス.
	private PlayerStatus playerStatus;		// プレイヤーステータスのインスタンス.
	
	private GameObject shotPosition;		// バルカン弾の発射場所.
	
	private int fireCount;					// 発砲回数.
	private bool isFiring = false;			// 発砲中.
	private bool isMakingBullet = false;	// バルカン弾作成中.
	
	private GameObject subScreenMessage;	// SubScreenのメッセージ領域.
	
	void Start () {
	
		// playerのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");
		
		// プレイヤーステータスのインスタンスを取得.
		playerStatus = player.GetComponent<PlayerStatus>();
		
		// バルカン弾の発射位置情報を取得.
		shotPosition = GetComponentInChildren<Transform>().Find("ShotPosition").gameObject;
		
		// SubScreenMessageのインスタンスを取得.
		subScreenMessage = GameObject.FindGameObjectWithTag("SubScreenMessage");

	}
	
	void Update () {
	
		// 発射中か?.
		if ( isFiring )
		{
			// バルカン弾の発射準備中か?.
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// バルカン弾を作成.
	//  - プレイヤーが生きている時のみ発射する.
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
		// バルカン弾のGameObjectは指定されているか?
		if ( Bullet )
		{
			// バルカン弾作成.
			GameObject tmpBullet;
			if ( playerStatus.GetIsNOWPLAYING() )
			{
				tmpBullet = Instantiate( Bullet, shotPosition.transform.position, this.transform.rotation ) as GameObject;
				tmpBullet.SendMessage( "SetTarget", player );	
			}
			
			// 発射数をカウント.
			this.fireCount++;
			
			// 指定した数を発射したらバルカン弾の作成を終了する.
			if ( this.fireCount >= numberOfBullets )
			{
				isFiring = false;
			}
			
			// 次の発射まで一定時間待つ.
			StartCoroutine( WaitAndUpdateFlag( fireInterval ) );
		}
	}
	
	// ------------------------------------------------------------------------
	// 指定した時間を待ち、状態を変更する.
	// ------------------------------------------------------------------------
	IEnumerator WaitAndUpdateFlag( float waitForSeconds )
	{
		// 待ち.
		yield return new WaitForSeconds( waitForSeconds );
		
		// ステージ更新.
		isMakingBullet = false;
	}
	
	// ------------------------------------------------------------------------
	// 発射を開始.
	// ------------------------------------------------------------------------
	public void SetIsFiring()
	{
		fireCount = 0;
		this.isFiring = true;
	}
	
	// ------------------------------------------------------------------------
	// BOSS Vulcan Unitが破壊された時の処理.
	// ------------------------------------------------------------------------
	void OnDestroy()
	{
		if ( this.GetComponent<EnemyStatus>() )
		{
			if (
				this.GetComponent<EnemyStatus>().GetIsBreakByPlayer() ||
				this.GetComponent<EnemyStatus>().GetIsBreakByStone() )
			{
				subScreenMessage.SendMessage("SetMessage", " ");
				subScreenMessage.SendMessage("SetMessage", "DESTROYED VULCAN UNIT." );
				subScreenMessage.SendMessage("SetMessage", " ");
			}
		}
	}
}
