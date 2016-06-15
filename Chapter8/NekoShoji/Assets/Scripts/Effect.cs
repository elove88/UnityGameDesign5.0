using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour {

	void Start ()
	{

	}
	
	void Update ()
	{

		// 再生が終わったら削除.
		if(!this.GetComponentInChildren<ParticleSystem>().isPlaying) {

			Destroy(this.gameObject);
		}
	}
}
