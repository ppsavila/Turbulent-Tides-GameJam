using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [field: Header("Menus")]
    [field: SerializeField]
    private CanvasGroup StartMenu {get;set;}

    [field: SerializeField]
    private CanvasGroup PauseMenu {get;set;}

    [field: SerializeField]
    private CanvasGroup GameOverMenu {get;set;}

    [field: SerializeField]
    private CanvasGroup WinningMenu {get;set;}
    
    [field: SerializeField]
    private CanvasGroup CreditsMenu {get;set;}


    [field: Header("StartMenu")]
    [field: SerializeField]
    private Button PlayBtn {get;set;}

    [field: SerializeField]
    private Button SettingsBtn {get;set;}

    [field: SerializeField]
    private Button SettingsBackBtn {get;set;}

    [field: SerializeField]
    private Button WikiBtn {get;set;}

    [field: SerializeField]
    private Button WikiBackBtn {get;set;}

    [field: SerializeField]
    private CanvasGroup PlayMenu {get;set;}

    [field: SerializeField]
    private CanvasGroup SettingsMenu {get;set;}

    [field: SerializeField]
    private CanvasGroup WikiMenu {get;set;}

    [field: Header("PauseMenu")]
    [field: SerializeField]
    private Button SettignsPauseBtn {get;set;}
    [field: SerializeField]
    private Button SettignsBackPauseBtn {get;set;}
    [field: SerializeField]
    private Button MainMenuPauseBtn {get;set;}
    [field: SerializeField]
    private Button BackPauseBtn {get;set;}
    [field: SerializeField]
    private CanvasGroup SettingsPauseMenu {get;set;}

    [field: Header("PauseMenu")]
    [field: SerializeField]
    private Button PlayAgainBtn {get;set;}

    [field: SerializeField]
    private Button TheEndBtn {get;set;}
    [field: SerializeField]
    private Button RePlayBtn {get;set;}
    [field: SerializeField]
    private Button QuitBtn {get;set;}

    [field: Header("Audios")]
    [field: SerializeField]
    private AudioSource AudioSource {get;set;}

    [field: SerializeField]
    private AudioClip PlayClip {get;set;}

    [field: SerializeField]
    private AudioClip ClickClip {get;set;}
    
    private bool isPause = false;

    void Awake()
    {
        isPause = false;

        StartMenu.alpha = 1;
        PlayMenu.alpha = 1;
        SettingsMenu.alpha = 0;
        WikiMenu.alpha = 0;
        PauseMenu.alpha = 0;
        SettingsPauseMenu.alpha = 0;
        GameOverMenu.alpha = 0;
        WinningMenu.alpha = 0;
        CreditsMenu.alpha=0;

        StartMenu.blocksRaycasts = true;
        PlayMenu.blocksRaycasts = true;
        SettingsMenu.blocksRaycasts = false;
        WikiMenu.blocksRaycasts = false;
        PauseMenu.blocksRaycasts = false;
        SettingsPauseMenu.blocksRaycasts = false;
        GameOverMenu.blocksRaycasts = false;
        WinningMenu.blocksRaycasts = false;
        CreditsMenu.blocksRaycasts = false;
    }

    void Start()
    {
        PlayBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(PlayClip);
            StartMenu.DOFade(0f,.2f).OnComplete(()=> 
            StartMenu.blocksRaycasts = false)
            .Play();
        });

        SettingsBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            SettingsMenu.DOFade(1f, .2f).OnComplete(() =>
            {
                
                SettingsMenu.blocksRaycasts = true;
            })
            .Play();
        });

        SettingsBackBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            SettingsMenu.DOFade(0f, .2f).OnComplete(() =>
            {
                SettingsMenu.blocksRaycasts = false;
            })
            .Play();
        });

        WikiBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            WikiMenu.DOFade(1f, .2f).OnComplete(() =>
            {
                WikiMenu.blocksRaycasts = true;
            })
            .Play();
        });

        WikiBackBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            WikiMenu.DOFade(0f, .2f).OnComplete(() =>
            {

                WikiMenu.blocksRaycasts = false;
            })
            .Play();
        });

        SettignsPauseBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            SettingsPauseMenu.DOFade(1f, .2f).OnComplete(() =>
            {
                SettingsPauseMenu.blocksRaycasts = true;
            }).Play();
        });

        SettignsBackPauseBtn.onClick.AddListener(()=>
        {
            AudioSource.PlayOneShot(ClickClip);
            SettingsPauseMenu.DOFade(0f, .2f).OnComplete(() =>
            {
                SettingsPauseMenu.blocksRaycasts = false;
            }).Play();
        });

        MainMenuPauseBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            SceneManager.LoadScene(0);
        });

        BackPauseBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            PauseMenu.DOFade(0f, .2f).OnComplete(() =>
            {
                PauseMenu.blocksRaycasts = false;
            }).Play();
        });

        PlayAgainBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            SceneManager.LoadScene(0);
        });

        TheEndBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            CreditsMenu.DOFade(1f, .2f).OnComplete(() =>
            {
                CreditsMenu.blocksRaycasts = true;
            }).Play();
        });
        RePlayBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            SceneManager.LoadScene(0);
        });
        QuitBtn.onClick.AddListener(() =>
        {
            AudioSource.PlayOneShot(ClickClip);
            Application.Quit();
        });

    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            if(!isPause && StartMenu.alpha != 1)
            {
                AudioSource.PlayOneShot(ClickClip);
                PauseMenu.DOFade(1f, .2f).OnComplete(() =>
                {
                    PauseMenu.blocksRaycasts = true;
                    isPause = true;
                })
                .Play();
            }
            else
            {
                PauseMenu.DOFade(0f, .2f).OnComplete(() =>
                {
                    PauseMenu.blocksRaycasts = false;
                    isPause = false;
                })
                .Play();
            }
        }
    }

    public void GameOver()
    {
        
        GameOverMenu.DOFade(1f, .2f).OnComplete(() =>
        {
            GameOverMenu.blocksRaycasts = true;
        }).Play();
    }

    public void Win()
    {
        WinningMenu.DOFade(1f, .2f).OnComplete(() =>
        {
            WinningMenu.blocksRaycasts = true;
        }).Play();
    }
}
