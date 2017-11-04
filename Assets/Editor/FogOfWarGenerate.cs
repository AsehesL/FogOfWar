using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FogOfWarGenerate : EditorWindow
{

    private Texture2D m_Tex;

    private int m_Width;
    private int m_Height;

    private float m_Size = 1.5f;

    [MenuItem("Test/Init")]
    static void Init()
    {
        FogOfWarGenerate g = FogOfWarGenerate.GetWindow<FogOfWarGenerate>();
    }

    void OnGUI()
    {
        m_Tex = EditorGUILayout.ObjectField("Texture", m_Tex, typeof (Texture2D), false) as Texture2D;

        if (GUILayout.Button("Generate"))
        {
            if (m_Tex)
                CreateMapMesh(m_Tex);
        }
    }

    private void CreateMapMesh(Texture2D map)
    {

        Mesh mesh = new Mesh();

        List<Vector3> vertexes = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indexes = new List<int>();

        int index = 0;
        m_Width = map.width;
        m_Height = map.height;

        int[,] array = new int[m_Width, m_Height];

        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                Color col = map.GetPixel(i, j);
                if (col.r > 0.5f)
                {
                    index = CreateTopMesh(i, j, vertexes, uvs, indexes, index, m_Size);
                    index = CreateLeftMesh(i, j, map, vertexes, uvs, indexes, index);
                    index = CreateRightMesh(i, j, map, vertexes, uvs, indexes, index);
                    index = CreateBackMesh(i, j, map, vertexes, uvs, indexes, index);
                    index = CreateForwardMesh(i, j, map, vertexes, uvs, indexes, index);
                    array[i, j] = 1;
                }
                else
                {
                    index = CreateBottomMesh(i, j, vertexes, uvs, indexes, index);
                }
            }
        }
        mesh.SetVertices(vertexes);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(indexes.ToArray(), 0);
        mesh.RecalculateNormals();

        AssetDatabase.CreateAsset(mesh, "Assets/t.asset");
    }

    private int CreateTopMesh(int x, int y, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index, float height = 0)
    {
        if (x < 0 || x >= m_Width)
            return index;
        if (y < 0 || y >= m_Height)
            return index;
        vertexes.Add(new Vector3(x * m_Size, height, y * m_Size));
        vertexes.Add(new Vector3(x * m_Size, height, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, height, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, height, y * m_Size));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(0.5f, 0.5f));
        uvs.Add(new Vector2(0.5f, 0));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }

    private int CreateBottomMesh(int x, int y, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index)
    {
        if (x < 0 || x >= m_Width)
            return index;
        if (y < 0 || y >= m_Height)
            return index;
        vertexes.Add(new Vector3(x * m_Size, 0, y * m_Size));
        vertexes.Add(new Vector3(x * m_Size, 0, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, 0, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, 0, y * m_Size));
        uvs.Add(new Vector2(0.5f, 0));
        uvs.Add(new Vector2(0.5f, 0.5f));
        uvs.Add(new Vector2(1, 0.5f));
        uvs.Add(new Vector2(1, 0));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }

    private int CreateLeftMesh(int x, int y, Texture2D map, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index)
    {
        bool create = false;
        if (x <= 0)
            create = true;
        else
        {
            Color col = map.GetPixel(x - 1, y);
            if (col.r < 0.5f)
                create = true;
        }
        if (!create)
            return index;

        vertexes.Add(new Vector3(x * m_Size, 0, (y + 1) * m_Size));
        vertexes.Add(new Vector3(x * m_Size, m_Size, (y + 1) * m_Size));
        vertexes.Add(new Vector3(x * m_Size, m_Size, y * m_Size));
        vertexes.Add(new Vector3(x * m_Size, 0, y * m_Size));
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(0.5f, 0.5f));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }

    private int CreateRightMesh(int x, int y, Texture2D map, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index)
    {
        bool create = false;
        if (x >= map.width - 1)
            create = true;
        else
        {
            Color col = map.GetPixel(x + 1, y);
            if (col.r < 0.5f)
                create = true;
        }
        if (!create)
            return index;

        vertexes.Add(new Vector3((x + 1) * m_Size, 0, y * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, m_Size, y * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, m_Size, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, 0, (y + 1) * m_Size));
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(0.5f, 0.5f));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }

    private int CreateBackMesh(int x, int y, Texture2D map, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index)
    {
        bool create = false;
        if (y <= 0)
            create = true;
        else
        {
            Color col = map.GetPixel(x, y - 1);
            if (col.r < 0.5f)
                create = true;
        }
        if (!create)
            return index;

        vertexes.Add(new Vector3(x * m_Size, 0, y * m_Size));
        vertexes.Add(new Vector3(x * m_Size, m_Size, y * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, m_Size, y * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, 0, y * m_Size));
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(0.5f, 0.5f));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }

    private int CreateForwardMesh(int x, int y, Texture2D map, List<Vector3> vertexes, List<Vector2> uvs, List<int> indexes, int index)
    {
        bool create = false;
        if (y >= map.height - 1)
            create = true;
        else
        {
            Color col = map.GetPixel(x, y + 1);
            if (col.r < 0.5f)
                create = true;
        }
        if (!create)
            return index;

        vertexes.Add(new Vector3((x + 1) * m_Size, 0, (y + 1) * m_Size));
        vertexes.Add(new Vector3((x + 1) * m_Size, m_Size, (y + 1) * m_Size));
        vertexes.Add(new Vector3(x * m_Size, m_Size, (y + 1) * m_Size));
        vertexes.Add(new Vector3(x * m_Size, 0, (y + 1) * m_Size));
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(0.5f, 1));
        uvs.Add(new Vector2(0.5f, 0.5f));
        indexes.Add(index);
        indexes.Add(index + 1);
        indexes.Add(index + 2);
        indexes.Add(index);
        indexes.Add(index + 2);
        indexes.Add(index + 3);
        return index + 4;
    }
}
