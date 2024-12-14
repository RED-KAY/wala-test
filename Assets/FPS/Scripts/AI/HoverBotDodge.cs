using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class HoverBotDodge : MonoBehaviour
{
    Health m_Health;

    [Header("Dodge Settings")]
    public float m_Speed = 1f;            
    public float m_Offset = 1f;           
    public AnimationCurve m_EasingCurve;  

    private Vector3 m_InitialPos;   
    private Vector3[] m_Positions;  
    private int m_CurrentIndex = 1; 

    private bool m_IsDodging = false;

    [SerializeField] Transform m_PartToMove;

    private void Start()
    {
        m_Health = GetComponent<Health>();
        m_Health.OnDamaged += OnDamage;

        m_InitialPos = m_PartToMove.localPosition;

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

    public void Dodge()
    {
        if (m_IsDodging)
            return;

        //Debug.Log("Dodge!");

        int[] possibleIndices = { 0, 1, 2 };

        var possibleNewPositions = System.Array.FindAll(possibleIndices, i => i != m_CurrentIndex);
        int newIndex = possibleNewPositions[Random.Range(0, possibleNewPositions.Length)];

        StartCoroutine(DodgeToPosition(newIndex));
    }

    private IEnumerator DodgeToPosition(int targetIndex)
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
