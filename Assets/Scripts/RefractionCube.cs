using UnityEngine;

public class RefractionCube : MonoBehaviour
{
    public LineRenderer m_LineRenderer;
    public float m_MaxDistance = 50f;
    public LayerMask m_LayerMask;
    bool m_IsReflectingLaser = false;
    private bool m_AttachedObject = false;
    private Rigidbody m_Rigidbody;
    public float m_BounceForce;

    private void Start()
    {
        m_LineRenderer.gameObject.SetActive(false);
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if (m_IsReflectingLaser)
        {
            UpdateLaser();
            m_IsReflectingLaser = false;

        }
        else
        {
            m_LineRenderer.gameObject.SetActive(false);
        }
    }
    void UpdateLaser()
    {
        m_LineRenderer.gameObject.SetActive(true);
        float l_Distance = m_MaxDistance;
        Ray l_Ray = new Ray(m_LineRenderer.transform.position, m_LineRenderer.transform.forward);
        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_Distance = l_RayCastHit.distance;
            if (l_RayCastHit.collider.CompareTag("RefractionCube"))
            {
                l_RayCastHit.collider.GetComponent<RefractionCube>().Reflect();
            }
            if (l_RayCastHit.collider.CompareTag("Player"))
            {
                GameManager.GetGameManager().GameOver();
            }
        }
        Vector3 l_Position = new Vector3(0f, 0f, l_Distance);
        m_LineRenderer.SetPosition(1, l_Position);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (m_AttachedObject) return;

        if (collision.collider.CompareTag("BouncingSurface"))
            ApplyBounce(collision);
    }

    void ApplyBounce(Collision col)
    {
        if (!m_Rigidbody) return;

        Vector3 normal = col.GetContact(0).normal;
        m_Rigidbody.AddForce(normal * m_BounceForce, ForceMode.Impulse);
    }

    public void Reflect()
    {
        if(m_IsReflectingLaser) return;

        m_IsReflectingLaser = true;

        UpdateLaser();
    }
    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;
    }
}
