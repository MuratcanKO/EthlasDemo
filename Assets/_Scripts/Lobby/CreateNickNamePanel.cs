using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNickNamePanel : LobbyPanelBase
{
    [Header("CreateNickNamePanel Variables")]

    [SerializeField]
    private TMP_InputField nickNameInputField;

    [SerializeField]
    private Button createNicknameButton;

    public override void InitPanel(LobbyUIManager lobbyUIManager)
    {
        base.InitPanel(lobbyUIManager);
        createNicknameButton.interactable = false;
        createNicknameButton.onClick.AddListener(OnClickCreateNickname);
        nickNameInputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string arg0)
    {
        createNicknameButton.interactable = arg0.Length >= GlobalConstants.MIN_CHAR_FOR_NICKNAME;
    }

    private void OnClickCreateNickname()
    {
        var nickName = nickNameInputField.text;
        if (nickName.Length >= GlobalConstants.MIN_CHAR_FOR_NICKNAME)
        {
            GlobalManagers.Instance.AudioManager.Play(GlobalConstants.CLICK_SFX_NAME);
            GlobalManagers.Instance.NetworkRunnerController.SetPlayerNickname(nickName);

            base.ClosePanel();
            lobbyUIManager.ShowPanel(LobbyPanelType.MiddleSectionPanel);
        }
    }
}