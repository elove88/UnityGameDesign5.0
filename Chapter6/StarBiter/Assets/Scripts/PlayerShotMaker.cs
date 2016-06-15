using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------
// プレイヤーのショットを作成.
// ----------------------------------------------------------------------------
public class PlayerShotMaker : MonoBehaviour {
	
	public float fireInterval = 0.1f;		// 弾と弾の間隔.
	public float fireSetInterval = 0.3f;	// 次の3弾を発砲するまでの待ち時間.
	public int fireOrderMax = 1;			// 発射指示の最大数.
	
	public GameObject shot;					// 弾.
	
	public GameObject effectShot;
	
	private GameObject effectShot1;
	private GameObject effectShot2;
	private GameObject effectShot3;
	private ParticleSystem particleEffectShot1;		// ショットエフェクト.
	private ParticleSystem particleEffectShot2;		// ショットエフェクト.
	private ParticleSystem particleEffectShot3;		// ショットエフェクト.
	
	private ArrayList fireOrders = new ArrayList();	// 発射指示.
	
	private bool isFiring = false;			// 発砲中.
	
	void Start () 
	{
		// ショットエフェクトのインスタンスを取得.
		effectShot1 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot1 = effectShot1.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot2 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot2 = effectShot2.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
		effectShot3 = Instantiate( effectShot, Vector3.zero, new Quaternion( 0, 0, 0, 0 ) ) as GameObject;
		particleEffectShot3 = effectShot3.GetComponentInChildren<ParticleSystem>() as ParticleSystem;
	}
	
	void Update ()
	{
		
		ReadFireOrder();
	}
	
	// Fire -> Memory
	public void SetFireOrder()
	{
		if ( fireOrders.Count < fireOrderMax )
		{
			fireOrders.Add( true );
		}
	}
	
	// Memory -> Fire
	private void ReadFireOrder()
	{
		if ( fireOrders.Count > 0 )
		{
			if ( !isFiring )
			{
				isFiring = true;
				fireOrders.RemoveAt(0);
				StartCoroutine( "Firing" );
			}
		}
	}
	
	// ------------------------------------------------------------------------
	// 一定間隔でショットを3つ射出する.
	// ------------------------------------------------------------------------
	IEnumerator Firing()
	{
		// ショットオブジェクトが存在するか?.
		if ( shot )
		{
			// 弾1作成.
			Instantiate( shot, transform.position, transform.rotation );
			effectShot1.transform.rotation = transform.rotation;
			effectShot1.transform.position = effectShot1.transform.forward * 1.5f;
			// エフェクトの角度を補正.
			//  - 素材の方向補正.90fを足しているのは単に素材の方向を補正するため.
			//  - 57.29578fで割ると丁度プレイヤーの前方位置になる.(ParticleSystemのstartRotationはこれをしないとずれる).
			particleEffectShot1.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// エフェクトの再生.
			particleEffectShot1.Play();
			
			// 一定時間待つ.
			yield return new WaitForSeconds( fireInterval );
			
			// 弾2作成.
			Instantiate( shot, transform.position, transform.rotation );
			effectShot2.transform.rotation = transform.rotation;
			effectShot2.transform.position = effectShot2.transform.forward * 1.5f;
			// エフェクトの角度を補正.
			//  - 素材の方向補正.90fを足しているのは単に素材の方向を補正するため.
			//  - 57.29578fで割ると丁度プレイヤーの前方位置になる.(ParticleSystemのstartRotationはこれをしないとずれる).
			particleEffectShot2.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// エフェクトの再生.
			particleEffectShot2.Play();
			
			// 一定時間待つ.
			yield return new WaitForSeconds( fireInterval );
			
			// 弾3作成.
			Instantiate( shot, transform.position, transform.rotation );
			effectShot3.transform.rotation = transform.rotation;
			effectShot3.transform.position = effectShot3.transform.forward * 1.5f;
			// エフェクトの角度を補正.
			//  - 素材の方向補正.90fを足しているのは単に素材の方向を補正するため.
			//  - 57.29578fで割ると丁度プレイヤーの前方位置になる.(ParticleSystemのstartRotationはこれをしないとずれる).
			particleEffectShot3.startRotation = ( transform.rotation.eulerAngles.y + 90f ) / 57.29578f;
			// エフェクトの再生.
			particleEffectShot3.Play();
			
			// 次の発射まで一定時間待つ.
			yield return new WaitForSeconds( fireSetInterval );
			
			// 発射終了.
			isFiring = false;
		}
	}

}
