using UnityEngine;
using System;

public enum GameMode
{
    None,
    Digging,
    Building,
    CancelOrders,
    DeconstructBuilding
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance {  get; private set; }
    public GameMode CurrentMode { get; private set; } = GameMode.None;

    public event Action<GameMode> OnGameModeChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ChangeGameMode(GameMode newMode)
    {
        if(CurrentMode == newMode) newMode = GameMode.None;
        CurrentMode = newMode;
        Debug.Log($"[GameModeManager] Режим изменен на: {CurrentMode}");

        OnGameModeChanged?.Invoke(CurrentMode);
    }

    public void OnDiggingBtn() => ChangeGameMode(GameMode.Digging);
    public void OnCancelOrdersBtn() => ChangeGameMode(GameMode.CancelOrders);
    public void OnBuildingBtn() => ChangeGameMode(GameMode.Building);
    public void OnDeconstructBtn() => ChangeGameMode(GameMode.DeconstructBuilding);
    public void OnBackBtn() => ChangeGameMode(GameMode.None);
}
