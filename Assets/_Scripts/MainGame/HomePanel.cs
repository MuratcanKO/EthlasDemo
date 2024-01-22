using UnityEngine;
using UnityEngine.UI;

public class HomePanel : MonoBehaviour
{

    [SerializeField] 
    private Button returnToLobbyButton;

    [SerializeField] 
    private Button continueButton;

    [SerializeField] 
    private GameObject childObj;

    void Start()
    {
        GlobalManagers.Instance.GameManager.OnHomeIsClicked += OnHomeIsClicked;
        returnToLobbyButton.onClick.AddListener(() => GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner());
        continueButton.onClick.AddListener(OnClickContinueButton);
    }

    private void OnClickContinueButton()
    {
        childObj.SetActive(false);
        GlobalManagers.Instance.AudioManager.Play(GlobalConstants.CLICK_SFX_NAME);
    }

    private void OnHomeIsClicked()
    {
        GlobalManagers.Instance.AudioManager.Play(GlobalConstants.CLICK_SFX_NAME);
        childObj.SetActive(true);
    }

    private void OnDestroy()
    {
        GlobalManagers.Instance.GameManager.OnHomeIsClicked -= OnHomeIsClicked;
    }
}
