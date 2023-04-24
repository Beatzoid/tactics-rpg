using UnityEngine;

public class Tile : MonoBehaviour
{
    public const float STEP_HEIGHT = 0.25f;

    public Point pos;
    public int height;

    public Vector3 center
    {
        get
        {
            return new Vector3(pos.x, height * STEP_HEIGHT, pos.y);
        }
    }

    public void Grow()
    {
        height++;
        Match();
    }

    public void Shrink()
    {
        height--;
        Match();
    }

    public void Load(Point p, int h)
    {
        pos = p;
        height = h;
        Match();
    }

    public void Load(Vector3 v)
    {
        Load(new Point((int)v.x, (int)v.z), (int)v.y);
    }

    private void Match()
    {
        transform.localPosition = new Vector3(pos.x, height * STEP_HEIGHT / 2f, pos.y);
        transform.localScale = new Vector3(1, height * STEP_HEIGHT, 1);
    }
}
