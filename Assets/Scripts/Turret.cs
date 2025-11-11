using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Laser Settings")]
    public LineRenderer m_LineRenderer;
    public float m_MaxDistance = 50f;
    public LayerMask m_LayerMask;
    public float m_MaxAlifeAngle = 15f;

    [Header("Physics Surfaces")]
    public float bounceForce = 10f;
    public float slidingDrag = 0.05f;
    public float slidingAcceleration = 8f;

    private Rigidbody m_Rigidbody;
    private bool m_AttachedObject = false;
    private bool m_LaserEnabled = true;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Solo dispara si no está siendo sostenida
        if (!m_AttachedObject)
            HandleLaser();
        else if (m_LineRenderer.gameObject.activeSelf)
            m_LineRenderer.gameObject.SetActive(false);
    }

    private void HandleLaser()
    {
        float l_DotAngle = Vector3.Dot(transform.up, Vector3.up);

        if (l_DotAngle < Mathf.Cos(m_MaxAlifeAngle * Mathf.Deg2Rad))
        {
            m_LineRenderer.gameObject.SetActive(false);
        }
        else
        {
            m_LineRenderer.gameObject.SetActive(true);

            float l_Distance = m_MaxDistance;
            Ray l_Ray = new Ray(m_LineRenderer.transform.position, m_LineRenderer.transform.forward);

            if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
            {
                l_Distance = l_RayCastHit.distance;

                if (l_RayCastHit.collider.CompareTag("RefractionCube"))
                {
                    var refraction = l_RayCastHit.collider.GetComponent<RefractionCube>();
                    if (refraction != null)
                        refraction.Reflect();
                }
            }

            Vector3 l_Position = new Vector3(0f, 0f, l_Distance);
            m_LineRenderer.SetPosition(1, l_Position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_AttachedObject) return;

        if (collision.collider.CompareTag("BouncingSurface"))
            ApplyBounce(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (m_AttachedObject) return;

        if (collision.collider.CompareTag("SlidingSurface"))
            ApplySliding(collision);
    }

    void ApplyBounce(Collision col)
    {
        if (!m_Rigidbody) return;

        Vector3 normal = col.GetContact(0).normal;
        m_Rigidbody.AddForce(normal * bounceForce, ForceMode.Impulse);
    }

    void ApplySliding(Collision col)
    {
        if (!m_Rigidbody) return;

        Vector3 normal = col.GetContact(0).normal;
        Vector3 slideDir = Vector3.ProjectOnPlane(transform.forward, normal).normalized;

        m_Rigidbody.linearVelocity = slideDir * slidingAcceleration;
        m_Rigidbody.angularVelocity = Vector3.zero;

        transform.forward = Vector3.Lerp(transform.forward, slideDir, Time.deltaTime * 5f);
    }

    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;

        if (m_Rigidbody)
        {
            m_Rigidbody.isKinematic = AttachedObject;
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }

        if (AttachedObject)
        {
            if (m_LineRenderer != null)
                m_LineRenderer.gameObject.SetActive(false);
        }
        else
        {
            // Reactivar láser cuando se suelta
            if (m_LineRenderer != null)
                m_LineRenderer.gameObject.SetActive(true);
        }
    }
}
