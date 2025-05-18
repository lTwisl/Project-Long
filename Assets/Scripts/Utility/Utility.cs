using UnityEngine;


public static class Utility
{
    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public static float MapRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax, bool clamp = false)
    {
        // Обработка случая, когда исходный диапазон нулевой
        if (oldMin == oldMax)
            return newMin;

        // Вычисление нового значения
        float newValue = ((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin) + newMin;

        // Ограничение результата, если включено
        if (clamp)
        {
            float minClamp = Mathf.Min(newMin, newMax);
            float maxClamp = Mathf.Max(newMin, newMax);

            if (newValue < minClamp)
                return minClamp;
            else if (newValue > maxClamp)
                return maxClamp;
        }

        return newValue;
    }

    public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
    }

    public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return vector - direction * Vector3.Dot(vector, direction);
    }

}

