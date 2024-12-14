using System;
using Unity.FPS.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Marker DB", menuName = "Enemy Marker/ Create Enemy Marker")]
public class CompassMarkerDatabase : ScriptableObject
{

    [Serializable]
    public struct MarkerEntry
    {
        public MakerType m_Type;
        public CompassMarker m_MarkerPrefab;
    }

    [SerializeField] MarkerEntry[] m_Markers;

    public CompassMarker GetMarkerPrefab(MakerType type)
    {
        foreach (var entry in m_Markers)
        {
            if (entry.m_Type == type)
            {
                return entry.m_MarkerPrefab;
            }
        }
        Debug.LogWarning($"No marker prefab found for EnemyType: {type}");
        return null;
    }
}

public enum MakerType
{
    Direction,
    HoverBot,
    HoverBot2,
    Turret
}