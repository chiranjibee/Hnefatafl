using UnityEngine;

namespace C13
{
    public class GameState_SplashScreen : GameState
    {
        private float m_currentTime;

        public GameState_SplashScreen(GameManager owner) : base(owner) { }

        public override void OnStateEntered()
        {
            // todo
            if (m_gameManager == null) { return; }
            m_gameManager.OnSplashScreenEnter();
            m_currentTime = 0.0f;
        }

        public override void OnStateUpdate()
        {
            if (m_gameManager == null) { return; }

            m_currentTime += Time.deltaTime;
            if (m_currentTime > m_gameManager.m_splashScreenDisplayDuration)
            {
                m_gameManager.OnSplashScreenUpdate();
            }
        }

        public override void OnStateExited()
        {
            if (m_gameManager == null) { return; }
            m_gameManager.OnSplashScreenExit();
        }
    }
}
