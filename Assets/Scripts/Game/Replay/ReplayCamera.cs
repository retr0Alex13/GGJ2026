using UnityEngine;

public class ReplayCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float heightOffset = 2f;
    [SerializeField] private float distanceFromCharacters = 8f;

    [Header("Camera Modes")]
    [SerializeField] private CameraMode currentMode = CameraMode.Overview;
    [SerializeField] private float modeChangeInterval = 10f;


    private Transform engineerTransform;
    private Transform firefighterTransform;
    private float modeTimer;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public enum CameraMode
    {
        Overview,           // Shows both characters
        FollowEngineer,    // Follows engineer
        FollowFirefighter, // Follows firefighter
        Cinematic          // Orbits around action
    }

    public void Initialize(Transform engineer, Transform firefighter)
    {
        engineerTransform = engineer;
        firefighterTransform = firefighter;
        modeTimer = modeChangeInterval;
    }

    private void Update()
    {
        if (engineerTransform == null || firefighterTransform == null)
            return;

        // Auto-switch camera modes
        modeTimer -= Time.deltaTime;
        if (modeTimer <= 0)
        {
            CycleCameraMode();
            modeTimer = modeChangeInterval;
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        switch (currentMode)
        {
            case CameraMode.Overview:
                UpdateOverviewMode();
                break;
            case CameraMode.FollowEngineer:
                UpdateFollowMode(engineerTransform);
                break;
            case CameraMode.FollowFirefighter:
                UpdateFollowMode(firefighterTransform);
                break;
            case CameraMode.Cinematic:
                UpdateCinematicMode();
                break;
        }

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateOverviewMode()
    {
        // Position camera to see both characters
        Vector3 midpoint = (engineerTransform.position + firefighterTransform.position) / 2f;
        float distance = Vector3.Distance(engineerTransform.position, firefighterTransform.position);

        // Adjust distance based on how far apart characters are
        float adjustedDistance = Mathf.Max(distanceFromCharacters, distance * 0.8f);

        targetPosition = midpoint + new Vector3(0, heightOffset, -adjustedDistance);
        targetRotation = Quaternion.LookRotation(midpoint - targetPosition);
    }

    private void UpdateFollowMode(Transform target)
    {
        // Follow specific character from behind and above
        Vector3 offset = -target.forward * distanceFromCharacters + Vector3.up * heightOffset;
        targetPosition = target.position + offset;
        targetRotation = Quaternion.LookRotation(target.position - targetPosition);
    }

    private void UpdateCinematicMode()
    {
        // Orbit around the center point
        Vector3 midpoint = (engineerTransform.position + firefighterTransform.position) / 2f;
        float angle = Time.time * 0.2f; // Slow orbit

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distanceFromCharacters,
            heightOffset,
            Mathf.Sin(angle) * distanceFromCharacters
        );

        targetPosition = midpoint + offset;
        targetRotation = Quaternion.LookRotation(midpoint - targetPosition);
    }

    private void CycleCameraMode()
    {
        currentMode = (CameraMode)(((int)currentMode + 1) % System.Enum.GetValues(typeof(CameraMode)).Length);
        Debug.Log($"Camera mode changed to: {currentMode}");
    }

    public void SetCameraMode(CameraMode mode)
    {
        currentMode = mode;
        modeTimer = modeChangeInterval;
    }
}