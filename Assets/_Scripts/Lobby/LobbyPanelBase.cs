using UnityEngine;

public class LobbyPanelBase : MonoBehaviour
{
    [field: SerializeField, Header("LobbyPanelBase Variables")]
    public LobbyPanelType PanelType { get; private set; }
    
    protected LobbyUIManager lobbyUIManager;
    
    public enum LobbyPanelType
    {
        None,
        CreateNicknamePanel,
        MiddleSectionPanel
    }

    public virtual void InitPanel(LobbyUIManager uiManager)
    {
        lobbyUIManager = uiManager;
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
    }

    protected void ClosePanel()
    {
        this.gameObject.SetActive(false);
    }
}
