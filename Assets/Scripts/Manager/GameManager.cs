using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region 싱글톤

    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    #endregion

    #region UI 관련 오브젝트

    [Space(10)]
    [Header("UI 관련 오브젝트")]
    [SerializeField] private Image effectImage;

    #endregion

    #region 카메라 관련 오브젝트

    [Space(10)]
    [Header("카메라 관련 오브젝트")]
    [SerializeField] private CameraShake cameraShake;

    #endregion

    #region 게임 관련 중요한 오브젝트

    [Space(10)]
    [Header("게임 관련 중요한 오브젝트")]
    public GameObject curPlayer;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CameraShake(float intensity, float time)
    {
        cameraShake.ShakeCamera(intensity, time);
    }

    Coroutine effectImageRoutine;
    public void ShowEffectImage(float time, float fadeAmount)
    {
        if (effectImageRoutine != null)
        {
            StopCoroutine(effectImageRoutine);
        }

        effectImageRoutine = StartCoroutine(FadeOutObject(effectImage, time, fadeAmount));
    }

    private IEnumerator FadeOutObject(Image _image, float time, float fadeAmount)
    {
        if (time == 0)
        {
            yield return null;
        }

        float targetAlpha = fadeAmount;
        float curAlpha = 0;
        float temp = 0;

        float fadeInOutTime = time / 2;

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, curAlpha);

        while (temp <= fadeInOutTime)
        {
            curAlpha += Time.deltaTime * targetAlpha / fadeInOutTime;

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, curAlpha);

            temp += Time.deltaTime;

            yield return null;
        }

        curAlpha = fadeAmount;

        temp = 0;

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, curAlpha);

        while (temp <= fadeInOutTime)
        {
            curAlpha -= Time.deltaTime / fadeInOutTime;

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, curAlpha);

            temp += Time.deltaTime;

            yield return null;
        }
    }
}
