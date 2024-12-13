using System.Threading;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class Jetpack : MonoBehaviour
    {
        [Header("References")] [Tooltip("Audio source for jetpack sfx")]
        public AudioSource AudioSource;

        [Tooltip("Particles for jetpack vfx")] public ParticleSystem[] JetpackVfx;

        [Header("Parameters")] [Tooltip("Whether the jetpack is unlocked at the begining or not")]
        public bool IsJetpackUnlockedAtStart = false;

        [Tooltip("The strength with which the jetpack pushes the player up")]
        public float JetpackAcceleration = 7f;

        [Range(0f, 1f)]
        [Tooltip(
            "This will affect how much using the jetpack will cancel the gravity value, to start going up faster. 0 is not at all, 1 is instant")]
        public float JetpackDownwardVelocityCancelingFactor = 1f;

        [Header("Durations")] [Tooltip("Time it takes to consume all the jetpack fuel")]
        public float ConsumeDuration = 1.5f;

        [Tooltip("Time it takes to completely refill the jetpack while on the ground")]
        public float RefillDurationGrounded = 2f;

        [Tooltip("Time it takes to completely refill the jetpack while in the air")]
        public float RefillDurationInTheAir = 5f;

        [Tooltip("Delay after last use before starting to refill")]
        public float RefillDelay = 1f;

        [Header("Audio")] [Tooltip("Sound played when using the jetpack")]
        public AudioClip JetpackSfx;

        bool m_CanUseJetpack;
        PlayerCharacterController m_PlayerCharacterController;
        PlayerInputHandler m_InputHandler;
        float m_LastTimeOfUse;

        // stored ratio for jetpack resource (1 is full, 0 is empty)
        public float CurrentFillRatio { get; private set; }
        public bool IsJetpackUnlocked { get; private set; }

        public bool IsPlayergrounded() => m_PlayerCharacterController.IsGrounded;

        public UnityAction<bool> OnUnlockJetpack;

        [Header("Slam")]
        private bool m_SlammingDown;
        private bool m_JetpackIsInUse;
        [SerializeField] float m_SlamTimeWindow = 0.5f;
        [SerializeField] float m_SlamDamageMultiplier, m_SlamBaseDamage;
        float m_SlamDamageToApply;
        public AudioSource m_SlammingAudioSource;
        public AudioClip m_SlamSFX;

        Health m_Health;

        void Start()
        {
            IsJetpackUnlocked = IsJetpackUnlockedAtStart;

            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, Jetpack>(m_PlayerCharacterController,
                this, gameObject);

            m_InputHandler = GetComponent<PlayerInputHandler>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerInputHandler, Jetpack>(m_InputHandler, this, gameObject);

            CurrentFillRatio = 1f;

            AudioSource.clip = JetpackSfx;
            AudioSource.loop = true;

            m_SlammingAudioSource.clip = m_SlamSFX;

            m_PlayerCharacterController.OnStanceChanged += OnStanceChanged;

            m_Health = GetComponent<Health>();
        }

        private void OnDisable()
        {
            m_PlayerCharacterController.OnStanceChanged -= OnStanceChanged;
        }

        void Update()
        {
            // jetpack can only be used if not grounded and jump has been pressed again once in-air
            if (IsPlayergrounded())
            {
                m_CanUseJetpack = false;
                if (m_SlammingDown)
                {
                    m_SlammingDown = false;
                    m_Health.TakeDamage(m_SlamDamageToApply, this.gameObject);

                    m_SlammingAudioSource.Play();

                    //Debug.Log("Slam damage to apply: " + m_SlamDamageToApply);
                }
            }
            else if (!m_PlayerCharacterController.HasJumpedThisFrame && m_InputHandler.GetJumpInputDown() && !m_SlammingDown)
            {
                m_CanUseJetpack = true;
            }

            // jetpack usage
            m_JetpackIsInUse = m_CanUseJetpack && IsJetpackUnlocked && CurrentFillRatio > 0f &&
                                  m_InputHandler.GetJumpInputHeld();
            if (m_JetpackIsInUse)
            {
                // store the last time of use for refill delay
                m_LastTimeOfUse = Time.time;

                float totalAcceleration = JetpackAcceleration;

                // cancel out gravity
                totalAcceleration += m_PlayerCharacterController.GravityDownForce;

                if (m_PlayerCharacterController.CharacterVelocity.y < 0f)
                {
                    // handle making the jetpack compensate for character's downward velocity with bonus acceleration
                    totalAcceleration += ((-m_PlayerCharacterController.CharacterVelocity.y / Time.deltaTime) *
                                          JetpackDownwardVelocityCancelingFactor);
                }

                // apply the acceleration to character's velocity
                m_PlayerCharacterController.CharacterVelocity += Vector3.up * totalAcceleration * Time.deltaTime;

                // consume fuel
                CurrentFillRatio = CurrentFillRatio - (Time.deltaTime / ConsumeDuration);

                for (int i = 0; i < JetpackVfx.Length; i++)
                {
                    var emissionModulesVfx = JetpackVfx[i].emission;
                    emissionModulesVfx.enabled = true;
                }

                if (!AudioSource.isPlaying)
                    AudioSource.Play();
            }
            else
            {
                // refill the meter over time
                if (IsJetpackUnlocked && Time.time - m_LastTimeOfUse >= RefillDelay)
                {
                    float refillRate = 1 / (m_PlayerCharacterController.IsGrounded
                        ? RefillDurationGrounded
                        : RefillDurationInTheAir);
                    CurrentFillRatio = CurrentFillRatio + Time.deltaTime * refillRate;
                }

                for (int i = 0; i < JetpackVfx.Length; i++)
                {
                    var emissionModulesVfx = JetpackVfx[i].emission;
                    emissionModulesVfx.enabled = false;
                }

                // keeps the ratio between 0 and 1
                CurrentFillRatio = Mathf.Clamp01(CurrentFillRatio);

                if (AudioSource.isPlaying)
                    AudioSource.Stop();
            }
        }

        public bool TryUnlock()
        {
            if (IsJetpackUnlocked)
                return false;

            OnUnlockJetpack.Invoke(true);
            IsJetpackUnlocked = true;
            m_LastTimeOfUse = Time.time;
            return true;
        }

        void OnStanceChanged(bool crouched)
        {
            float delta = Time.time - m_LastTimeOfUse;
            bool cond1 = m_JetpackIsInUse && crouched;
            bool cond2 = crouched && (delta <= m_SlamTimeWindow);
            //Debug.Log("Jetpack In Use: " + m_JetpackIsInUse + ", Crouched: " + crouched + ", delta: " + delta + ", cond2: " + cond2.ToString() + ", cond1: " + cond1.ToString());
            if (cond1 || cond2)
            {
                //Debug.Log("Slam down!");
                m_SlammingDown = true;

                Ray r = new Ray(transform.position, -1 * transform.up);
                RaycastHit hit = new RaycastHit();
                if(Physics.Raycast(r, out hit, Mathf.Infinity, m_PlayerCharacterController.GroundCheckLayers))
                {
                    float slamHeight = hit.distance;
                    //Debug.Log("SlamHeight: " + slamHeight);
                    m_SlamDamageToApply = m_SlamBaseDamage * Mathf.Pow(slamHeight, 2) * m_SlamDamageMultiplier;
                }

                m_PlayerCharacterController.CharacterVelocity = new Vector3(
                    m_PlayerCharacterController.CharacterVelocity.x,
                    -20f, 
                    m_PlayerCharacterController.CharacterVelocity.z
                );
            }
        }
    }
}