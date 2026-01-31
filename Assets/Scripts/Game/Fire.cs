using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField]
    private float _healthPoints = 100;
    [SerializeField]
    private ParticleSystem _fireParticles;
    [SerializeField]
    private ParticleSystem _smokeParticles;
    [SerializeField]
    private Light _fireLight;
    [SerializeField]
    private AudioSource _fireSource;
    [SerializeField]
    private AudioSource _fireHiss;
    [SerializeField]
    private AudioSource _fireExtinguished;

    [SerializeField]
    private int _persistentFireId = -1;

    private float _initialFireEmissionRate;
    private float _initialSmokeEmissionRate;
    private float _initialLightIntensity;
    private float _initialHealthPoints;
    private bool _isBeingExtinguished;
    private int _fireIndex = -1;
    private static int _nextFireIndex = 0;
    private bool _hasBeenFullyExtinguished = false;
    private bool _isInPlaybackMode = false;

    public bool IsExtinguished => _healthPoints <= 0;
    public float HealthPoints => _healthPoints;

    private void Awake()
    {
        _initialFireEmissionRate = _fireParticles.emission.rateOverTime.constant;
        _initialSmokeEmissionRate = _smokeParticles.emission.rateOverTime.constant;
        _initialLightIntensity = _fireLight.intensity;
        _initialHealthPoints = _healthPoints;

        if (_persistentFireId >= 0)
        {
            _fireIndex = _persistentFireId;
            if (_persistentFireId >= _nextFireIndex)
            {
                _nextFireIndex = _persistentFireId + 1;
            }
        }
        else
        {
            _fireIndex = _nextFireIndex++;
            _persistentFireId = _fireIndex;
        }
    }

    public int GetFireIndex()
    {
        return _fireIndex;
    }

    public void SetPlaybackMode(bool enabled)
    {
        _isInPlaybackMode = enabled;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetFireIndexCounter()
    {
        _nextFireIndex = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_isInPlaybackMode) return;

        if (other.TryGetComponent(out Extinguisher extinguisher))
        {
            float damage = 35f * Time.deltaTime;
            Extinguish(damage);
            _isBeingExtinguished = true;

            if (extinguisher.Recording != null)
            {
                extinguisher.Recording.RecordFireState(
                    GameManager.Instance.LevelTimer,
                    _fireIndex,
                    _healthPoints
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isBeingExtinguished)
        {
            if (!_fireHiss.isPlaying)
                _fireHiss.Play();
        }
        else
        {
            if (_fireHiss.isPlaying)
                _fireHiss.Stop();
        }
        _isBeingExtinguished = false;
    }

    private void LateUpdate()
    {
        UpdateVisualEffects();
    }

    public void Extinguish(float damage)
    {
        _healthPoints -= damage;
        _healthPoints = Mathf.Max(0, _healthPoints);

        if (_healthPoints <= 0 && !_hasBeenFullyExtinguished)
        {
            _hasBeenFullyExtinguished = true;
            OnFireFullyExtinguished();
        }
    }

    public void SetHealthForPlayback(float health)
    {
        float previousHealth = _healthPoints;
        _healthPoints = Mathf.Max(0, health);

        if (previousHealth > 0 && _healthPoints <= 0 && !_hasBeenFullyExtinguished)
        {
            _hasBeenFullyExtinguished = true;
            OnFireFullyExtinguished();
        }
    }

    private void UpdateVisualEffects()
    {
        if (_healthPoints <= 0) return;

        float healthPercentage = _healthPoints / _initialHealthPoints;

        var fireEmission = _fireParticles.emission;
        var fireRate = fireEmission.rateOverTime;
        fireRate.constant = _initialFireEmissionRate * healthPercentage;
        fireEmission.rateOverTime = fireRate;

        var smokeEmission = _smokeParticles.emission;
        var smokeRate = smokeEmission.rateOverTime;
        smokeRate.constant = _initialSmokeEmissionRate * (1 - healthPercentage * 0.5f);
        smokeEmission.rateOverTime = smokeRate;

        _fireLight.intensity = _initialLightIntensity * healthPercentage;
    }

    private void OnFireFullyExtinguished()
    {
        _fireParticles.Stop();
        _fireLight.enabled = false;

        if (!_fireExtinguished.isPlaying)
        {
            _fireExtinguished.Play();
        }

        Invoke(nameof(StopSmoke), _fireExtinguished.clip.length);
        NotifyFireExtinguished();
    }

    private void StopSmoke()
    {
        _smokeParticles.Stop();
    }

    private void NotifyFireExtinguished()
    {
        if (_isInPlaybackMode) return;

        if (GameManager.Instance != null)
        {
            var task = GameManager.Instance.GetTask("firefighter_extinguish_fires");
            if (task != null && !task.IsCompleted)
            {
                task.IncrementProgress(1);
                Debug.Log($"Fire {_fireIndex} extinguished! Task progress updated.");
            }
        }
    }

    public void ResetFire()
    {
        _healthPoints = _initialHealthPoints;
        _fireLight.intensity = _initialLightIntensity;
        _fireLight.enabled = true;

        var fireEmission = _fireParticles.emission;
        var fireRate = fireEmission.rateOverTime;
        fireRate.constant = _initialFireEmissionRate;
        fireEmission.rateOverTime = fireRate;

        var smokeEmission = _smokeParticles.emission;
        var smokeRate = smokeEmission.rateOverTime;
        smokeRate.constant = _initialSmokeEmissionRate;
        smokeEmission.rateOverTime = smokeRate;

        _fireParticles.Play();
        _smokeParticles.Play();
        _hasBeenFullyExtinguished = false;
    }
}