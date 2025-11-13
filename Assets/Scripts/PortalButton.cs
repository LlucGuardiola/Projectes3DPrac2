using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_Open;
    public UnityEvent m_Close;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube") && !other.gameObject.GetComponent<CompanionCube>().m_AttachedObject)
        {
            m_Open.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            m_Close.Invoke();
        }
    }
}
