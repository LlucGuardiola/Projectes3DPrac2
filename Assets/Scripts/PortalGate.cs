using System.Collections;
using UnityEngine;

public class PortalGate : MonoBehaviour
{
    public Animation m_Animation;
    public AnimationClip m_OpenAnimationClip;
    public AnimationClip m_CloseAnimationClip;
    public AnimationClip m_OpenedAnimationClip;
    public AnimationClip m_ClosedAnimationClip;

    public bool m_IsOpened; 

    public enum TState
    {
        OPEN,
        OPENED,
        CLOSE,
        CLOSED
    }

    TState m_State;

    private void Start()
    {
        if (m_IsOpened)
        {
            SetOpenedState();
        }
        else
        {
            SetClosedState();
        }
    }
    void SetOpenedState()
    {
        m_State = TState.OPENED;
        m_Animation.Play(m_OpenedAnimationClip.name);
    }
    void SetClosedState()
    {
        m_State = TState.CLOSED;
        m_Animation.Play(m_ClosedAnimationClip.name);
    }
    public void Open()
    {
        if (m_State == TState.CLOSED)
        {
            m_State=TState.OPEN;
            m_Animation.Play(m_OpenAnimationClip.name);
            StartCoroutine(SetState(m_OpenAnimationClip.length, TState.OPENED));
        }
    }
    public void Close()
    {
        if (m_State == TState.OPENED)
        {
            m_State = TState.CLOSE;
            m_Animation.Play(m_CloseAnimationClip.name);
            StartCoroutine(SetState(m_CloseAnimationClip.length, TState.CLOSED));
        }
    }

    IEnumerator SetState(float _Length, TState _State)
    {
        yield return new WaitForSeconds(_Length);
        m_State = _State;
    }
}
