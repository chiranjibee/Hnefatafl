using UnityEngine;

public class Tile : MonoBehaviour
{
    //the X and Y (logical) coordinates of the game board
    public int m_X;
    public int m_Y;

    public bool m_isThrone = false;
    public bool m_isCorner = false;
    public Rook m_ownedRook;

    private void Start()
    {
        if (m_ownedRook != null)
        {
            m_ownedRook.m_owner = this;
        }
    }
}
