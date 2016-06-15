using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {
	public GUIText m_text;
	
	private static int m_score;

	// Use this for initialization
	void Start () {
		m_score = 0;
	}
	
	void OnGameStart()
	{
		m_score = 0;
	}
	

	
	
	
	// Update is called once per frame
	void Update () {
		m_text.text = "SCORE: "+m_score;
	
	}
	
	public static  void AddScore(int point)
	{
		m_score += point;
	}
	

	
}
