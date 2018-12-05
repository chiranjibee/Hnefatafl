using UnityEngine;

namespace C13
{
    public class Tile : MonoBehaviour
    {
        //the X and Y (logical) coordinates of the game board
        public int m_X;
        public int m_Y;

        public bool m_isThrone = false;
        public bool m_isCorner = false;
        public Rook m_ownedRook;

        public void InitializeTile()
        {
            RaycastHit hitManifold;
            if (Physics.Raycast(gameObject.transform.position, Vector3.up, out hitManifold, 5.0f, GameManager.GetInstance().m_rookLayerMask))
            {
                Rook rook = hitManifold.collider.gameObject.GetComponent<Rook>();
                if (rook != null)
                {
                    m_ownedRook = rook;
                    rook.SetDefaultOwner(this);
                }
            }
        }

        public void ResetTile()
        {
            m_ownedRook = null;
            InitializeTile();
        }
    }
}
