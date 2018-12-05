
namespace C13
{
    public class GameState_MainMenu : GameState
    {
        public GameState_MainMenu(GameManager owner) : base(owner) { }

        public override void OnStateEntered()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnMainMenuEnter();
        }

        public override void OnStateUpdate() { }

        public override void OnStateExited()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnMainMenuExit();
        }
    }
}