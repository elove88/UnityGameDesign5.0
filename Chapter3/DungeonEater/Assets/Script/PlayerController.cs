using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private GridMove m_grid_move;
	private Map m_map;
	private GameCtrl m_gameCtrl;
	private Weapon m_weapon;
	public AudioClip m_stepSE;
	
	public float THRESHOLD = 0.1f;
	private Vector3 m_lastInput = Vector3.zero;
	private float m_lastInputTime = 0;
	
	public enum STATE {
		NORMAL,
		DEAD
	};
	
	// Use this for initialization
	void Start () {
		// 使用するObjectをキャッシュ.
		m_grid_move = GetComponent<GridMove>();
		m_gameCtrl = FindObjectOfType(typeof(GameCtrl)) as GameCtrl;
		m_weapon = GetComponent<Weapon>();
		m_gameCtrl.AddObjectToList(gameObject);
		m_map = FindObjectOfType(typeof(Map)) as Map;
	}
	
	void OnStageStart()
	{
		ChangeState("State_Normal",State_NormalInit);
		transform.position = m_map.GetSpawnPoint(Map.SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER);
		m_lastInput = Vector3.zero;
		m_lastInputTime = 0.0f;
		
	}
	
	void OnRestart()
	{
		ChangeState("State_Normal",State_NormalInit);;
		transform.position = m_map.GetSpawnPoint(Map.SPAWN_POINT_TYPE.BLOCK_SPAWN_POINT_PLAYER);
		m_lastInput = Vector3.zero;
		m_lastInputTime = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		m_updateFunc();
	}
	

	Vector3 GetMoveDirection()
	{
		float xacc = Input.GetAxis("Horizontal");
		float zacc = Input.GetAxis("Vertical");
		float absXacc = Mathf.Abs(xacc);
		float absZacc = Mathf.Abs(zacc);
		
		// 先行入力.
		if (absXacc < THRESHOLD && absZacc < THRESHOLD) {
			if (m_lastInputTime < 0.2f) {
				m_lastInputTime += Time.deltaTime;
				xacc = m_lastInput.x;
				zacc = m_lastInput.z;
				absXacc = Mathf.Abs(xacc);
				absZacc = Mathf.Abs(zacc);
			}
		} else {
			m_lastInputTime = 0;
			m_lastInput.x = xacc;
			m_lastInput.z = zacc;
		}
		
		if (absXacc < 0.1f && absZacc < 0.1f)
			return Vector3.zero;
		Vector3 direction;
		if (absXacc > absZacc)
			direction = new Vector3(xacc,0,0).normalized;
		else
			direction = new Vector3(0,0,zacc).normalized;
		return direction;
	}
	
	Vector3 GetAdvModeMoveDirection()
	{
		Vector3[] directions = new Vector3[4] {
			new Vector3(1,0,0),
			new Vector3(-1,0,0),
			new Vector3(0,0,1),
			new Vector3(0,0,-1)
		};
		
		while (true) {
			Vector3 d = directions[Random.Range(0,4)];
			if (!m_grid_move.IsReverseDirection(d))
				return d;
		}
	}
	
	
	public void OnGrid(Vector3 newPos)
	{
		// 宝石を拾う.
		m_map.PickUpItem(newPos);
		Vector3 direction;
		if (GlobalParam.GetInstance().IsAdvertiseMode())
			direction  = GetAdvModeMoveDirection();
		else
			direction = GetMoveDirection();

		// キー入力なし（方向転換しない）.
		if (direction == Vector3.zero)
			return;

		// キー入力の方向へ移動（移動できる場合）.
		if (!m_grid_move.CheckWall(direction))
			m_grid_move.SetDirection(direction);
	}
	
	public void Encount(Transform other)
	{
		m_encountFunc(other);
	}
	
	public void Damage( )
	{
		ChangeState("State_Dead",State_DeadInit);
	}
	
	
	public bool IsDead()
	{
		return m_currentStateName == "State_Dead";
	}
		

	//-----------------------------
	delegate void STATE_FUNC(); // 引数なし戻り値なし.
	private string m_currentStateName;
	STATE_FUNC m_stateEndFunc;
	
	delegate void ENCOUNT_FUNC(Transform o);
	private ENCOUNT_FUNC m_encountFunc;
	private STATE_FUNC m_updateFunc;
	
	private void SetDefaultFunc()
	{
		m_stateEndFunc = null;
		m_encountFunc = Encount_Normal;
		m_updateFunc = Update_Normal;
	}
	
	private void ChangeState(string newState, STATE_FUNC init)
	{
		if (m_currentStateName == newState)
			return;
		
		StopCoroutine(m_currentStateName);

		if (m_stateEndFunc != null)
			m_stateEndFunc();
		
		SetDefaultFunc();
		
		m_currentStateName = newState;
		
		if (init != null)
			init();
		
		StartCoroutine(m_currentStateName);
	}


	//--------- Normal State --------
	private void State_NormalInit()
	{
		m_encountFunc = Encount_Normal;
		m_updateFunc = Update_Normal;
	}
	
	IEnumerator State_Normal()
	{
		yield return null;
	}
	
	private void Encount_Normal(Transform other)
	{
		if (m_weapon != null && m_weapon.CanAutoAttack())
			m_weapon.AutoAttack(other);
		else 
			other.SendMessage("Attack",transform);
		
	}
	
	private void Update_Normal()
	{
		if (Input.GetButtonDown("Jump"))
			SendMessage("OnAttack");
		Vector3 direction =GetMoveDirection();
		if (direction == Vector3.zero)
			return;

		// 真後ろのキーが押されたら、方向転換.
		if (m_grid_move.IsReverseDirection(direction))
			m_grid_move.SetDirection(direction);
	}
	
	
	//---------- Dead State --------
	private void State_DeadInit()
	{
		m_encountFunc = Encount_Dead;
		m_updateFunc = Update_Dead;
	}
	
	
	IEnumerator State_Dead()
	{
		SendMessage("OnDead");
		yield return new WaitForSeconds(3);
		m_gameCtrl.PlayerIsDead();
	}
	
	private void Encount_Dead(Transform other)
	{
		return;
	}
	
	private void Update_Dead()
	{
		return;
	}
	
	// 足音再生.
	public void PlayStepSound(AnimationEvent ev)
	{
		(FindObjectOfType(typeof(AudioChannels)) as AudioChannels).PlayOneShot(m_stepSE,1.0f,0.0f,ev.floatParameter);
	
	}
}
