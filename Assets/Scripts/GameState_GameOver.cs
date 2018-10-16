using UnityEngine;

public class GameState_GameOver : GameState
{
    private float m_currentTime;

    public GameState_GameOver(GameManager owner) : base(owner) {}

    public override void OnStateEntered()
    {
        m_gameManager.OnGameOverStateEntered();
        m_currentTime = 0.0f;
    }

    public override void OnStateUpdate()
    {
        if (m_currentTime < m_gameManager.m_preGameOverDisplayDuration)
        {
            m_currentTime += Time.deltaTime;
        }
        else
        {
            m_gameManager.OnGameOverStateUpdate();
        }
    }

    public override void OnStateExited()
    {
        m_gameManager.OnGameOverStateExited();
    }
}
