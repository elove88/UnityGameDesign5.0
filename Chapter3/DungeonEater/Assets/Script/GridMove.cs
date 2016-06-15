// グリッド移動.
using UnityEngine;
using System.Collections;

public class GridMove : MonoBehaviour {
	// ポーズ.
	private enum PAUSE_TYPE {
		NONE,
		GAME_PAUSE,
		HITSTOP,
	};
	private PAUSE_TYPE  m_pause = PAUSE_TYPE.NONE;

	// 移動速度.
	public float SPEED = 1.0f;
	
	// 移動方向、ベクトル
	private Vector3 m_direction;
	private Vector3 m_move_vector;
	private Vector3 m_current_grid;
	
	// ヒットチェック.
	private const float HITCHECK_HEIGHT = 0.5f;
	private const int HITCHECK_LAYER_MASK = 1 << 0;
	
	// Use this for initialization
	void Start () {
		m_move_vector = Vector3.zero;
		m_direction = Vector3.forward;
		m_pause = PAUSE_TYPE.NONE;
	}
	
	public void OnRestart()
	{
		m_move_vector = Vector3.zero;
		m_pause = PAUSE_TYPE.NONE;
	}
	
	public void OnGameStart()
	{
		m_move_vector = Vector3.zero;
		m_pause = PAUSE_TYPE.NONE;
	}
	
	public void OnStageStart()
	{
		m_move_vector = Vector3.zero;
		m_pause = PAUSE_TYPE.NONE;
	}
	
	public void OnDead()
	{
		m_pause = PAUSE_TYPE.GAME_PAUSE;
	}
	
	public void OnStageClear()
	{
		m_pause = PAUSE_TYPE.GAME_PAUSE;
	}
	
	public void OnRebone()
	{
		m_pause = PAUSE_TYPE.NONE;
	}
	
	
	// Update is called once per frame
	void Update () {
		if (m_pause != PAUSE_TYPE.NONE) {
			m_move_vector = Vector3.zero;
			return;
		}
		
		// あまりにもdeltaTimeが大きいと壁を抜けてしまうので .
		// 大きいときは少しずつ数回に分けて処理する.
		if (Time.deltaTime <= 0.1f)
			Move(Time.deltaTime);
		else {
			int n = (int)(Time.deltaTime / 0.1f) + 1;
			for (int i = 0; i < n; i++)
				Move(Time.deltaTime / (float)n);
		}
	}
	
	public void Move(float t)
	{
		// 次に移動する位置.
		Vector3 pos = transform.position;
		pos += m_direction * SPEED * t;
		
		
		// グリッド上を通過したかチェック.
		bool across = false;		

		// 整数化した値が違っていた場合、グリッドの境界をまたいだ.
		if ((int)pos.x != (int)transform.position.x)
			across = true;
		if ((int)pos.z != (int)transform.position.z)
			across = true;

		Vector3 near_grid = new Vector3(Mathf.Round(pos.x),pos.y,Mathf.Round(pos.z));
		m_current_grid = near_grid;
		// 正面の壁にぶつかったか.
		Vector3 forward_pos = pos + m_direction*0.5f; // 半Unit先までRayを飛ばしてみる.
		if (Mathf.RoundToInt(forward_pos.x) != Mathf.RoundToInt(pos.x) ||
		    Mathf.RoundToInt(forward_pos.z) != Mathf.RoundToInt(pos.z)) {
			Vector3 tpos =pos;
			tpos.y += HITCHECK_HEIGHT;
			bool collided = Physics.Raycast (tpos,m_direction,1.0f,HITCHECK_LAYER_MASK);
			if (collided) {
				pos = near_grid;
				across = true;
			}
		}
		if (across || (pos-near_grid).magnitude < 0.00005f) {
			Vector3 direction_save = m_direction;

			// メッセージを送信して、OnGrid() メソッドを呼ぶ.
			SendMessage("OnGrid",pos);

			if (Vector3.Dot(direction_save,m_direction )< 0.00005f)
				pos = near_grid + m_direction * 0.001f;  // 少し動かしておかないと再びOnGridするので.
		}
		
		m_move_vector = (pos-transform.position)/t;
		transform.position = pos;
	}
	
	public void SetDirection(Vector3 v)
	{
		m_direction = v;
	}
	
	public Vector3 GetDirection()
	{
		return m_direction;
	}
	
	public bool IsReverseDirection(Vector3 v)
	{
		if (Vector3.Dot(v,m_direction) < -0.99999f)
			return true;
		else
			return false;
	}

	public bool CheckWall(Vector3 direction)
	{
		Vector3 tpos =m_current_grid;
		tpos.y += HITCHECK_HEIGHT;
		return Physics.Raycast(tpos,direction,1.0f,HITCHECK_LAYER_MASK);
	}
	
	public bool IsRunning()
	{
		if (m_move_vector.magnitude > 0.01f)
			return true;
		return false;
	}

	public void HitStop(bool enable)
	{
		if (enable)
			m_pause |= PAUSE_TYPE.HITSTOP;
		else
			m_pause &= ~PAUSE_TYPE.HITSTOP;
	}
	
}
