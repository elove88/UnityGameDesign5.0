using UnityEngine;
using System.Collections;
//スタートメニューのGUIの挙動
public class StartupMenuGUI : MonoBehaviour {
	public Texture titleTexture;
	public GUISkin guiStyle;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}
	void OnGUI(){
		GUI.skin = guiStyle;
		Graphics.DrawTexture( 
			new Rect( (Screen.width - titleTexture.width)/2.0f , 0, titleTexture.width, titleTexture.height )
			, titleTexture
		);
		if( GUI.Button( new Rect( (Screen.width - 150)/2.0f, 300, 150, 40 ), "Start" ) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("OnBeginInstruction");
		}
#if UNITY_EDITOR
		if ( GUI.Button( new Rect( (Screen.width - 150)/2.0f, 360, 150, 40 ), "Development" ) ){
			GameObject.Find("PhaseManager").GetComponent<PhaseManager>().SetPhase("DevelopmentMode");
		}
#endif
	}
}
