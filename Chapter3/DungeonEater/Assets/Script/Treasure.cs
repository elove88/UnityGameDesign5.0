using UnityEngine;
using System.Collections;

public class Treasure : MonoBehaviour {
	public GameObject m_pickupEffect;	// 拾ったときのエフェクト.
	public AudioClip m_SEPickuped; 		// 拾ったときのSE.
	public AudioClip m_SEAppear; 		// 拾ったときのSE.
	public BillBoradText m_scoreBorad;  // 拾ったときの得点表示.
	public int m_point = 1000;			// 拾ったときの得点.
	
	// 消滅時間.
	public float m_lifeTime = 10.0f;
	
	// 初期化.
	void Start () {
		AudioChannels audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		if (audio != null)
			audio.PlayOneShot(m_SEAppear,1.0f,0.0f);
		Destroy(gameObject,m_lifeTime);
	}
	
	// Update
	void Update () {
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerController>() != null) {  // todo: プレイヤーかどうかはコンポーネントで判断するのがよいのか？.
			Score.AddScore(m_point);
			GameObject borad = (GameObject)Instantiate(m_scoreBorad.gameObject,transform.position + new Vector3(0,2.0f,0),Quaternion.identity);
			borad.GetComponent<BillBoradText>().SetText(m_point.ToString());

			// エフェクト発生.
			GameObject o = (GameObject)Instantiate(m_pickupEffect.gameObject,transform.position  + new Vector3(0,1.0f,0),Quaternion.identity);
			
			// 効果音.
			(FindObjectOfType(typeof(AudioChannels)) as AudioChannels).PlayOneShot(m_SEPickuped,1.0f,0.0f);
			Destroy(o,3.0f);
			Destroy(gameObject);
		}
	}
}
