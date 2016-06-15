using UnityEngine;
using System.Collections;

public class SonarCamera : MonoBehaviour {

//    [SerializeField]
    private float radius = 0.0f;

    void Awake()
    {
        // カメラとCollider半径を揃えておく
        radius = GetComponent<Camera>().orthographicSize;
        Debug.Log(Time.time + ": SonarCamera.Awake");
    }

    void Start()
    {

        SphereCollider shereCollider = GetComponent<SphereCollider>();
        if (shereCollider)
        {
            shereCollider.radius = radius;
        }
    }

    // Enterではとりのがしが発生する場合がある
    void OnTriggerEnter(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Enter(other.gameObject);
    }

    // Stayで代用する
    void OnTriggerStay(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Enter(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
//        Debug.Log("OnTriggerStay:" + other.gameObject.tag + ", " + other.gameObject.name);
        CheckSonarPoint_Exit(other.gameObject);
    }

    private void CheckSonarPoint_Enter(GameObject target)
    {
        if (!target.CompareTag("Sonar")) return;
        ColorFader fader = target.GetComponent<ColorFader>();
        if (fader==null) return;
        if (fader.SonarInside()) return;
        Debug.Log("CheckSonarPoint");
        target.BroadcastMessage("OnSonarInside");
    }

    private void CheckSonarPoint_Exit(GameObject target)
    {
        if (!target.CompareTag("Sonar")) return ;
//        ColorFader fader = target.GetComponent<ColorFader>();
//        if (fader) return fader.SonarInside();
        Debug.Log("CheckSonarPoint_TriggerExit");
        target.SendMessage("OnSonarOutside");
    }

    void OnInstantiatedSonarPoint(GameObject target)
    {
        // すでにソナー内にいるかチェックする
        Vector3 pos = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 target_pos = new Vector3( target.transform.position.x, 0.0f, target.transform.position.z );
        float dist = Vector3.Distance(pos, target_pos);
        Debug.Log("OnInstantiatedSonarPoint[" + target.transform.parent.gameObject.name + "]: dist=" + dist + ", radius=" + radius);
        if (dist <= radius)
        {
            target.SendMessage("OnSonarInside");
        }
        else {
            target.SendMessage("OnSonarOutside");
        }
    }

    public float Radius() 
    {
        return radius;
    } 

    // 表示位置調整
	public void SetRect( Rect rect )
    {
        Debug.Log("SetRect:" + rect);
        GetComponent<Camera>().pixelRect = new Rect(rect.x, rect.y, rect.width, rect.height);

        // カメラ表示領域をテクスチャに内接させる場合
        //float r = rect.width * 0.5f;
        //float newWidth = r * Mathf.Pow(2.0f,0.5f);
        //float sub = (rect.width - newWidth)*0.5f;
        //camera.pixelRect = new Rect(rect.x + sub, rect.y + sub, newWidth, newWidth);
        
        //sonarCamera.pixelRect = new Rect( rect );
    }
}
