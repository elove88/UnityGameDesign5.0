using UnityEngine;
using System.Collections;

public class Note : MonoBehaviour {
    [SerializeField]
    private float interval = 1.0f;
    [SerializeField]
    private float offset = 0.0f;
    [SerializeField]
    private bool visible = true;
    [SerializeField]
    private bool valid   = true;

    private float counter;
    private float param;
    private Color baseColor;

    private void OnHitItem()
    {
        Debug.Log("OnHitItem"); 
        valid = false;
        // Stopと使うと音がぶつ切りになる場合があるため、音量をフェードアウトさせて対応
        //audio.Stop();

        // エフェクト開始(1つだけとする)
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
//        Debug.Log("Emitter" + emitter);
        if (particleSystem) {
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if(player != null) {

				this.transform.parent = player.transform;
				this.transform.localPosition = Vector3.forward*1.0f;
			}
            particleSystem.Play();
            Debug.Log("Particle Start");
        }

        StartCoroutine("Fadeout", 1.0f);
        Debug.Log("OnHitItem End");
    }

    private IEnumerator Fadeout(float duration)
    {
        // フェードアウト
        float currentTime = 0.0f;
        float waitTime = 0.02f;
        float firstVol = GetComponent<AudioSource>().volume;
        while (duration > currentTime)
        {
//            audio.volume = Mathf.Clamp01(firstVol * (duration - currentTime) / duration);
            GetComponent<AudioSource>().volume = Mathf.Lerp( firstVol, 0.0f, currentTime/duration );
            Debug.Log("Step:" + GetComponent<AudioSource>().volume);
            yield return new WaitForSeconds(waitTime);
            currentTime += waitTime;
        }

        // エフェクトが完全に終了していたらオブジェクト破棄
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        while (particleSystem.isPlaying)
        {
            yield return new WaitForSeconds(waitTime);
        }
        //Destroy( gameObject );
        //Debug.Log("Destory");

        GameObject parent = GameObject.Find("/NotesObject");
        if(parent) parent.SendMessage("OnDestroyObject", gameObject);
    }

    private void Clock(float step)
    {
        if (valid)
        {
            counter += step;
            if (counter >= interval)
            {
                GetComponent<AudioSource>().Play();
                param = 1.0f;
                counter = 0.0f;
                Debug.Log(name + ":Play");
            }
        }
    }

    // Use this for initialization
	void Start () 
    {
        counter = offset;
        GetComponent<Renderer>().enabled = visible;
        baseColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b);
        param = 1.0f;
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
        if (valid)
        {
            Clock(Time.deltaTime);
            if (visible)
            {
                param *= Mathf.Exp(-3.0f * Time.deltaTime);
                //	        transform.localScale = Vector3.one * (1.0f + param * 0.5f);
                Color color = new Color(Mathf.Abs(baseColor.r - param), baseColor.g, baseColor.b);
                GetComponent<Renderer>().material.color = color;
            }
        }
	}

}
