using UnityEngine;

namespace C13
{
    public class GameState_GameOver : GameState
    {
        private float m_currentTime;

        public GameState_GameOver(GameManager owner) : base(owner) { }

        public override void OnStateEntered()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameOverStateEntered();
            m_currentTime = 0.0f;
        }

        public override void OnStateUpdate()
        {
            if (m_gameManager == null) { return; }
            m_currentTime += Time.deltaTime;

            if (m_currentTime > m_gameManager.m_preGameOverDisplayDuration && m_currentTime < m_gameManager.m_preGameOverDisplayDuration + m_gameManager.m_returnToMainMenuDuration)
            {
                m_gameManager.OnGameOverStateUpdate();
            }
            else if (m_currentTime > m_gameManager.m_preGameOverDisplayDuration + m_gameManager.m_returnToMainMenuDuration)
            {
                m_gameManager.ReturnToMainMenu();
            }
        }

        public override void OnStateExited()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnGameOverStateExited();
        }
    }
}
