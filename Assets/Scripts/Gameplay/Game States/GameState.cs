
namespace C13
{
    public abstract class GameState
    {
        protected GameManager m_gameManager;

        public GameState(GameManager owner) { m_gameManager = owner; }

        public abstract void OnStateEntered();
        public abstract void OnStateUpdate();
        public abstract void OnStateExited();
    }
}
