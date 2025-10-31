using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    private float m_Yaw;
    private float m_Pitch;

    private Vector3 m_StartPosition;
    private Quaternion m_StartRotation;

    public float m_YawSpeed;
    public float m_PitchSpeed;
    public float m_Speed;
    public float m_JumpSpeed;
    public float m_SpeedMultiplier;

    public float m_MinPitch;
    public float m_MaxPitch;

    public Transform m_PitchController;

    public bool m_UseInvertedYaw;
    public bool m_UseInvertedPitch;

    bool m_AngleLocked = false;
    public CharacterController m_CharacterController;
    float m_VerticalSpeed = 0f;

    public float CoyoteTime;
    private float CoyoteTimeCounter = 0;

    public Camera m_Camera;

    [Header("Shoot")]
    public float m_ShootMaxDistance = 50f;
    public LayerMask m_ShootLayerMask;
    //public GameObject m_ShootParticles;
    //public GameObject m_ShootFlare;
    public int m_AmmoCount = 0;
    public int m_MagazineMaxCapacity = 20;
    public int m_MagazineCurrentBullets = 20;
    public float m_FireRateCooldown = 1f;
    public float m_FireRateCurrentTime = 0f;
    public float m_ReloadCooldown = 1f;
    public float m_ReloadTime = 0f;

    [Header("Animation")]

    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;

    [Header("Input")]
    private KeyCode m_LeftKeyCode = KeyCode.A;
    private KeyCode m_RightKeyCode = KeyCode.D;
    private KeyCode m_UpKeyCode = KeyCode.W;
    private KeyCode m_DownKeyCode = KeyCode.S;
    private KeyCode m_JumpKeyCode = KeyCode.Space;
    private KeyCode m_RunKeyCode = KeyCode.LeftShift;
    private KeyCode m_GrabKeyCode = KeyCode.E;
    public int m_BlueShootMouseButton = 0;
    public int m_OrangeShootMouseButton = 1;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;

    [Header("Stats")]
    public int m_Life = 60;
    public int m_MaxLife = 100;
    public int m_Shield = 40;
    public int m_MaxShield = 100;
    [HideInInspector] public List<int> m_PlayerKeys;

    [Header("Teleport")]
    public float m_portalDistance = 1.5f;
    Vector3 m_MovementDirection;
    public float m_MaxAngleToTeleport;

    [Header("Portals")]
    public Portal m_BluePortal;
    public Portal m_OrangePortal;

    [Header("AttachObject")]
    public ForceMode m_ForceMode;
    public float m_ThrowForce = 10f;
    public Transform m_GripTransform;
    Rigidbody m_AttachedRigidbody;
    bool m_AttachingObject;
    Vector3 m_StartAttachingObjectPosition;
    float m_AttachingCurrentTime;
    public float m_AttachingTime = 1.5f;
    public float m_AttachingObjetRotationDistanceLerp = 2f;
    bool m_AttachedObject;
    public LayerMask m_ValidAttachObjectsLayerMask;

    void Start()
    {
        //PlayerController l_Player = GameManager.GetGameManager().GetPlayer();
        //if (l_Player != null)
        //{
        //    l_Player.m_CharacterController.enabled = false;
        //    l_Player.transform.position = transform.position;
        //    l_Player.transform.rotation = transform.rotation;
        //    l_Player.m_CharacterController.enabled = true;
        //    l_Player.m_StartPosition = transform.position;
        //    l_Player.m_StartRotation = transform.rotation;
        //    Destroy(gameObject);
        //    return;
        //}

        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;

        DontDestroyOnLoad(gameObject);
        //GameManager.GetGameManager().SetPlayer(this);
        Cursor.lockState = CursorLockMode.Locked;
        SetIdleAnimation();
    }

    void Update()
    {
        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");

        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;

        if (!m_AngleLocked)
        {
            m_Yaw = m_Yaw + l_MouseX * m_YawSpeed * Time.deltaTime * (m_UseInvertedYaw ? -1.0f : 1.0f);
            m_Pitch = m_Pitch + l_MouseY * m_PitchSpeed * Time.deltaTime * (m_UseInvertedPitch ? -1.0f : 1.0f);
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            transform.rotation = Quaternion.Euler(0f, m_Yaw, 0f);
            m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0f, 0f);
        }

        Vector3 l_Movement = Vector3.zero;
        float l_YawPiRadians = m_Yaw * Mathf.Deg2Rad;
        float l_Yaw90Radians = (m_Yaw + 90f) * Mathf.Deg2Rad;

        Vector3 l_ForwardDirection = new Vector3(Mathf.Sin(l_YawPiRadians), 0f, Mathf.Cos(l_YawPiRadians));
        Vector3 l_RightDirection = new Vector3(Mathf.Sin(l_Yaw90Radians), 0f, Mathf.Cos(l_Yaw90Radians));

        if (Input.GetKey(m_RightKeyCode))
            l_Movement += l_RightDirection;
        else if (Input.GetKey(m_LeftKeyCode))
            l_Movement -= l_RightDirection;

        if (Input.GetKey(m_UpKeyCode))
            l_Movement += l_ForwardDirection;
        else if (Input.GetKey(m_DownKeyCode))
            l_Movement -= l_ForwardDirection;

        float l_SpeedMultiplier = 1f;

        if (Input.GetKey(m_RunKeyCode))
            l_SpeedMultiplier = m_SpeedMultiplier;

        l_Movement.Normalize();
        m_MovementDirection = l_Movement;
        l_Movement *= m_Speed * l_SpeedMultiplier * Time.deltaTime;

        m_VerticalSpeed = m_VerticalSpeed + Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if (m_VerticalSpeed < 0f && (l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            m_VerticalSpeed = 0f;
            CoyoteTimeCounter = CoyoteTime;
        }
        else if (m_VerticalSpeed > 0f && (l_CollisionFlags & CollisionFlags.Above) != 0)
            m_VerticalSpeed = 0f;

        CoyoteTimeCounter -= Time.deltaTime;
        if (m_FireRateCurrentTime > 0f) m_FireRateCurrentTime -= Time.deltaTime;
        if (m_ReloadTime > 0f) m_ReloadTime -= Time.deltaTime;

        if (Input.GetKeyDown(m_JumpKeyCode) && CoyoteTimeCounter > 0)
        {
            m_VerticalSpeed = m_JumpSpeed;
            CoyoteTimeCounter = 0;
        }
        
        if(CanShoot())
        {
            if (Input.GetMouseButtonDown(m_BlueShootMouseButton))
                Shoot(m_BluePortal);
            else if (Input.GetMouseButtonDown(m_OrangeShootMouseButton))
                Shoot(m_OrangePortal);
        }

        if (CanAttachObjects())
            AttachObject();

        if (m_AttachedRigidbody != null)
            UpdateAttachedObject();

        //if (Input.GetMouseButtonDown(m_BlueShootMouseButton) && CanShoot())
        //    //Shoot();

        UpdateHUD();
    }
    bool CanAttachObjects()
    {
        return true;
    }

    bool CanShoot()
    {
        return true;
        //bool l_CanShoot = m_MagazineCurrentBullets > 0 && m_FireRateCurrentTime <= 0f && m_ReloadTime <= 0f;
        //return l_CanShoot;
    }
    int GetBullets()
    {
        int l_Bullets = m_MagazineMaxCapacity - m_MagazineCurrentBullets;
        l_Bullets = m_AmmoCount < l_Bullets ? m_AmmoCount : l_Bullets;
        m_AmmoCount -= l_Bullets;
        return l_Bullets;
    }
    void Shoot(Portal _Portal)
    {
        //m_MagazineCurrentBullets--;
        //m_FireRateCurrentTime = m_FireRateCooldown;
        //m_ShootFlare.SetActive(true);

        //SetShootAnimation();
        //Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, m_ShootLayerMask.value))
        //{
        //    if (l_RaycastHit.collider.CompareTag("HitCollider"))
        //    {
        //        l_RaycastHit.collider.GetComponent<HitCollider>().Hit();

        //    }
        //    else if (l_RaycastHit.collider.CompareTag("MovingTarget"))
        //    {
        //        MovingTarget m_MovingTarget = l_RaycastHit.collider.GetComponent<MovingTarget>();
        //        if (m_MovingTarget != null)
        //            m_MovingTarget.Hit();
        //    }
        //    else if (l_RaycastHit.collider.CompareTag("Target"))
        //    {
        //        Target m_Target = l_RaycastHit.collider.GetComponent<Target>();
        //        if (m_Target != null)
        //            m_Target.Hit();
        //    }
        //    else
        //    {
        //        CreateShootHitParticles(l_RaycastHit.point, l_RaycastHit.normal);
        //    }
        //}
        SetShootAnimation();
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, 
            _Portal.m_ValidLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
            {
                if (_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.gameObject.SetActive(true);

                }
                else
                {
                    _Portal.gameObject.SetActive(false);
                }
            }
        }

    }
    void CreateShootHitParticles(Vector3 Position, Vector3 Normal)
    {
        //GameObject l_ShootParticles = m_ShootParticlesPool.GetNextElement();
        //l_ShootParticles.transform.position = Position;
        //l_ShootParticles.transform.rotation = Quaternion.LookRotation(Normal);
        //l_ShootParticles.SetActive(true);
    }
    void SetIdleAnimation()
    {
        //m_Animation.CrossFade(m_IdleAnimationClip.name);
    }
    void SetReloadAnimation()
    {
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.0f);
    }
    void SetShootAnimation()
    {
        //m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        //m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.0f);
    }

    public void AddAmo(int Ammo)
    {
        m_AmmoCount += Ammo;
    }
    public void AddShield(int shield)
    {
        m_Shield = m_Shield + shield > m_MaxShield ? m_MaxShield : m_Shield + shield;
    }
    public void AddLife(int life)
    {
        m_Life = m_Life + life > m_MaxLife ? m_MaxLife : m_Life + life;
    }
    public void AddKey(int key)
    {
        m_PlayerKeys.Add(key);
    }
    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Item"))
        //{
        //    Item l_Item = other.GetComponent<Item>();

        //    if (l_Item.CanPick())
        //    {
        //        l_Item.Pick();
        //    }
        //}
        //else if (other.CompareTag("DeathZone"))
        //{
        //    Kill(false);
        //}
        //else if (other.CompareTag("Door"))
        //{
        //    if (other.gameObject.GetComponent<Door>().m_Locked) return;

        //    other.gameObject.GetComponent<Door>().MoveForward();
        //}

        if (other.CompareTag("Portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
            {
                Teleport(other.GetComponent<Portal>());
            }
        }
    }
    bool CanTeleport(Portal _Portal)
    {
        float l_DotValue = Vector3.Dot(_Portal.transform.forward, -m_MovementDirection);
        return l_DotValue > Mathf.Cos(m_MaxAngleToTeleport*Mathf.Deg2Rad);
    }
    void Teleport(Portal _Portal)
    {
        m_MovementDirection.Normalize();
        Vector3 l_NextPosition = transform.position + m_MovementDirection * m_portalDistance;
        Vector3 l_LocalPosition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_NextPosition);
        Vector3 l_WorldPosition = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);
        l_WorldForward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Door"))
        //{
        //    if (other.gameObject.GetComponent<Door>().m_Locked) return;

        //    other.gameObject.GetComponent<Door>().ResetPosition();
        //}
    }
    void Kill(bool resetStats)
    {
        //GameManager.GetGameManager().m_Fade.FadeIn(() => {
        //    GameManager.GetGameManager().RestartLevel(resetStats);
        //});
    }
    public void Restart(bool resetStats)
    {
        m_CharacterController.enabled = false;

        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;

        if (resetStats)
        {
            m_Life = m_MaxLife;
            m_Shield = m_MaxShield;
            m_MagazineCurrentBullets = m_MagazineMaxCapacity;
            m_AmmoCount = 15;
        }

        m_CharacterController.enabled = true;
        UpdateHUD();
    }

    void AttachObject()
    {   
        if(Input.GetKeyDown(m_GrabKeyCode))
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance,
                m_ValidAttachObjectsLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if (l_RaycastHit.collider.CompareTag("Cube"))
                {
                    AttachObject(l_RaycastHit.rigidbody);
                }
            }
        }
        //m_AttachingObject = true;
        //m_AttachedRigidbody = _Rigidody;
        //m_StartAttachingObjectPosition = _Rigidody.transform.position;
        //m_AttachingCurrentTime = 0f;
        //m_AttachedObject = false;
    }

    void AttachObject(Rigidbody _Rigidbody)
    {
        m_AttachingObject = true;
        m_AttachedRigidbody = _Rigidbody;
        m_StartAttachingObjectPosition = _Rigidbody.transform.position;
        m_AttachingCurrentTime = 0f;
        m_AttachedObject = false;
    }
    void UpdateAttachedObject()
    {
        if (m_AttachingObject)
        {
            m_AttachingCurrentTime += Time.deltaTime;
            float l_Pct = Mathf.Min(1f, m_AttachingCurrentTime / m_AttachingTime);
            Vector3 l_Position = Vector3.Lerp(m_StartAttachingObjectPosition, m_GripTransform.position, l_Pct);
            float l_Distance = Vector3.Distance(l_Position, m_GripTransform.position);
            float l_RotationPct = 1f - Mathf.Min(1f, l_Distance / m_AttachingObjetRotationDistanceLerp);
            Quaternion l_Rotation = Quaternion.Lerp(transform.rotation, m_GripTransform.rotation.normalized, l_RotationPct);
            m_AttachedRigidbody.MovePosition(l_Position);
            m_AttachedRigidbody.MoveRotation(l_Rotation);

            if(l_Pct == 1f)
            {
                m_AttachingObject = false;
                m_AttachedObject = true;
                m_AttachedRigidbody.transform.SetParent(m_GripTransform);
                m_AttachedRigidbody.transform.localPosition = Vector3.zero;
                m_AttachedRigidbody.transform.localRotation = Quaternion.identity;
                m_AttachedRigidbody.isKinematic = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
            ThrowObject(m_ThrowForce);
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyUp(m_GrabKeyCode))
            ThrowObject(0f);
    }
    
    void ThrowObject(float Force)
    {
        m_AttachedRigidbody.isKinematic = false;
        m_AttachedRigidbody.AddForce(m_PitchController.forward * Force, m_ForceMode);
        m_AttachedRigidbody.transform.SetParent(null);
        m_AttachingObject = false;
        m_AttachedObject = false;
        m_AttachedRigidbody = null;
    }

    public void TakeDamage(int damage)
    {
        if (m_Shield > 0)
        {
            int m_shieldDamage = Mathf.RoundToInt(damage * 0.75f);
            int m_lifeDamage = damage - m_shieldDamage;

            m_Shield -= m_shieldDamage;
            if (m_Shield < 0)
            {
                m_lifeDamage += -m_Shield;
                m_Shield = 0;
            }

            m_Life -= m_lifeDamage;
        }
        else
        {
            m_Life -= damage;
        }

        if (m_Life <= 0)
        {
            m_Life = 0;
            Kill(true);
        }

        UpdateHUD();
    }

    void UpdateHUD()
    {
        //if (GameManager.GetGameManager().hud != null)
        //{
        //    GameManager.GetGameManager().hud.UpdateStats(m_Life, m_Shield, m_MagazineCurrentBullets, m_AmmoCount);
        //}
    }
}