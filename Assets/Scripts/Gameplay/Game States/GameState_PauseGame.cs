
namespace C13
{
    public class GameState_PauseGame : GameState
    {
        public GameState_PauseGame(GameManager owner) : base(owner) { }

        public override void OnStateEntered()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInPauseEnter();
        }

        public override void OnStateUpdate()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInPauseUpdate();
        }

        public override void OnStateExited()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameInPauseExit();
        }
    }
}