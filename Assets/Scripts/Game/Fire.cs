using System.Collections.Generic;
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

    private ParticleSystem.EmissionModule _fireEmission;
    private ParticleSystem.EmissionModule _smokeEmission;
    private float _initialFireEmissionRate;
    private float _initialSmokeEmissionRate;
    private float _initialLightIntensity;
    private float _initialHealthPoints;
    private bool _isBeingExtinguished;

    private int _fireIndex = -1;
    private static int _nextFireIndex = 0;

    private bool _hasBeenFullyExtinguished = false;

    public bool IsExtinguished => _healthPoints <= 0;

    private void Awake()
    {
        _fireEmission = _fireParticles.emission;
        _smokeEmission = _smokeParticles.emission;
        _initialFireEmissionRate = _fireEmission.rateOverTime.constant;
        _initialSmokeEmissionRate = _smokeEmission.rateOverTime.constant;
        _initialLightIntensity = _fireLight.intensity;
        _initialHealthPoints = _healthPoints;

        _fireIndex = _nextFireIndex++;
    }

    public int GetFireIndex()
    {
        return _fireIndex;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetFireIndexCounter()
    {
        _nextFireIndex = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Extinguisher extinguisher))
        {
            float damage = 35f * Time.deltaTime;
            Extinguish(damage);
            _isBeingExtinguished = true;

            if (extinguisher.Recording != null)
            {
                extinguisher.Recording.RecordFireDamage(
                    GameManager.Instance.LevelTimer,
                    _fireIndex,
                    damage
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

    public void Extinguish(float damage)
    {
        _healthPoints -= damage;
        _healthPoints = Mathf.Max(0, _healthPoints);

        float healthPercentage = _healthPoints / _initialHealthPoints;

        var fireRate = _fireEmission.rateOverTime;
        fireRate.constant = _initialFireEmissionRate * healthPercentage;
        _fireEmission.rateOverTime = fireRate;

        var smokeRate = _smokeEmission.rateOverTime;
        smokeRate.constant = _initialSmokeEmissionRate * (1 - healthPercentage * 0.5f);
        _smokeEmission.rateOverTime = smokeRate;

        _fireLight.intensity = _initialLightIntensity * healthPercentage;

        if (_healthPoints <= 0 && !_hasBeenFullyExtinguished)
        {
            _hasBeenFullyExtinguished = true;
            OnFireFullyExtinguished();
        }
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

        var fireRate = _fireEmission.rateOverTime;
        fireRate.constant = _initialFireEmissionRate;
        _fireEmission.rateOverTime = fireRate;

        var smokeRate = _smokeEmission.rateOverTime;
        smokeRate.constant = _initialSmokeEmissionRate;
        _smokeEmission.rateOverTime = smokeRate;

        _fireLight.intensity = _initialLightIntensity;
        _fireLight.enabled = true;

        _fireParticles.Play();
        _smokeParticles.Play();

        _hasBeenFullyExtinguished = false;
    }
}