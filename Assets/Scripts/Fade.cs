using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    bool m_IsFadeIn;
    public float m_FadeTIme = .8f;
    float m_CurrentTime = 0f;
    public Image m_FadeImage;
    public delegate void OnFadeEndedFn();
    OnFadeEndedFn m_OnFadeEndedFn;

    private void Update()
    {
        UpdateFade();
    }
    void UpdateFade()
    {
        m_CurrentTime += Time.deltaTime;
        float l_Pct = Mathf.Min(1f, m_CurrentTime / m_FadeTIme);
        m_FadeImage.color = new Color(0f, 0f, 0f, m_IsFadeIn ? l_Pct : 1f - l_Pct);
        if (l_Pct == 1f)
        {
            m_OnFadeEndedFn?.Invoke();
        }
    }
    public void FadeIn(OnFadeEndedFn _OnFadeEndedFn)
    {
        _Fade(_OnFadeEndedFn, true);
    }
    public void FadeOut(OnFadeEndedFn _OnFadeEndedFn)
    {
        _Fade(_OnFadeEndedFn, false);
    }
    void _Fade(OnFadeEndedFn _OnFadeEndedFn, bool IsFadeIn)
    {
        m_OnFadeEndedFn = _OnFadeEndedFn;
        m_CurrentTime = 0f;
        gameObject.SetActive(true);
        m_IsFadeIn = IsFadeIn;
        UpdateFade();
    }
}
