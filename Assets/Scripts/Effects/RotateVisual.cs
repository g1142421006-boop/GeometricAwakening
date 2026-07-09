using UnityEngine;

public class RotateVisual : MonoBehaviour
{
    public enum RotateMode
    {
        DegreesPerSecond,
        SecondsPerRevolution
    }

    [Header("Rotation")]
    [SerializeField] private bool rotate = true;

    [SerializeField]
    private RotateMode rotateMode =
        RotateMode.SecondsPerRevolution;

    [Header("Degrees Per Second")]
    [SerializeField] private float degreesPerSecond = 90f;

    [Header("Seconds Per Revolution")]
    [SerializeField] private float secondsPerRevolution = 4f;

    [SerializeField] private bool clockwise = true;

    private void Update()
    {
        if (!rotate)
            return;

        float rotateAmount;

        if (rotateMode == RotateMode.DegreesPerSecond)
        {
            rotateAmount = degreesPerSecond;
        }
        else
        {
            if (secondsPerRevolution <= 0.01f)
                return;

            rotateAmount = 360f / secondsPerRevolution;
        }

        if (!clockwise)
            rotateAmount = -rotateAmount;

        transform.Rotate(
            0f,
            0f,
            -rotateAmount * Time.deltaTime
        );
    }
}
