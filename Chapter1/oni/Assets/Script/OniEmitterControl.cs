using UnityEngine;
using System.Collections;

public class OniEmitterControl : MonoBehaviour {

	public GameObject[]	oni_prefab;

	// SE.
	public AudioClip	EmitSound = null;		// 遠くから飛んでくる音（『ひゅ～』）.
	public AudioClip	HitSound = null;		// おにがおに山に当たったときの音.

	// 最後に生成されたオニ.
	private GameObject	last_created_oni = null;

	private static float	collision_radius = 0.25f;

	// 生成するオニの数（残り）.
	// 実際の値はリザルトによってかわる.
	public int		oni_num = 2;

	public bool		is_enable_hit_sound = true;

	// -------------------------------------------------------------------------------- //

	void Start()
	{
		this.GetComponent<AudioSource>().PlayOneShot(this.EmitSound);
	}

	void 	Update()
	{

		do {

			if(this.oni_num <= 0) {

				break;
			}

			// 最後に生成されたオニが十分離れるまで待つ.
			// （同じ位置に重なるように生成すると、コリジョンによって大きくはじかれてしまうので）.
			if(this.last_created_oni != null) {

				if(Vector3.Distance(this.transform.position, last_created_oni.transform.position) <= OniEmitterControl.collision_radius*2.0f) {

					break;
				}
			}

			Vector3	initial_position = this.transform.position;

			initial_position.y += Random.Range(-0.5f, 0.5f);
			initial_position.z += Random.Range(-0.5f, 0.5f);

			// 回転（ランダムっぽく見えればいい）.
			Quaternion	initial_rotation;

			initial_rotation = Quaternion.identity;
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*50.0f, Vector3.forward);
			initial_rotation *= Quaternion.AngleAxis(this.oni_num*30.0f, Vector3.right);

			GameObject oni = Instantiate(this.oni_prefab[this.oni_num%2], initial_position, initial_rotation) as GameObject;	

			//

			oni.GetComponent<Rigidbody>().velocity        = Vector3.down*1.0f;
			oni.GetComponent<Rigidbody>().angularVelocity = initial_rotation*Vector3.forward*5.0f*(this.oni_num%3);

			oni.GetComponent<OniStillBodyControl>().emitter_control = this;

			//

			this.last_created_oni = oni;

			this.oni_num--;

		} while(false);

	}

	// おにがおに山に当たったときの音をならす.
	//
	// 短い間隔でたくさんなってしまうときれいに聞こえないので、一定間隔
	// でなるように調整する.
	//
	public void	PlayHitSound()
	{
		if(this.is_enable_hit_sound) {

			bool	to_play = true;
	
			if(this.GetComponent<AudioSource>().isPlaying) {
	
				if(this.GetComponent<AudioSource>().time < this.GetComponent<AudioSource>().clip.length*0.75f) {
	
					to_play = false;
				}
			}
	
			if(to_play) {
	
				this.GetComponent<AudioSource>().clip = this.HitSound;
				this.GetComponent<AudioSource>().Play();
			}
		}
	}

}
