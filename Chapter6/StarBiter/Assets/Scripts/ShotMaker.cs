using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// Boss Shot 弾の発射制御.
// ----------------------------------------------------------------------------
public class ShotMaker : MonoBehaviour {

	public float fireInterval = 0.1f;		// 発砲間隔.
	public int numberOfBullets = 10;		// 一回の弾数.
	
	public GameObject Shot;					// 弾のPrefab.
	
	private GameObject player;				// プレイヤー.
	
	private int fireCount;					// 発砲回数.
	private bool isFiring = false;			// 発砲中.
	private bool isMakingBullet = false;	// 弾作成中.
	
	private float fireAngle = 0;			// 発射角度.
	
	void Start () {
	
		// playerのインスタンスを取得.
		player = GameObject.FindGameObjectWithTag("Player");

	}
	
	void Update () {
		
		// 発射中か?.
		if ( isFiring )
		{
			// 発射前?
			if ( fireCount == 0 )
			{
				// 発射角度計算.
				SetAngle();
			}
			
			// 弾の発射準備中か?.
			if ( !isMakingBullet )
			{
				isMakingBullet = true;
				MakeBullet();
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 弾を作成.
	// ------------------------------------------------------------------------
	private void MakeBullet()
	{
		// 弾のGameObjectは指定されているか?
		if ( Shot )
		{
			// 弾作成.
			GameObject tmpBullet;
			tmpBullet = Instantiate( Shot, transform.position, Quaternion.Euler( 0, fireAngle, 0 ) ) as GameObject;
			tmpBullet.SendMessage( "SetTarget", player );	
			
			// 発射数をカウント.
			fireCount++;
			
			// 角度を1発毎に15度動かす.
			fireAngle -= 15f;
			
			// 指定した数を発射したら弾の作成を終了する.
			if ( fireCount >= numberOfBullets )
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
	// 発射角度を求める.
	// ------------------------------------------------------------------------
	private void SetAngle()
	{
		// 発射角度計算.
		Vector3 targetPosition = player.transform.position;
		Vector3 relativePosition = targetPosition - transform.position;
		Quaternion tiltedRotation = Quaternion.LookRotation( relativePosition );
		fireAngle = tiltedRotation.eulerAngles.y + ( numberOfBullets / 2 ) * 15;
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
	// 発射中かどうかの状態を返す.
	// ------------------------------------------------------------------------
	public bool GetIsFiring()
	{
		return isFiring;
	}
}
