using UnityEngine;

namespace C13
{
    public class CameraRotation : MonoBehaviour
    {
        public float m_rotationAngle = 30.0f;

        private Quaternion m_defaultRotation;

        private void Start()
        {
            m_defaultRotation = gameObject.transform.rotation;
        }

        public void RotateAntiClockwise()
        {
            Quaternion targetRotation = Quaternion.AngleAxis(-m_rotationAngle, Vector3.up);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, Time.deltaTime);
        }

        public void RotateClockwise()
        {
            Quaternion targetRotation = Quaternion.AngleAxis(m_rotationAngle, Vector3.up);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, targetRotation, Time.deltaTime);
        }

        public void ResetRotation()
        {
            if (gameObject.transform.rotation != m_defaultRotation)
            {
                gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, m_defaultRotation, Time.deltaTime);
            }
        }
    }
}