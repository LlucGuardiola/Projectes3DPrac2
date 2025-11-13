using NUnit.Framework;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public float m_NearCameraOffset;
    public List<Transform> m_ValidPositions;
    public bool m_PreviewPortal;

    [Header("Validation")]
    public float m_ValidDistanceOffset = .15f;
    public LayerMask m_ValidLayerMask;
    public float m_MaxAnglePermitted = 5.0f;

    private void LateUpdate()
    {
        if ( m_PreviewPortal) { return; }

        Vector3 l_WorldPosition = Camera.main.transform.position;
        Vector3 l_LocalPosition = m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);

        m_MirrorPortal.m_Camera.transform.position = m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = Camera.main.transform.forward;
        Vector3 l_LocalForward = m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);
        m_MirrorPortal.m_Camera.transform.forward = m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        float l_DistanceToPortal = Vector3.Distance(m_MirrorPortal.transform.position, 
            m_MirrorPortal.m_Camera.transform.position);
        m_MirrorPortal.m_Camera.nearClipPlane = l_DistanceToPortal + m_NearCameraOffset;
    }
    public bool IsValidPosition(Vector3 Position, Vector3 Normal)
    {
        bool l_Valid = true;
        gameObject.SetActive(false);
        Vector3 l_InitialPosition = transform.position;
        Quaternion l_InitialRotation = transform.rotation;

        transform.position = Position;
        transform.rotation = Quaternion.LookRotation(Normal);
        Vector3 l_CameraPosition = Camera.main.transform.position;

        for (int i = 0; i < m_ValidPositions.Count; ++i)
        {
            Vector3 l_ValidPosition = m_ValidPositions[i].position;
            Vector3 l_Direction = l_ValidPosition - l_CameraPosition;
            float l_Distance = Vector3.Distance(l_ValidPosition, l_CameraPosition);
            l_Direction /= l_Distance;

            Ray l_Ray = new Ray(l_CameraPosition, l_Direction);
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, l_Distance + m_ValidDistanceOffset, 
                m_ValidLayerMask.value))
            {
                if (l_RaycastHit.collider.CompareTag("DrawableWall"))
                {
                    if (Vector3.Distance(l_RaycastHit.point, l_ValidPosition) < m_ValidDistanceOffset)
                    {
                        float l_DotAngle = Vector3.Dot(l_RaycastHit.normal, m_ValidPositions[i].forward);
                        if(l_DotAngle < Mathf.Cos(m_MaxAnglePermitted * Mathf.Deg2Rad))
                        {
                            l_Valid = false;

                        }
                    }
                    else
                    {
                        l_Valid = false;
                    }
                }
                else
                {
                    l_Valid = false;
                }
            }
            else
            {
                l_Valid = false;
            }
        }

        if (!l_Valid)
        {
            transform.position = l_InitialPosition;
            transform.rotation = l_InitialRotation;
            if (m_PreviewPortal) 
            { 
                gameObject.SetActive(false);
                return l_Valid;
            }
        }

        gameObject.SetActive(true);
        
        return l_Valid;
    }
    public void ChangeSize(int _Size)
    {
        switch(_Size)
        {
            case 0:
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case 1:
                transform.localScale = new Vector3(1f, 1f, 1f);
                break;
            case 2:
                transform.localScale = new Vector3(2f, 2f, 2f);
            break;
        }
    }
}
