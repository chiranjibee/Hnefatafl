using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Tile m_owner;
    public bool m_isSelected = false;

    public Vector3 m_prevPosition;
    public Material m_selectedMaterial;

    private Renderer m_renderer;
    private Material m_defaultMaterial;

    private void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_defaultMaterial = m_renderer.material;

        m_prevPosition = transform.position;
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