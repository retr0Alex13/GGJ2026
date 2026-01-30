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

    private void Awake()
    {
        _fireEmission = _fireParticles.emission;
        _smokeEmission = _smokeParticles.emission;

        _initialFireEmissionRate = _fireEmission.rateOverTime.constant;
        _initialSmokeEmissionRate = _smokeEmission.rateOverTime.constant;
        _initialLightIntensity = _fireLight.intensity;
        _initialHealthPoints = _healthPoints;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Extinguisher extinguisher))
        {
            Extinguish(35f * Time.deltaTime);
            _isBeingExtinguished = true;
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

        if (_healthPoints <= 0)
        {
            _fireParticles.Stop();
            _fireLight.enabled = false;
            if (!_fireExtinguished.isPlaying)
            {
                _fireExtinguished.Play();
            }
            Invoke(nameof(StopSmoke), _fireExtinguished.clip.length);
        }
    }

    private void StopSmoke()
    {
        _smokeParticles.Stop();
    }
}