using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class HoverBotDodge : MonoBehaviour
{
    Health m_Health;

    [Header("Dodge Settings")]
    public float m_Speed = 1f;            // How fast to move between positions
    public float m_Offset = 1f;           // Vertical distance from initial position
    public AnimationCurve m_EasingCurve;  // Easing curve for smoother movement

    private Vector3 m_InitialPos;   // The starting position of the robot
    private Vector3[] m_Positions;  // The three possible positions: [down, initial, up]
    private int m_CurrentIndex = 1; // 0 = down, 1 = initial, 2 = up (default to initial)

    private bool m_IsDodging = false;

    [SerializeField] Transform m_PartToMove;

    private void Start()
    {
        m_Health = GetComponent<Health>();
        m_Health.OnDamaged += OnDamage;

        // Record the initial position from the prefab
        m_InitialPos = m_PartToMove.localPosition;

        // Calculate the three possible positions
        m_Positions = new Vector3[3];
        m_Positions[0] = m_InitialPos - Vector3.up * m_Offset;
        m_Positions[1] = m_InitialPos;
        m_Positions[2] = m_InitialPos + Vector3.up * m_Offset;
    }

    private void OnDisable()
    {
        m_Health.OnDamaged -= OnDamage;
    }

    void OnDamage(float damage, GameObject damageSource)
    {
        //Debug.Log("OnDamage! ," + damageSource.name);
        Dodge();
    }

    // Call this method to trigger a dodge. It will choose one of the two other positions.
    public void Dodge()
    {
        if (m_IsDodging)
            return;

        //Debug.Log("Dodge!");

        // Find a random other position that is not the current one.
        // There are always two choices: if current = 1 (initial), then picks either 0 (down) or 2 (up).
        // If current = 0 or 2, picks from {1, (the other non-current position)}.

        int[] possibleIndices = { 0, 1, 2 };
        // Filter out the current index
        var possibleNewPositions = System.Array.FindAll(possibleIndices, i => i != m_CurrentIndex);
        int newIndex = possibleNewPositions[Random.Range(0, possibleNewPositions.Length)];

        StartCoroutine(DodgeToPosition(newIndex));
    }

    private System.Collections.IEnumerator DodgeToPosition(int targetIndex)
    {
        m_IsDodging = true;

        Vector3 startPos = m_PartToMove.localPosition;
        Vector3 endPos = m_Positions[targetIndex];

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * m_Speed;
            float t = m_EasingCurve.Evaluate(time);
            m_PartToMove.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        m_PartToMove.localPosition = endPos;
        m_CurrentIndex = targetIndex;
        m_IsDodging = false;
    }
}
