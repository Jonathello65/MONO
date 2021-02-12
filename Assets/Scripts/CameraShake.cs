using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{	
	public float shakeDuration = 0f;
	
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;
	
	Vector3 originalPos;

	void Update()
	{
        // Randomly shakes camera while duration is above 0, based on shake amount. Shake amount decreases over time
		if (shakeDuration > 0)
		{
            originalPos = transform.localPosition;
			transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
			
			shakeDuration -= Time.deltaTime;
            shakeAmount -= Time.deltaTime * decreaseFactor;
		}
		else
		{
			shakeDuration = 0f;
		}
	}

    public void ShakeCamera(float duration, float amplitude)
    {
        // Shakes camera based on duration and amplitude given
        SetShakeAmount(amplitude);
        SetDuration(duration);
    }

    void SetDuration(float duration)
    {
        shakeDuration = duration;
    }

    void SetShakeAmount(float amplitude)
    {
        shakeAmount = amplitude;
    }
}
