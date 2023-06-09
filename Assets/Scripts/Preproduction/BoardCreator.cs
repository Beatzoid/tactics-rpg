using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoardCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject tileViewPrefab;
    [SerializeField]
    private GameObject tileSelectionIndicatorPrefab;

    [SerializeField]
    private int width = 10;
    [SerializeField]
    private int depth = 10;
    [SerializeField]
    private float height = 8;

    [SerializeField]
    private Point pos;

    [SerializeField]
    private LevelData levelData;

    private Transform marker
    {
        get
        {
            if (_marker == null)
            {
                GameObject instance = Instantiate(tileSelectionIndicatorPrefab);
                _marker = instance.transform;
            }

            return _marker;
        }
    }

    private Transform _marker;

    private Dictionary<Point, Tile> tiles = new();

    public void GrowArea()
    {
        Rect r = RandomRect();
        GrowRect(r);
    }

    public void ShrinkArea()
    {
        Rect r = RandomRect();
        ShrinkRect(r);
    }

    public void Grow()
    {
        GrowSingle(pos);
    }
    public void Shrink()
    {
        ShrinkSingle(pos);
    }

    public void UpdateMarker()
    {
        Tile t = tiles.ContainsKey(pos) ? tiles[pos] : null;
        marker.localPosition = t != null ? t.center : new Vector3(pos.x, 0, pos.y);
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        tiles.Clear();
    }

    public void Save()
    {
        string filePath = Application.dataPath + "/Resources/Levels";
        if (!Directory.Exists(filePath))
            CreateSaveDirectory();

        LevelData board = ScriptableObject.CreateInstance<LevelData>();
        board.tiles = new List<Vector3>(tiles.Count);

        foreach (Tile t in tiles.Values)
            board.tiles.Add(new Vector3(t.pos.x, t.height, t.pos.y));

        string filename = string.Format("Assets/Resources/Levels/{1}.asset", filePath, name);

#if UNITY_EDITOR
        AssetDatabase.CreateAsset(board, filename);
#endif
    }

    public void Load()
    {
        Clear();

        if (levelData == null) return;

        foreach (Vector3 v in levelData.tiles)
        {
            Tile t = Create();
            t.Load(v);
            tiles.Add(t.pos, t);
        }
    }

    private void CreateSaveDirectory()
    {
#if UNITY_EDITOR
        string filePath = Application.dataPath + "/Resources";
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets", "Resources");

        filePath += "/Levels";
        if (!Directory.Exists(filePath))
            AssetDatabase.CreateFolder("Assets/Resources", "Levels");

        AssetDatabase.Refresh();
#endif
    }

    private Rect RandomRect()
    {
        int x = Random.Range(0, width);
        int y = Random.Range(0, depth);
        int w = Random.Range(1, width - x + 1);
        int h = Random.Range(1, depth - y + 1);

        return new Rect(x, y, w, h);
    }

    private void GrowRect(Rect rect)
    {
        for (int y = (int)rect.yMin; y < (int)rect.yMax; y++)
        {
            for (int x = (int)rect.xMin; x < (int)rect.xMax; x++)
            {
                Point p = new Point(x, y);
                GrowSingle(p);
            }
        }
    }

    private void ShrinkRect(Rect rect)
    {
        for (int y = (int)rect.yMin; y < (int)rect.yMax; y++)
        {
            for (int x = (int)rect.xMin; x < (int)rect.xMax; x++)
            {
                Point p = new Point(x, y);
                ShrinkSingle(p);
            }
        }
    }

    private Tile Create()
    {
        GameObject instance = Instantiate(tileViewPrefab);
        instance.transform.parent = transform;
        return instance.GetComponent<Tile>();
    }

    private Tile GetOrCreate(Point p)
    {
        if (tiles.ContainsKey(p))
            return tiles[p];

        Tile t = Create();
        t.Load(p, 0);
        tiles.Add(p, t);

        return t;
    }

    private void GrowSingle(Point p)
    {
        Tile t = GetOrCreate(p);
        if (t.height < height) t.Grow();
    }

    private void ShrinkSingle(Point p)
    {
        if (!tiles.ContainsKey(p)) return;

        Tile t = tiles[p];
        t.Shrink();

        if (t.height <= 0)
        {
            tiles.Remove(p);
            DestroyImmediate(t.gameObject);
        }
    }
}
