using UnityEngine;
using System.Collections;

public class MonsterCtrl : MonoBehaviour {
	private Transform m_player ;
	private GameCtrl m_gameCtrl;
	
	// マップ関連.
	private GridMove m_grid_move;
	public Vector3 m_spawnPosition = new Vector3(100,0,100);
	
	// スコア.
	public int m_point = 300;
	public BillBoradText m_scoreBorad ;
	
	// 音.
	private AudioChannels m_audio;
	public AudioClip m_attackSE;
	
	// 挙動.
	public enum AI_TYPE {
		TRACER,
		AMBUSH,
		PINCER,
		RANDOM
	};

	private Transform m_tracer;
	public AI_TYPE m_aiType = AI_TYPE.TRACER;
	private STATE m_state;
	private Transform m_attackTarget;
	
	// 難易度.
	public float m_baseSpeed = 2.1f;
	public float m_speedUpPerLevel = 0.3f;
	private const int MAX_LEVEL = 5;
	
	enum STATE {
		NORMAL,
		DEAD,
	};


	
	// Use this for initialization
	void Awake () {
		m_grid_move = GetComponent<GridMove>();
		m_gameCtrl = FindObjectOfType(typeof(GameCtrl)) as GameCtrl;
		m_gameCtrl.AddObjectToList(gameObject);
		m_audio = FindObjectOfType(typeof(AudioChannels)) as AudioChannels;
		GetComponent<Animation>()["up_sword_action"].layer = 1;
	}
	
	public void SetSpawnPosition(Vector3 pos)
	{
		m_spawnPosition = pos;
	}
	
	
	public void OnRestart()
	{
		m_state = STATE.NORMAL;
		transform.position = m_spawnPosition;
	}
		
	public void OnGameStart()
	{
		OnStageStart();
	}
	
	public void OnStageStart()
	{
		m_state = STATE.NORMAL;
		transform.position = m_spawnPosition;
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		for (int enemyCnt = 0; enemyCnt < enemys.Length; enemyCnt++) {
			MonsterCtrl mc = enemys[enemyCnt].GetComponent<MonsterCtrl>();
			if (mc != null) {
				if (mc.m_aiType == MonsterCtrl.AI_TYPE.TRACER)
					m_tracer = mc.transform;
			}
		}
		
		m_player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	// AI
	public void OnGrid(Vector3 newPos)
	{
		if (m_state != STATE.NORMAL)
			return;
		switch (m_aiType) {
		case AI_TYPE.TRACER:
			Tracer(newPos);
			break;
		case AI_TYPE.AMBUSH:
			Ambush(newPos);
			break;
		case AI_TYPE.RANDOM:
			RandomAI(newPos);
			break;
		case AI_TYPE.PINCER:
			Pincer(newPos);
			break;
			
		}
	}
	
	//  移動方向の候補を参考にして移動可能な方向を得る.
	//  引数:
	//  first   第一候補.
	//  second  第二候補.
	//  戻り値:
	//  移動方向/ 移動不可能ならVector3.zero
	private Vector3 DirectionChoice(Vector3 first, Vector3 second)
	{
		// 第一候補.
		// 第二候補.
		// 第二候補の逆方向.
		// 第ー候補の逆方向.
		//
		// の順番に調べて、移動可能ならその方向を返す.

		// 第一候補.
		if (!m_grid_move.IsReverseDirection(first) && 
		    !m_grid_move.CheckWall(first))
			return first;

		// 第二候補.
		if (!m_grid_move.IsReverseDirection(second) && 
		    !m_grid_move.CheckWall(second))
			return second;
		
		first *= -1.0f;
		second *= -1.0f;
		// 第二候補の逆方向.
		if (!m_grid_move.IsReverseDirection(second) && 
		    !m_grid_move.CheckWall(second))
			return second;
		// 第ー候補の逆方向.
		if (!m_grid_move.IsReverseDirection(first) && 
		    !m_grid_move.CheckWall(first))
			return first;
		
		return Vector3.zero;
	}
	
	private void Tracer(Vector3 newPos)
	{
		Vector3 newDirection1st,newDirection2nd;

		// 自分の位置からプレイヤーの位置に向かうベクトル.
		Vector3 diff = m_player.position - newPos;
		
		// X、Zの絶対値が大きい方を選ぶ.
		if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z)) {
			newDirection1st = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection2nd = new Vector3(0,0,1) * Mathf.Sign(diff.z);
		} else {
			newDirection2nd = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection1st = new Vector3(0,0,1) * Mathf.Sign(diff.z);			
		}

		// 二つの候補から、移動可能な方向を選ぶ.
		Vector3 newDir = DirectionChoice(newDirection1st,newDirection2nd);

