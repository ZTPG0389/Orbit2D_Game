using UnityEngine;

public static class ScreenBounds
{
    public static float Width  => Camera.main.orthographicSize * Camera.main.aspect;
    public static float Height => Camera.main.orthographicSize;

    public static Vector3 RandomTargetPosition(float minDistFromCenter, float margin = 0.8f)
    {
        float   maxX     = Width  * margin;
        float   maxY     = Height * margin;
        Vector3 pos      = Vector3.zero;
        int     attempts = 0;

        do
        {
            float x = Random.Range(-maxX, maxX);
            float y = Random.Range(-maxY, maxY);
            pos = new Vector3(x, y, 0f);
            attempts++;
        }
        while (pos.magnitude < minDistFromCenter && attempts < 100);

        return pos;
    }
}
