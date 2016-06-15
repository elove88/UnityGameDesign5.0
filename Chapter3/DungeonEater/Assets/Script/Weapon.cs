using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	public GameCtrl m_gameCtrl;		// ゲーム.
	public GameObject m_sword;		// プレイヤーの剣.
	public GameObject m_scoreBorad;	// スコア表示オブジェクト.
	private AudioChannels m_audio;	// オーディオ.
	public AudioClip m_swordAttackSE;	// 攻撃SE.
	public GameObject SWORD_ATTACK_OBJ;  // 攻撃範囲オブジェクト.
	
	private bool m_equiped = false;  // ソードを装備中.
	private Transform m_target;  // 攻撃対象.
	
	// 得点.
	private const int POINT = 500;
	private const int COMBO_BONUS = 200;
	private int m_combo = 0;
	
	// 初期化.
	void Start () {
		m_equiped = false;
		m_sword.GetComponent<Renderer>().enabled = false;

		// アニメーションの合成を開始するノード.
		Transform mixTransform = transform.Find("root/hip/mune");

		// 剣を振り下ろす.
		GetComponent<Animation>()["up_sword_action"].layer = 1;
		GetComponent<Animation>()["up_sword_action"].AddMixingTransform(mixTransform);		

		// 剣を胸の前に掲げる.
		GetComponent<Animation>()["up_sword"].layer = 1;
		GetComponent<Animation>()["up_sword"].AddMixingTransform(mixTransform);			

		m_audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		m_combo = 0;
	}
	
	// ステージ開始時.
	void OnStageStart()
	{
		m_equiped = false;
		m_sword.GetComponent<Renderer>().enabled = false;
	}
	
	// ソードを拾った.
	void OnGetSword()
	{
		if (!m_equiped) {
			m_sword.GetComponent<Renderer>().enabled = true;
			m_equiped = true;
			GetComponent<Animation>().CrossFade("up_sword",0.25f);
		} else {
			BillBoradText borad = ((GameObject)Instantiate(m_scoreBorad,transform.position + new Vector3(0,2.0f,0),Quaternion.identity)).GetComponent<BillBoradText>();
			int point = POINT + COMBO_BONUS * m_combo;
			borad.SetText(point.ToString());
			Score.AddScore(point);
			m_combo++;
		}
	}
	
	void Remove()  
	{
		m_sword.GetComponent<Renderer>().enabled = false;
		m_equiped = false;
//		animation.CrossFade("up_idle",0.25f);
		GetComponent<Animation>().Stop("up_sword_action");
		GetComponent<Animation>().Stop("up_sword");
		m_combo = 0;
	}

	
	// 自動攻撃する.
	public void AutoAttack(Transform other)
	{
		if (m_equiped) {
			m_target = other;
			StartCoroutine("SwordAutoAttack");
		}
	}
	
	// 攻撃可能か？.
	public bool CanAutoAttack()
	{
		if (m_equiped)
			return true;
		else
			return false;
	}
		
	
	IEnumerator SwordAutoAttack()
	{
		m_gameCtrl.OnAttack();
		// 振り向く.
		transform.LookAt(m_target.transform);
		yield return null;
		// 攻撃.
		GetComponent<Animation>().CrossFade("up_sword_action",0.2f);
		yield return new WaitForSeconds(0.3f);
		m_audio.PlayOneShot(m_swordAttackSE,1.0f,0.0f);		
		yield return new WaitForSeconds(0.2f);
		Vector3 projectilePos;
		projectilePos = transform.position + transform.forward * 0.5f;
		Instantiate(SWORD_ATTACK_OBJ,projectilePos,Quaternion.identity);
		yield return null;
		// 向きを元に戻す.
		Remove();
		m_gameCtrl.OnEndAttack();
	}
}
