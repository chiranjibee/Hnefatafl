using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace C13
{
    public class GameManager : MonoBehaviour
    {
        // singleton
        private static GameManager Instance { get; set; }

        private Tile[] m_tiles;         // all the tiles that make up the board
        private Rook[] m_rookPieces;    // all the rook pieces
        private Rook m_lastMoved;       // the rook moved this turn
        private Rook m_king;            // cache a reference to the king

        [SerializeField] private Player m_attackingPlayerPrefab;
        [SerializeField] private Player m_defendingPlayerPrefab;
        [SerializeField] private GameObject m_boardPrefab;
        [SerializeField] private GameObject m_rooksPrefab;

        private Player m_attackingPlayer;
        private Player m_defendingPlayer;

        [SerializeField] private PlayerType m_firstPlay;
        private PlayerType m_currentPlayerTurn;
        private PlayerType m_winner;

        // all possible game states
        private GameState_SplashScreen m_splashScreenState;
        private GameState_MainMenu m_mainMenuState;
        private GameState_InProgress m_gameInProgressGameState;
        private GameState_PauseGame m_pauseGameState;
        private GameState_GameOver m_gameOverGameState;

        [HideInInspector] public bool m_pauseGame = false;

        private GameState m_currentState;
        private GameState m_previousState;
        public float m_rookYOffset = 1.2f;     // offset of all the rook pieces from the board
        public LayerMask m_rookLayerMask;

        // ui panels
        public Transform m_splashScreenPanel;
        public Transform m_mainMenuPanel;
        public Transform m_inGamePanel;
        public Transform m_pauseGamePanel;
        public Transform m_gameOverPanel;

        // ui text objects
        public Text m_currentTurnText;
        public Text m_gameWinnerText;
        public Text m_infoText;

        // scene numbers
        public int m_menuSceneID;
        public int m_gameSceneID;

        // ui appearance durations
        public float m_splashScreenDisplayDuration = 2.0f;
        public float m_preGameOverDisplayDuration = 5.0f;
        public float m_returnToMainMenuDuration = 2.0f;

        private readonly string m_attackingPlayerGameObjectName = "Attacking Player";
        private readonly string m_defendingPlayerGameObjectName = "Defending Player";

        // strings to be used as ui text 
        private readonly string TURN_ATTACKER = "ATTACKER'S TURN";
        private readonly string TURN_DEFENDER = "DEFENDER'S TURN";

        private readonly string WIN_ATTACKER = "ATTACKERS WON THE GAME";
        private readonly string WIN_DEFENDER = "DEFENDERS WON THE GAME";

        public static GameManager GetInstance()
        {
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // another instance of game manager already exists, destroy this and return
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            InitializeGameStates();
            InstantiateGameBoard();
            SetGameState(m_splashScreenState);
        }

        private void Update()
        {
            if (m_currentState == null) { return; }

            m_currentState.OnStateUpdate();
        }

        public void SetGameState(GameState state)
        {
            if (m_currentState != null)
            {
                m_currentState.OnStateExited();
                m_previousState = m_currentState;
            }

            m_currentState = state;
            m_currentState.OnStateEntered();
        }

        public void DisableInput(PlayerType playerType)
        {
            // disable input of the player based on the player type
            switch (playerType)
            {
                case PlayerType.ATTACKER:
                    m_attackingPlayer.DisableInteraction();
                    break;

                case PlayerType.DEFENDER:
                    m_defendingPlayer.DisableInteraction();
                    break;
            }
        }

        public void EnableInput(PlayerType playerType)
        {
            // enable input of the player based on the player type
            switch (playerType)
            {
                case PlayerType.ATTACKER:
                    m_attackingPlayer.EnableInteraction();
                    break;

                case PlayerType.DEFENDER:
                    m_defendingPlayer.EnableInteraction();
                    break;
            }
        }

        public bool CanMoveToPosition(Rook rook, Tile targetPos)
        {
            //evaluate if the rook can be moved to the position
            if ((rook.m_rookType != RookType.KING) && (targetPos.m_isCorner || targetPos.m_isThrone))
            {
                return false;
            }
            else if ((rook.m_rookType == RookType.KING) && (targetPos.m_isCorner || targetPos.m_isThrone))
            {
                return true;
            }

            int dx = Mathf.Abs(targetPos.m_X - rook.m_owner.m_X);
            int dy = Mathf.Abs(targetPos.m_Y - rook.m_owner.m_Y);

            if (dx > 0 && dy > 0)
            {
                return false;
            }
            else if (dx == 0 && dy > 0)
            {
                //check if there is a rook in the path
                int diff = targetPos.m_Y - rook.m_owner.m_Y;
                Tile startTile = rook.m_owner;
                Tile tempTile = null;
                for (int i = 0; i < dy; i++)
                {
                    if (diff > 0)
                    {
                        //move right
                        tempTile = GetTileRight(startTile);
                        if (tempTile != null && tempTile.m_ownedRook != null)
                        {
                            return false;
                        }

                        startTile = tempTile;
                    }
                    else if (diff < 0)
                    {
                        //move left
                        tempTile = GetTileLeft(startTile);
                        if (tempTile != null && tempTile.m_ownedRook != null)
                        {
                            return false;
                        }

                        startTile = tempTile;
                    }
                }

                return true;
            }
            else if (dy == 0 && dx > 0)
            {
                //check if there is a rook in the path
                int diff = targetPos.m_X - rook.m_owner.m_X;
                Tile startTile = rook.m_owner;
                Tile tempTile = null;
                for (int i = 0; i < dx; i++)
                {
                    if (diff > 0)
                    {
                        //move down
                        tempTile = GetTileDown(startTile);
                        if (tempTile != null && tempTile.m_ownedRook != null)
                        {
                            return false;
                        }

                        startTile = tempTile;
                    }
                    else if (diff < 0)
                    {
                        //move up
                        tempTile = GetTileUp(startTile);
                        if (tempTile != null && tempTile.m_ownedRook != null)
                        {
                            return false;
                        }

                        startTile = tempTile;
                    }
                }

                return true;
            }

            return false;
        }

        private void KillRook(Rook rook)
        {
            Tile owner = rook.m_owner;
            if (owner != null && owner.m_ownedRook != null)
            {
                owner.m_ownedRook = null;
                rook.m_owner = null;
                rook.gameObject.SetActive(false);
            }
        }

        private void UpdateAttackerNeighbors(Tile[] neighbors)
        {
            //left
            if (neighbors[0] != null && neighbors[0].m_ownedRook != null)
            {
                if (neighbors[0].m_ownedRook.m_rookType == RookType.DEFENDER)
                {
                    //check 1 tile left for attackers
                    Tile leftTile = GetTileLeft(neighbors[0]);
                    if (leftTile != null && leftTile.m_ownedRook != null)
                    {
                        if (leftTile.m_ownedRook.m_rookType != RookType.DEFENDER && leftTile.m_ownedRook.m_rookType != RookType.KING)
                        {
                            //custodial capture, capture the neighbors[0]
                            KillRook(neighbors[0].m_ownedRook);
                        }
                    }
                    else if (leftTile != null && leftTile.m_ownedRook == null)
                    {
                        if (leftTile.m_isCorner || leftTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[0]
                            KillRook(neighbors[0].m_ownedRook);
                        }
                    }
                }
            }

            //right
            if (neighbors[1] != null && neighbors[1].m_ownedRook != null)
            {
                if (neighbors[1].m_ownedRook.m_rookType == RookType.DEFENDER)
                {
                    //check 1 tile left for attackers
                    Tile rightTile = GetTileRight(neighbors[1]);
                    if (rightTile != null && rightTile.m_ownedRook != null)
                    {
                        if (rightTile.m_ownedRook.m_rookType != RookType.DEFENDER && rightTile.m_ownedRook.m_rookType != RookType.KING)
                        {
                            //custodial capture, capture the neighbors[1]
                            KillRook(neighbors[1].m_ownedRook);
                        }
                    }
                    else if (rightTile != null && rightTile.m_ownedRook == null)
                    {
                        if (rightTile.m_isCorner || rightTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[1]
                            KillRook(neighbors[1].m_ownedRook);
                        }
                    }
                }
            }

            //up
            if (neighbors[2] != null && neighbors[2].m_ownedRook != null)
            {
                if (neighbors[2].m_ownedRook.m_rookType == RookType.DEFENDER)
                {
                    //check 1 tile left for attackers
                    Tile upTile = GetTileUp(neighbors[2]);
                    if (upTile != null && upTile.m_ownedRook != null)
                    {
                        if (upTile.m_ownedRook.m_rookType != RookType.DEFENDER && upTile.m_ownedRook.m_rookType != RookType.KING)
                        {
                            //custodial capture, capture the neighbors[2]
                            KillRook(neighbors[2].m_ownedRook);
                        }
                    }
                    else if (upTile != null && upTile.m_ownedRook == null)
                    {
                        if (upTile.m_isCorner || upTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[2]
                            KillRook(neighbors[2].m_ownedRook);
                        }
                    }
                }
            }

            //down
            if (neighbors[3] != null && neighbors[3].m_ownedRook != null)
            {
                if (neighbors[3].m_ownedRook.m_rookType == RookType.DEFENDER)
                {
                    //check 1 tile left for attackers
                    Tile downTile = GetTileDown(neighbors[3]);
                    if (downTile != null && downTile.m_ownedRook != null)
                    {
                        if (downTile.m_ownedRook.m_rookType != RookType.DEFENDER && downTile.m_ownedRook.m_rookType != RookType.KING)
                        {
                            //custodial capture, capture the neighbors[3]
                            KillRook(neighbors[3].m_ownedRook);
                        }
                    }
                    else if (downTile != null && downTile.m_ownedRook == null)
                    {
                        if (downTile.m_isCorner || downTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[3]
                            KillRook(neighbors[3].m_ownedRook);
                        }
                    }
                }
            }
        }

        private void UpdateDefenderNeighbors(Tile[] neighbors)
        {
            //left
            if (neighbors[0] != null && neighbors[0].m_ownedRook != null)
            {
                if (neighbors[0].m_ownedRook.m_rookType != RookType.DEFENDER && neighbors[0].m_ownedRook.m_rookType != RookType.KING)
                {
                    //check 1 tile left for defenders
                    Tile leftTile = GetTileLeft(neighbors[0]);
                    if (leftTile != null && leftTile.m_ownedRook != null)
                    {
                        if (leftTile.m_ownedRook.m_rookType == RookType.DEFENDER || leftTile.m_ownedRook.m_rookType == RookType.KING)
                        {
                            //custodial capture, capture the neighbors[0]
                            KillRook(neighbors[0].m_ownedRook);
                        }
                    }
                    else if (leftTile != null && leftTile.m_ownedRook == null)
                    {
                        if (leftTile.m_isCorner || leftTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[0]
                            KillRook(neighbors[0].m_ownedRook);
                        }
                    }
                }
            }

            //right
            if (neighbors[1] != null && neighbors[1].m_ownedRook != null)
            {
                if (neighbors[1].m_ownedRook.m_rookType != RookType.DEFENDER && neighbors[1].m_ownedRook.m_rookType != RookType.KING)
                {
                    //check 1 tile right for defenders
                    Tile rightTile = GetTileRight(neighbors[1]);
                    if (rightTile != null && rightTile.m_ownedRook != null)
                    {
                        if (rightTile.m_ownedRook.m_rookType == RookType.DEFENDER || rightTile.m_ownedRook.m_rookType == RookType.KING)
                        {
                            //custodial capture, capture the neighbors[1]
                            KillRook(neighbors[1].m_ownedRook);
                        }
                    }
                    else if (rightTile != null && rightTile.m_ownedRook == null)
                    {
                        if (rightTile.m_isCorner || rightTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[1]
                            KillRook(neighbors[1].m_ownedRook);
                        }
                    }
                }
            }

            //up
            if (neighbors[2] != null && neighbors[2].m_ownedRook != null)
            {
                if (neighbors[2].m_ownedRook.m_rookType != RookType.DEFENDER && neighbors[2].m_ownedRook.m_rookType != RookType.KING)
                {
                    //check 1 tile up for defenders
                    Tile upTile = GetTileUp(neighbors[2]);
                    if (upTile != null && upTile.m_ownedRook != null)
                    {
                        if (upTile.m_ownedRook.m_rookType == RookType.DEFENDER || upTile.m_ownedRook.m_rookType == RookType.KING)
                        {
                            //custodial capture, capture the neighbors[2]
                            KillRook(neighbors[2].m_ownedRook);
                        }
                    }
                    else if (upTile != null && upTile.m_ownedRook == null)
                    {
                        if (upTile.m_isCorner || upTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[2]
                            KillRook(neighbors[2].m_ownedRook);
                        }
                    }
                }
            }

            //down
            if (neighbors[3] != null && neighbors[3].m_ownedRook != null)
            {
                if (neighbors[3].m_ownedRook.m_rookType != RookType.DEFENDER && neighbors[3].m_ownedRook.m_rookType != RookType.KING)
                {
                    //check 1 tile down for defenders
                    Tile downTile = GetTileDown(neighbors[3]);
                    if (downTile != null && downTile.m_ownedRook != null)
                    {
                        if (downTile.m_ownedRook.m_rookType == RookType.DEFENDER || downTile.m_ownedRook.m_rookType == RookType.KING)
                        {
                            //custodial capture, capture the neighbors[3]
                            KillRook(neighbors[3].m_ownedRook);
                        }
                    }
                    else if (downTile != null && downTile.m_ownedRook == null)
                    {
                        if (downTile.m_isCorner || downTile.m_isThrone)
                        {
                            //custodial capture, capture the neighbors[3]
                            KillRook(neighbors[3].m_ownedRook);
                        }
                    }
                }
            }
        }

        private void UpdateGameStatus()
        {
            if (m_king != null)
            {
                // check defender win condition
                Tile tempTile = m_king.m_owner;
                if (tempTile != null)
                {
                    if (tempTile.m_isCorner)
                    {
                        m_winner = PlayerType.DEFENDER;
                        SetGameState(m_gameOverGameState);
                        return;
                    }
                }

                //check defender lose condition
                Tile[] kingNeighbors = GetNeighbors(m_king);    //left = 0, right = 1, up = 2, down = 3
                int attackers = 0;
                bool kingEdged = false;

                //left
                if (kingNeighbors[0] != null)
                {
                    if (kingNeighbors[0].m_ownedRook != null && kingNeighbors[0].m_ownedRook.m_rookType != RookType.DEFENDER)
                    {
                        attackers++;
                    }
                }
                else
                {
                    kingEdged = true;
                }

                //right
                if (kingNeighbors[1] != null)
                {
                    if (kingNeighbors[1].m_ownedRook != null && kingNeighbors[1].m_ownedRook.m_rookType != RookType.DEFENDER)
                    {
                        attackers++;
                    }
                }
                else
                {
                    kingEdged = true;
                }

                //up
                if (kingNeighbors[2] != null)
                {
                    if (kingNeighbors[2].m_ownedRook != null && kingNeighbors[2].m_ownedRook.m_rookType != RookType.DEFENDER)
                    {
                        attackers++;
                    }
                }
                else
                {
                    kingEdged = true;
                }

                //down
                if (kingNeighbors[3] != null)
                {
                    if (kingNeighbors[3].m_ownedRook != null && kingNeighbors[3].m_ownedRook.m_rookType != RookType.DEFENDER)
                    {
                        attackers++;
                    }
                }
                else
                {
                    kingEdged = true;
                }

                if (kingEdged && attackers == 3)
                {
                    // king is trapped on the edge
                    KillRook(m_king);
                    m_winner = PlayerType.ATTACKER;
                    SetGameState(m_gameOverGameState);
                }
                else if (attackers == 4)
                {
                    // king is captured
                    KillRook(m_king);
                    m_winner = PlayerType.ATTACKER;
                    SetGameState(m_gameOverGameState);
                }
            }
        }

        private void SwitchPlayerTurns()
        {
            switch (m_currentPlayerTurn)
            {
                case PlayerType.ATTACKER:
                    DisableInput(PlayerType.ATTACKER);
                    m_currentPlayerTurn = PlayerType.DEFENDER;
                    EnableInput(m_currentPlayerTurn);
                    break;

                case PlayerType.DEFENDER:
                    DisableInput(PlayerType.DEFENDER);
                    m_currentPlayerTurn = PlayerType.ATTACKER;
                    EnableInput(m_currentPlayerTurn);
                    break;
            }

            UpdatePlayerTurnUI();
        }

        public void UpdateGameBoard()
        {
            if (m_lastMoved != null)
            {
                // get the 4 neighbors of the last moved tile
                Tile[] neighbors = GetNeighbors(m_lastMoved);   //left = 0, right = 1, up = 2, down = 3
                if (m_lastMoved.m_rookType != RookType.DEFENDER && m_lastMoved.m_rookType != RookType.KING)
                {
                    // it was attacker move
                    UpdateAttackerNeighbors(neighbors);
                }
                else if (m_lastMoved.m_rookType == RookType.DEFENDER || m_lastMoved.m_rookType == RookType.KING)
                {
                    // it was defender move
                    UpdateDefenderNeighbors(neighbors);
                }

                UpdateGameStatus();
                SwitchPlayerTurns();
            }
        }

        public void OnSplashScreenEnter()
        {
            DisplaySplashScreen(true);
        }

        public void OnSplashScreenUpdate()
        {
            SceneManager.LoadScene(m_menuSceneID);
            SetGameState(m_mainMenuState);
        }

        public void OnSplashScreenExit()
        {
            DisplaySplashScreen(false);
        }

        public void OnMainMenuEnter()
        {
            if (m_previousState != m_splashScreenState)
            {
                DestroyGameBoard();
            }

            DisplayMainMenu(true);
        }

        public void OnMainMenuExit()
        {
            DisplayMainMenu(false);
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(m_gameSceneID);
            EnableGameBoard();
            SetGameState(m_gameInProgressGameState);
        }

        public void OnGameInProgressEnter()
        {
            if (m_previousState != m_pauseGameState)
            {
                EnableGameBoard();
                SetupGameData();
            }

            EnableInput(m_currentPlayerTurn);
            DisplayInGameMenu(true);
        }

        public void OnGameInProgressUpdate()
        {
            if (m_pauseGame)
            {
                SetGameState(m_pauseGameState);
            }
        }

        public void OnGameInProgressExit()
        {
            DisplayInGameMenu(false);
        }

        public void OnGameInPauseEnter()
        {
            DisplayPauseGameMenu(true);
            DisableInput(PlayerType.ATTACKER);
            DisableInput(PlayerType.DEFENDER);
        }

        public void OnGameInPauseUpdate()
        {
            if (!m_pauseGame)
            {
                SetGameState(m_gameInProgressGameState);
            }
        }

        public void OnGameInPauseExit()
        {
            m_pauseGame = false;
            EnableInput(m_currentPlayerTurn);
            DisplayPauseGameMenu(false);
        }

        public void OnGameOverStateEntered()
        {
            DisplayGameOverMenu(true);
            DisableInput(PlayerType.ATTACKER);
            DisableInput(PlayerType.DEFENDER);
            UpdateWinnerTextUI();
        }

        public void OnGameOverStateUpdate()
        {
            DisplayGameOverMenu(true);
        }

        public void OnGameOverStateExited()
        {
            DisplayGameOverMenu(false);
            DisableGameBoard();
        }

        public void ToggleInfoText()
        {
            m_infoText.enabled = !m_infoText.enabled;
        }

        public void ResumeGame()
        {
            if (m_pauseGame) { m_pauseGame = false; }
        }

        public void ReturnToMainMenu()
        {
            m_winner = PlayerType.NONE;
            m_currentPlayerTurn = m_firstPlay;
            UpdatePlayerTurnUI();
            DisableInput(PlayerType.ATTACKER);
            DisableInput(PlayerType.DEFENDER);
            SetGameState(m_mainMenuState);
            SceneManager.LoadScene(m_menuSceneID);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void InstantiateGameBoard()
        {
            m_attackingPlayer = Instantiate(m_attackingPlayerPrefab).GetComponent<Player>();
            m_attackingPlayer.gameObject.name = m_attackingPlayerGameObjectName;
            m_attackingPlayer.gameObject.transform.parent = gameObject.transform;
            m_defendingPlayer = Instantiate(m_defendingPlayerPrefab).GetComponent<Player>();
            m_defendingPlayer.gameObject.name = m_defendingPlayerGameObjectName;
            m_defendingPlayer.gameObject.transform.parent = gameObject.transform;

            if (m_rooksPrefab != null)
            {
                GameObject tRooksPrefab = Instantiate<GameObject>(m_rooksPrefab);
                tRooksPrefab.transform.parent = gameObject.transform;
                m_rookPieces = tRooksPrefab.GetComponentsInChildren<Rook>();

                for (int i = 0; i < m_rookPieces.Length; i++)
                {
                    if (m_rookPieces[i].m_rookType == RookType.KING)
                    {
                        m_king = m_rookPieces[i];
                        break;
                    }
                }
            }

            if (m_boardPrefab != null)
            {
                GameObject tBoardPrefab = Instantiate<GameObject>(m_boardPrefab);
                tBoardPrefab.transform.parent = gameObject.transform;
                m_tiles = tBoardPrefab.GetComponentsInChildren<Tile>();

                for (int i = 0; i < m_tiles.Length; i++)
                {
                    m_tiles[i].InitializeTile();
                }
            }

            DisableGameBoard();
        }

        private void DisableGameBoard()
        {
            for (int i = 0; i < m_rookPieces.Length; i++)
            {
                m_rookPieces[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < m_tiles.Length; i++)
            {
                m_tiles[i].gameObject.SetActive(false);
            }
        }

        private void EnableGameBoard()
        {
            for (int i = 0; i < m_rookPieces.Length; i++)
            {
                m_rookPieces[i].gameObject.SetActive(true);
            }

            for (int i = 0; i < m_tiles.Length; i++)
            {
                m_tiles[i].gameObject.SetActive(true);
            }
        }

        public void DestroyGameBoard()
        {
            if (m_rookPieces != null && m_tiles != null)
            {
                for (int i = 0; i < m_rookPieces.Length; i++)
                {
                    if (m_rookPieces[i] != null)
                    {
                        m_rookPieces[i].ResetRook();
                    }
                }

                for (int i = 0; i < m_tiles.Length; i++)
                {
                    if (m_tiles[i] != null)
                    {
                        m_tiles[i].ResetTile();
                    }
                }
            }
        }

        private void SetupGameData()
        {
            m_pauseGame = false;
            m_winner = PlayerType.NONE;
            m_currentPlayerTurn = m_firstPlay;
        }

        private void DisplaySplashScreen(bool display)
        {
            if (m_splashScreenPanel != null)
            {
                m_splashScreenPanel.gameObject.SetActive(display);
            }
        }

        private void DisplayMainMenu(bool display)
        {
            if (m_mainMenuPanel != null)
            {
                m_mainMenuPanel.gameObject.SetActive(display);
            }
        }

        private void DisplayInGameMenu(bool display)
        {
            if (m_inGamePanel != null)
            {
                m_inGamePanel.gameObject.SetActive(display);
            }
        }

        private void DisplayPauseGameMenu(bool display)
        {
            if (m_pauseGamePanel != null)
            {
                m_pauseGamePanel.gameObject.SetActive(display);
            }
        }

        private void DisplayGameOverMenu(bool display)
        {
            if (m_gameOverPanel != null)
            {
                m_gameOverPanel.gameObject.SetActive(display);
            }
        }

        private void UpdatePlayerTurnUI()
        {
            switch (m_currentPlayerTurn)
            {
                case PlayerType.ATTACKER:
                    m_currentTurnText.text = TURN_ATTACKER;
                    break;

                case PlayerType.DEFENDER:
                    m_currentTurnText.text = TURN_DEFENDER;
                    break;
            }
        }

        private void UpdateWinnerTextUI()
        {
            switch (m_winner)
            {
                case PlayerType.ATTACKER:
                    m_gameWinnerText.text = WIN_ATTACKER;
                    break;

                case PlayerType.DEFENDER:
                    m_gameWinnerText.text = WIN_DEFENDER;
                    break;
            }
        }

        private Tile[] GetNeighbors(Rook rook)
        {
            Tile[] neighbors = new Tile[4];

            Tile tempTile = GetTileLeft(rook.m_owner);
            if (tempTile != null)
            {
                neighbors[0] = tempTile;
            }
            else
            {
                neighbors[0] = null;
            }

            tempTile = GetTileRight(rook.m_owner);
            if (tempTile != null)
            {
                neighbors[1] = tempTile;
            }
            else
            {
                neighbors[1] = null;
            }

            tempTile = GetTileUp(rook.m_owner);
            if (tempTile != null)
            {
                neighbors[2] = tempTile;
            }
            else
            {
                neighbors[2] = null;
            }

            tempTile = GetTileDown(rook.m_owner);
            if (tempTile != null)
            {
                neighbors[3] = tempTile;
            }
            else
            {
                neighbors[3] = null;
            }

            return neighbors;
        }

        public void UpdateMovedRook(Rook rook)
        {
            m_lastMoved = rook;
        }

        private Tile GetTileUp(Tile currentTile)
        {
            for (int i = 0; i < m_tiles.Length; i++)
            {
                //X-1, Y
                if ((m_tiles[i].m_X == (currentTile.m_X - 1)) && (m_tiles[i].m_Y == currentTile.m_Y))
                {
                    return m_tiles[i];
                }
            }

            return null;
        }

        private Tile GetTileDown(Tile currentTile)
        {
            for (int i = 0; i < m_tiles.Length; i++)
            {
                //X+1, Y
                if ((m_tiles[i].m_X == (currentTile.m_X + 1)) && (m_tiles[i].m_Y == currentTile.m_Y))
                {
                    return m_tiles[i];
                }
            }

            return null;
        }

        private Tile GetTileLeft(Tile currentTile)
        {
            for (int i = 0; i < m_tiles.Length; i++)
            {
                //X, Y-1
                if ((m_tiles[i].m_X == currentTile.m_X) && (m_tiles[i].m_Y == (currentTile.m_Y - 1)))
                {
                    return m_tiles[i];
                }
            }

            return null;
        }

        private Tile GetTileRight(Tile currentTile)
        {
            for (int i = 0; i < m_tiles.Length; i++)
            {
                //X, Y+1
                if ((m_tiles[i].m_X == currentTile.m_X) && (m_tiles[i].m_Y == (currentTile.m_Y + 1)))
                {
                    return m_tiles[i];
                }
            }

            return null;
        }

        private void InitializeGameStates()
        {
            m_splashScreenState = new GameState_SplashScreen(this);
            m_mainMenuState = new GameState_MainMenu(this);
            m_gameInProgressGameState = new GameState_InProgress(this);
            m_pauseGameState = new GameState_PauseGame(this);
            m_gameOverGameState = new GameState_GameOver(this);
        }
    }
}
