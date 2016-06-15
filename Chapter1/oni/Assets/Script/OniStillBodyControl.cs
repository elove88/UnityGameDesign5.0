using UnityEngine;
using System.Collections;

public class OniStillBodyControl : MonoBehaviour {


	public OniEmitterControl	emitter_control = null;

	void 	Start()
	{
	}
	
	void 	Update()
	{
	}

	void	OnCollisionEnter(Collision other)
	{
		if(other.gameObject.tag == "OniYama") {

			// おに山にぶつかったときの音を鳴らす.
			this.emitter_control.PlayHitSound();

			// ここで直接SEを鳴らしてしまうと短い間隔で音がなって、音が重なりあって
			// きれいに聞こえないので、OniEmitterControl で適切な間隔で音がなるよう
			// にする.
		}

		if(other.gameObject.tag == "Floor") {

			// 物理計算を止めるため、rigidbody のコンポーネントを削除してしまう
			// かなり無理やりだけど、Sleep() だとじわじわ動いてしまう.
			Destroy(this.GetComponent<Rigidbody>());
		}
	}
}
