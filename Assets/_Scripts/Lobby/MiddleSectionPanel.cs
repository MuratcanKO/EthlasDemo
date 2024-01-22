using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiddleSectionPanel : LobbyPanelBase
{
    [Header("MiddleSectionPanel Variables")] 
    [SerializeField] 
    private Button joinRandomRoomButton;

    [SerializeField] 
    private Button joinRoomByArgButton;

    [SerializeField] 
    private Button createRoomButton;

    [SerializeField] 
    private TMP_InputField joinRoomByArgInputField;

    [SerializeField] 
    private TMP_InputField createRoomInputField;
    private NetworkRunnerController networkRunnerController;

    public override void InitPanel(LobbyUIManager uiManager)
    {
        base.InitPanel(uiManager);

        joinRoomByArgButton.interactable = false;
        createRoomButton.interactable = false;

        networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;

        joinRandomRoomButton.onClick.AddListener(JoinRandomRoom);
        joinRoomByArgButton.onClick.AddListener(() => CreateRoom(GameMode.Client, joinRoomByArgInputField.text));
        createRoomButton.onClick.AddListener(() => CreateRoom(GameMode.Host, createRoomInputField.text));

        joinRoomByArgInputField.onValueChanged.AddListener(OnInputValueChangedForJoinRoom);
        createRoomInputField.onValueChanged.AddListener(OnInputValueChangedForCreateRoom);
    }

    private void OnInputValueChangedForJoinRoom(string arg0)
    {
        joinRoomByArgButton.interactable = arg0.Length >= GlobalConstants.MIN_CHAR_FOR_ROOMNAME;
    }

    private void OnInputValueChangedForCreateRoom(string arg0)
    {
        createRoomButton.interactable = arg0.Length >= GlobalConstants.MIN_CHAR_FOR_ROOMNAME;
    }

    private void CreateRoom(GameMode mode, string field)
    {
        GlobalManagers.Instance.AudioManager.Play(GlobalConstants.CLICK_SFX_NAME);
        networkRunnerController.StartGame(mode, field);
    }

    private void JoinRandomRoom()
    {
        GlobalManagers.Instance.AudioManager.Play(GlobalConstants.CLICK_SFX_NAME);
        networkRunnerController.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }
}