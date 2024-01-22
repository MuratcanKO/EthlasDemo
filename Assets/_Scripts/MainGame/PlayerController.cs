using Fusion;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    public bool AcceptAnyInput => PlayerIsAlive && !GameManager.MatchIsOver;

    [SerializeField] 
    private TextMeshProUGUI playerNameText;

    [SerializeField] 
    private GameObject cam;

    [SerializeField] 
    private float moveSpeed = 6;

    [SerializeField] 
    private float jumpForce = 1000;


    [Header("Grounded Variables")] 

    [SerializeField] 
    private LayerMask groundLayer;

    [SerializeField] 
    private Transform groundDetectionObj;

    [Networked] 
    public TickTimer RespawnTimer { get; private set; }

    [Networked] 
    public NetworkBool PlayerIsAlive { get; private set; }

    [Networked(OnChanged = nameof(OnNicknameChanged))]
    private NetworkString<_16> playerName { get; set; }

    [Networked] 
    private NetworkButtons buttonsPrev { get; set; }

    [Networked] 
    private Vector2 serverNextSpawnPoint { get; set; }

    [Networked] 
    private NetworkBool isGrounded { get; set; }

    [Networked] 
    private TickTimer respawnToNewPointTimer { get; set; }

    private float horizontal;
    private Rigidbody2D rigid;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;
    private PlayerHealthController playerHealthController;

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        rigid = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        playerHealthController = GetComponent<PlayerHealthController>();

        SetLocalObjects();
        PlayerIsAlive = true;
    }

    private void SetLocalObjects()
    {
        if (Utils.IsLocalPlayer(Object))
        {
            cam.transform.SetParent(null);
            cam.SetActive(true);

            var nickName = GlobalManagers.Instance.NetworkRunnerController.localPlayerNickname;
            RpcSetNickName(nickName);
        }
        else
        {
            GetComponent<NetworkRigidbody2D>().InterpolationDataSource = InterpolationDataSources.Snapshots;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetNickName(NetworkString<_16> nickName)
    {
        playerName = nickName;
    }

    private static void OnNicknameChanged(Changed<PlayerController> changed)
    {
        changed.Behaviour.SetPlayerNickname(changed.Behaviour.playerName);
    }

    private void SetPlayerNickname(NetworkString<_16> nickName)
    {
        playerNameText.text = nickName + " " + Object.InputAuthority.PlayerId;
    }

    public void KillPlayer()
    {
        GlobalManagers.Instance.AudioManager.Play(GlobalConstants.DEATH_SFX_NAME);
        
        if (Runner.IsServer)
        {
            serverNextSpawnPoint = GlobalManagers.Instance.PlayerSpawnerController.GetRandomSpawnPoint();
            respawnToNewPointTimer = TickTimer.CreateFromSeconds(Runner, GlobalConstants.RESPAWN_AMOUNT - 1);
        }
        
        PlayerIsAlive = false;
        rigid.simulated = false;
        playerVisualController.TriggerDieAnimation();
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, GlobalConstants.RESPAWN_AMOUNT);
    }

    public void BeforeUpdate()
    {
        if (Utils.IsLocalPlayer(Object) && AcceptAnyInput)
        {
            horizontal = Input.GetAxisRaw(GlobalConstants.HORIZONTAL);
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();

        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            if (AcceptAnyInput)
            {
                rigid.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigid.velocity.y);

                CheckJumpInput(input);

                buttonsPrev = input.NetworkButtons;
            }
            else
            {
                rigid.velocity = Vector2.zero;
            }
        }

        playerVisualController.UpdateScaleTransforms(rigid.velocity);
    }

    private void CheckRespawnTimer()
    {
        if (PlayerIsAlive) return;

        if (respawnToNewPointTimer.Expired(Runner))
        {
            GetComponent<NetworkRigidbody2D>().TeleportToPosition(serverNextSpawnPoint);
            respawnToNewPointTimer = TickTimer.None;
        }
        
        if (RespawnTimer.Expired(Runner))
        {
            RespawnTimer = TickTimer.None;
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        PlayerIsAlive = true;
        rigid.simulated = true;
        playerVisualController.TriggerRespawnAnimation();
        playerHealthController.ResetHealthAmountToMax();
    }

    public override void Render()
    {
        playerVisualController.RendererVisuals(rigid.velocity, playerWeaponController.IsHoldingShootingKey);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var transform1 = groundDetectionObj.transform;
        isGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox(transform1.position,
            transform1.localScale, 0, groundLayer);

        if (isGrounded)
        {
            var pressed = input.NetworkButtons.GetPressed(buttonsPrev);
            if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
            {
                rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            }
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GlobalManagers.Instance.ObjectPoolingManager.RemoveNetworkObjectFromDic(Object);
        Destroy(gameObject);
    }

    public PlayerData GetPlayerNetworkInput()
    {
        PlayerData data = new PlayerData();
        data.HorizontalInput = horizontal;
        data.GunPivotRotation = playerWeaponController.LocalQuaternionPivotRot;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        data.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        return data;
    }
}