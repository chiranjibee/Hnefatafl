
namespace C13
{
    public class GameState_InProgress : GameState
    {
        public GameState_InProgress(GameManager owner) : base(owner) { }

        public override void OnStateEntered()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInProgressEnter();
        }

        public override void OnStateUpdate()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInProgressUpdate();
        }

        public override void OnStateExited()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInProgressExit();
        }
    }
}
