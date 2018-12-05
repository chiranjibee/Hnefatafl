using UnityEngine;

namespace C13
{
    public enum RookType
    {
        NONE = -1,
        ATTACKER,
        DEFENDER,
        KING
    }

    public class Rook : MonoBehaviour
    {
        public RookType m_rookType;
        public Material m_defaultMaterial;
        public Material m_selectedMaterial;

        [HideInInspector] public Tile m_owner;
        [HideInInspector] public bool m_isSelected = false;
        [HideInInspector] public Vector3 m_prevPosition;

        private Vector3 m_defaultPosition;
        private Tile m_defaultOwner;

        private Renderer m_renderer;

        private void Start()
        {
            m_defaultPosition = gameObject.transform.position;
            SetupRook();
        }

        private void SetupRook()
        {
            m_renderer = GetComponent<Renderer>();
            m_prevPosition = transform.position;
        }

        public void ResetRook()
        {
            if (m_renderer != null)
            {
                m_renderer.material = m_defaultMaterial;
            }

            gameObject.transform.position = m_defaultPosition;
            m_prevPosition = m_defaultPosition;
            m_owner = m_defaultOwner;
        }

        public void SetDefaultOwner(Tile owner)
        {
            m_owner = owner;
            m_defaultOwner = owner;
        }

        private void Update()
        {
            if (m_defaultMaterial != null && m_selectedMaterial != null)
            {
                if (m_isSelected)
                {
                    m_renderer.material = m_selectedMaterial;
                }
                else
                {
                    m_renderer.material = m_defaultMaterial;
                }
            }
        }
    }
}