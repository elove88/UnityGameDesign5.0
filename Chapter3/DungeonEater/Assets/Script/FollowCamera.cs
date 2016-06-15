using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	
	//public Vector3 m_rotation;
	//private Quaternion m_rotationQ = Quaternion.identity;
	public Vector3 m_position;
	public Transform m_target;
	private bool m_forcus;
	
	// Use this for initialization
	void Start () {
		//m_rotationQ.eulerAngles = m_rotation;
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = m_target.position + m_position;
		float t = 1.0f - Mathf.Pow(0.75f,Time.deltaTime*30.0f);
		if (m_forcus) {
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView,30.0f,t);
		} else {
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView,60.0f,t);
		}
	}
	
	// フォーカスする.
	public void OnAttack()
	{
		m_forcus = true;
	}
	
	public void OnEndAttack()
	{
		m_forcus = false;
	}
}
