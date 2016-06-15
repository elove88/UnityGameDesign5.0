using UnityEngine;
using System.Collections;

/// <summary>
/// ソナーポイントのフェード。
/// 表示・非表示のフラグ管理
/// </summary>
public class ColorFader : MonoBehaviour {

    [SerializeField]
    private float duration = 2.0f;
    [SerializeField]
    private float delay = 1.0f;
    [SerializeField]
    private float minAlpha = 0.1f;

    [SerializeField]
    private bool sonarHit = false;
    [SerializeField]
    private bool sonarInside = false;

    private bool wait = false;
    private float max = 0.0f;
    private float currentTime = 0.0f;
    private Color startColor;

	void Start () 
    {
        max = 1.0f - minAlpha;
        startColor = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, GetComponent<Renderer>().material.color.a);

        // 生成された段階で自分がソナー内にいるかチェック
        GameObject player = GameObject.Find("/Field/Player");
        if (player)
        {
            player.BroadcastMessage("OnInstantiatedSonarPoint", gameObject);
        }
	}

	void Update () 
    {
        if (!GetComponent<Renderer>().enabled) return;

        if (!wait)
        {
            float time = currentTime / duration;
            if (time <= (2.0f*max))
            {
                float alpha = 1.0f - Mathf.PingPong(time, max);
                GetComponent<Renderer>().material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                // 時間更新
                currentTime += Time.deltaTime;
            }
            else
            {
                wait = true;
                StartCoroutine("Delay", delay);
            }
        }
	}
    
    void OnTriggerEnter(Collider other)
    {
//        Debug.Log("OnTriggerEnter:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Enter(other.gameObject);
    }
    void OnTriggerStay(Collider other)  // Enterで抜けが発生する可能性があるので、Stayでもみておく
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Enter(other.gameObject);
    }
    void OnTriggerExit(Collider other)
    {
//        Debug.Log("OnTriggerExit:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarCamera_Exit(other.gameObject);
    }

    void CheckSonarCamera_Enter(GameObject target)
    {
        if (sonarInside) return;
        if (!target.CompareTag("SonarCamera")) return;

        Debug.Log("CheckSonarCamera_Enter");
        OnSonarInside();
    }

    void CheckSonarCamera_Exit(GameObject target)
    {
        if (!sonarInside) return;
        if (!target.CompareTag("SonarCamera")) return;

        Debug.Log("CheckSonarCamera_Exit");
        OnSonarOutside();
    }


    void OnHit()
    {
        // ヒットした瞬間でソナーから見えなくする
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> OnHit");
        sonarHit = false;
        Enable();
    }

    void OnActiveSonar()
    {
        // ソナーから見えることを許可する
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> ActiveSonar");
        sonarHit = true;
        Enable();
    }

    void OnSonarInside()
    {
        // ソナー表示領域の内側
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> SonarInside");
        sonarInside = true;
        Enable();
    }

    void OnSonarOutside()
    {
        // ソナー表示領域の外側
        Debug.Log(gameObject.transform.parent.gameObject.name + " -> SonarOutside");
        sonarInside = false;
        Enable();
    }

    private void Enable()
    {
        Debug.Log(gameObject.transform.parent.gameObject.name + ": sonarInside=" + sonarInside + ", sonarHit=" + sonarHit);
        bool result = (sonarInside && sonarHit)?true:false;
        GetComponent<Renderer>().enabled = result;
        if (result)
        {
            wait = false;
            currentTime = 0.0f;
        }
    }

    private IEnumerator Delay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        wait = false;
        currentTime = 0.0f;
    }

    public bool SonarInside() {
        return sonarInside;
    }
}
