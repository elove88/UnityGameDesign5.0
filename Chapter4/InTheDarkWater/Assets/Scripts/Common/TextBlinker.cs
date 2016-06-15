using UnityEngine;
using System.Collections;

/// <summary>
/// �e�L�X�g�̓_��
/// </summary>
public class TextBlinker : MonoBehaviour
{
    [SerializeField]
    private bool valid = true;
    [SerializeField]
    private float blinkTime = 0.8f;
    [SerializeField]
    private int num = 5;

    private int count = 0;

    void Start()
    {
    }

    // �_�ŃX�^�[�g
    void OnStartTextBlink()
    {
        if (!GetComponent<GUIText>() || !valid) return;
        count = 0;
        GetComponent<GUIText>().enabled = true;
        StartCoroutine("Delay", blinkTime);
    }


    private IEnumerator Delay(float delaytime)
    {
        yield return new WaitForSeconds(delaytime);
        GetComponent<GUIText>().enabled = !GetComponent<GUIText>().enabled;
        count++;
        if (count < num)
        {
            StartCoroutine("Delay", blinkTime);
        }
        else
        {
            // �I���ʒm
            SendMessage("OnEndTextBlink", SendMessageOptions.DontRequireReceiver);
        }
    }
}
