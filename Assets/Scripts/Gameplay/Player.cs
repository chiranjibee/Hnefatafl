using UnityEngine;

namespace C13
{
    public enum PlayerType
    {
        NONE = -1,
        ATTACKER,
        DEFENDER
    }

    public class Player : MonoBehaviour
    {
        [HideInInspector] public bool m_canPlayerTakeAction = false;
        private bool m_hasRook = false;

        public PlayerType m_playerType;

        private Rook m_selectedRook = null;
        private GameManager gameManager;

        private CameraRotation m_cameraRotation;

        private void Start()
        {
            gameManager = GameManager.GetInstance();
            m_cameraRotation = FindObjectOfType<CameraRotation>();
        }

        private void Update()
        {
            // pause game
            if (Input.GetAxis("Cancel") > 0.0f)
            {
                gameManager.m_pauseGame = true;
            }

            // if this player can take actions on the game board
            if (m_canPlayerTakeAction)
            {
                // camera rotation
                if (m_cameraRotation != null)
                {
                    if (Input.GetAxis("Rotate") > 0.0f)
                    {
                        m_cameraRotation.RotateAntiClockwise();
                    }
                    else if (Input.GetAxis("Rotate") < 0.0f)
                    {
                        m_cameraRotation.RotateClockwise();
                    }
                    else
                    {
                        m_cameraRotation.ResetRotation();
                    }
                }

                // mouse clicked and doesn't have a selected rook
                if (Input.GetMouseButtonDown(0) && !m_hasRook)
                {
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        // check if the hit object is a rook
                        Rook tempRook = hit.transform.gameObject.GetComponent<Rook>();
                        if (tempRook != null)
                        {
                            // only attacking player can select the attacker rooks and defending player can select only the defending rooks
                            if ((tempRook.m_rookType == RookType.ATTACKER && m_playerType == PlayerType.ATTACKER) || (tempRook.m_rookType == RookType.DEFENDER && m_playerType == PlayerType.DEFENDER) || (tempRook.m_rookType == RookType.KING && m_playerType == PlayerType.DEFENDER))
                            {
                                SelectRook(tempRook);
                            }
                        }
                    }
                }

                // mouse clicked and has a selected rook
                else if (Input.GetMouseButtonDown(0) && m_hasRook)
                {
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    {
                        // if raycast hit is on tile, check if rook can move to this position
                        Tile tempTile = hit.transform.gameObject.GetComponent<Tile>();
                        if (tempTile != null && gameManager.CanMoveToPosition(m_selectedRook, tempTile))
                        {
                            // move rook to new position
                            UpdateRookOwner(tempTile);
                            UpdateRookPosition(new Vector3(tempTile.transform.position.x, gameManager.m_rookYOffset, tempTile.transform.position.z));
                            gameManager.UpdateMovedRook(m_selectedRook);

                            // update the game board
                            gameManager.UpdateGameBoard();

                            // deselect the moved rook
                            DeselectRook(m_selectedRook.m_prevPosition);
                        }

                        // if raycast hit is on a rook
                        Rook tempRook = hit.transform.gameObject.GetComponent<Rook>();
                        if (tempRook != null)
                        {
                            // deselect the currently selected rook
                            DeselectRook(m_selectedRook.m_prevPosition);
                        }
                    }

                    // no raycast hit, deselect the rook
                    else
                    {
                        DeselectRook(m_selectedRook.m_prevPosition);
                    }
                }
            }
        }

        private void UpdateRookOwner(Tile newTile)
        {
            m_selectedRook.m_owner.m_ownedRook = null;
            newTile.m_ownedRook = m_selectedRook;
            m_selectedRook.m_owner = newTile;
        }

        private void UpdateRookPosition(Vector3 position)
        {
            m_selectedRook.m_prevPosition = position;
        }

        private void SelectRook(Rook rook)
        {
            m_selectedRook = rook;
            m_selectedRook.m_isSelected = true;
            m_hasRook = true;
        }

        private void DeselectRook(Vector3 position)
        {
            m_selectedRook.transform.position = position;
            m_selectedRook.m_isSelected = false;
            m_hasRook = false;
            m_selectedRook = null;
        }

        public void EnableInteraction()
        {
            m_canPlayerTakeAction = true;
        }

        public void DisableInteraction()
        {
            m_canPlayerTakeAction = false;
        }
    }
}
