using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class CompassElement : MonoBehaviour
    {
        //[Tooltip("The marker on the compass for this element")]
        private CompassMarker CompassMarkerPrefab;

        [SerializeField] private MakerType m_MarkerType;
        [Tooltip("Text override for the marker, if it's a direction")]
        public string TextDirection;

        Compass m_Compass;

        CompassMarkerDatabase m_Database;

        void Awake()
        {
            LoadMarkerDatabase();

            m_Compass = FindObjectOfType<Compass>();
            DebugUtility.HandleErrorIfNullFindObject<Compass, CompassElement>(m_Compass, this);

            var markerInstance = Instantiate(CompassMarkerPrefab);

            markerInstance.Initialize(this, TextDirection);
            m_Compass.RegisterCompassElement(transform, markerInstance);
        }

        void LoadMarkerDatabase()
        {
            m_Database = Resources.Load<CompassMarkerDatabase>("Compass/Marker DB");
            CompassMarkerPrefab = m_Database.GetMarkerPrefab(m_MarkerType);
        }

        void OnDestroy()
        {
            m_Compass.UnregisterCompassElement(transform);
        }
    }
}