		if (newDir == Vector3.zero)
			m_grid_move.SetDirection(-m_grid_move.GetDirection());
		else
			m_grid_move.SetDirection(newDir);
	}
	
	private static float	AMBUSH_DISTANCE = 3.0f;

	private void Ambush(Vector3 newPos)
	{
		Vector3 newDirection1st,newDirection2nd;

		// プレイヤーの前方を目標位置にする.
		Vector3 diff = m_player.position + m_player.forward*AMBUSH_DISTANCE - newPos;

		if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z)) {
			newDirection1st = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection2nd = new Vector3(0,0,1) * Mathf.Sign(diff.z);
		} else {
			newDirection2nd = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection1st = new Vector3(0,0,1) * Mathf.Sign(diff.z);			
		}
		
		Vector3 newDir = DirectionChoice(newDirection1st,newDirection2nd);
		if (newDir == Vector3.zero)
			m_grid_move.SetDirection(-m_grid_move.GetDirection());
		else
			m_grid_move.SetDirection(newDir);
		
	}
	
	private Vector3[] DIRECTION_VEC = new Vector3[4] {
		new Vector3(1,0,0),			// 右.
		new Vector3(-1,0,0),		// 左.
		new Vector3(0,0,1),			// 上.
		new Vector3(0,0,-1)			// 下.
	};
	
	private const float DISTANCE_RANDOM_MOVE = 10.0f;
	
	private void RandomAI(Vector3 newPos)
	{
		Vector3 newDirection1st,newDirection2nd;
		Vector3 diff = m_player.position - newPos;
		if (diff.magnitude < DISTANCE_RANDOM_MOVE) {
			int r = Random.Range(0,4);

			// 縦と横がペアーで選ばれるように.
			newDirection1st = DIRECTION_VEC[r];
			newDirection2nd = DIRECTION_VEC[(r+2)%4];

			if (Random.value > 0.5f)
				newDirection2nd *= -1.0f;

			Vector3 newDir = DirectionChoice(newDirection1st,newDirection2nd);
			if (newDir == Vector3.zero)
				m_grid_move.SetDirection(-m_grid_move.GetDirection());
			else
				m_grid_move.SetDirection(newDir);
		} else
			Tracer(newPos);
	
	}
	
	private void Pincer(Vector3 newPos)
	{
		Vector3 newDirection1st,newDirection2nd;

		Vector3 diff;
		if (m_tracer == null)
			diff = m_player.position - newPos;
		else
			diff = m_player.position * 2 - m_tracer.position - newPos;
		
		
		if (Mathf.Abs(diff.x) > Mathf.Abs(diff.z)) {
			newDirection1st = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection2nd = new Vector3(0,0,1) * Mathf.Sign(diff.z);
		} else {
			newDirection2nd = new Vector3(1,0,0) * Mathf.Sign(diff.x);
			newDirection1st = new Vector3(0,0,1) * Mathf.Sign(diff.z);			
		}
		
		Vector3 newDir = DirectionChoice(newDirection1st,newDirection2nd);
		if (newDir == Vector3.zero)
			m_grid_move.SetDirection(-m_grid_move.GetDirection());
		else
			m_grid_move.SetDirection(newDir);
		
	}
	
	// 難易度を背呈する.
	// 引数：
	// difficulty   難しさ.
	public void SetDifficulty(int difficulty)
	{
		if (difficulty > MAX_LEVEL)
			difficulty = MAX_LEVEL;
		m_grid_move.SPEED = m_baseSpeed+difficulty*m_speedUpPerLevel;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (m_state != STATE.NORMAL)
			return;
		
		PlayerController player = other.GetComponent<PlayerController>();
		if (player != null) {
			player.Encount(transform);
			
		}
	}
	
	public void Damage()
	{
		SendMessage("OnDead");
		m_state = STATE.DEAD;
		Score.AddScore(m_point);
		GameObject borad = (GameObject)Instantiate(m_scoreBorad.gameObject,transform.position + new Vector3(0,2.0f,0),Quaternion.identity);
		borad.GetComponent<BillBoradText>().SetText(m_point.ToString());
		StartCoroutine("Dead");
	}

	public void OnStageClear()
	{
		StopAllCoroutines();
		SendMessage("OnDead");
		m_state = STATE.DEAD;
	}
	
	IEnumerator Dead()
	{
		yield return new WaitForSeconds(3.0f);
		m_state = STATE.NORMAL;
		SendMessage("OnRebone");
	}
	
	// 攻撃.
	public void Attack(Transform other)
	{
		m_attackTarget = other;
		StartCoroutine("AttackSub");
	}
	
		
	IEnumerator AttackSub()
	{
		m_gameCtrl.OnAttack();
		transform.LookAt(m_attackTarget.position);
		
		yield return null;
		GetComponent<Animation>().CrossFade("up_sword_action",0.1f);
		m_audio.PlayOneShot(m_attackSE,1.0f,0.0f);
		yield return new WaitForSeconds(0.5f);
		m_attackTarget.SendMessage("Damage");
		GetComponent<Animation>().CrossFade("up_idle",0.25f);
		m_gameCtrl.OnEndAttack(); 
	}
	
	// 足音(無効).
	public void PlayStepSound(AnimationEvent ev)
	{
	}
}