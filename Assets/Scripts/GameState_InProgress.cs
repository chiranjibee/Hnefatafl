public class GameState_InProgress : GameState
{
    public GameState_InProgress(GameManager owner) : base(owner) {}

    public override void OnStateEntered()
    {
        if (m_gameManager == null) { return; }
        m_gameManager.OnGameInProgressEnter();
    }

    public override void OnStateUpdate() {}

    public override void OnStateExited() {}
}
