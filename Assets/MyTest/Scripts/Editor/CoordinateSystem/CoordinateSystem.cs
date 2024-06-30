using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CoordinateSystem : EditorWindow
{
    private Vector3 origin = new Vector3(0, 0, 0);

    private bool displayUnitNumbers = true;

    private Mesh curveMesh;

    private static Material s_CurveMaterial;

    public static Material curveMaterial
    {
        get
        {
            if (!s_CurveMaterial)
            {
                Shader shader = (Shader)EditorGUIUtility.LoadRequired("Editors/AnimationWindow/Curve.shader");
                s_CurveMaterial = new Material(shader);
                s_CurveMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return s_CurveMaterial;
        }
    }

    [MenuItem("Tools/Coordinate System")]
    public static void ShowWindow()
    {
        GetWindow<CoordinateSystem>("Coordinate System");
    }

    private void OnGUI()
    {
        DrawAxis();
        DrawGrid();

        DrawCurve();
    }

    private void DrawAxis() 
    {
        Vector3 hStart = new Vector3(0, position.height / 2, 0);
        Vector3 hEnd = new Vector3(position.width, position.height / 2, 0);

        Color lastColor = Handles.color;
        Handles.color = CoordinateSettings.AxisColor;
        Handles.DrawLine(hStart, hEnd);

        Vector3 vStart = new Vector3(position.width / 2, 0, 0);
        Vector3 vEnd = new Vector3(position.width / 2, position.height, 0);
        Handles.DrawLine(vStart, vEnd);
        Handles.color = lastColor;

        origin = new Vector3(position.width / 2, position.height / 2, 0);
    }

    private void DrawGrid()
    {
        Color lastColor = Handles.color;

        Handles.color = CoordinateSettings.GridColor;
        for (float x = origin.x; x < position.width; x += CoordinateSettings.UnitLength)
        {
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, position.height, 0));
        }
        for (float x = origin.x; x > 0; x -= CoordinateSettings.UnitLength)
        {
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, position.height, 0));
        }
        for (float y = origin.y; y < position.height; y += CoordinateSettings.UnitLength)
        {
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(position.width, y, 0));
        }
        for (float y = origin.y; y > 0; y -= CoordinateSettings.UnitLength)
        {
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(position.width, y, 0));
        }
        Handles.color = lastColor;
    }

    private void DrawCurve()
    {
        if (curveMesh == null)
        {
            BuildCurveMesh();
        }

        curveMaterial.SetColor("_Color", Color.blue);
        curveMaterial.SetPass(0);
        Color lastColor = Handles.color;
        Handles.color = Color.blue;
        Graphics.DrawMeshNow(curveMesh, Matrix4x4.identity);
        Handles.color = lastColor;
    }

    private void BuildCurveMesh()
    {
        curveMesh = new Mesh()
        {
            name = "CurveMesh",
        };

        List<Vector3> vertices = new List<Vector3>();

        for (float x = 0; x < position.width; x += CoordinateSettings.UnitLength)
        {
            for (int i = 0; i < CoordinateSettings.UnitLength; i++)
            {
                float curveX = (x + i) / CoordinateSettings.UnitLength;
                float curveY = Mathf.Sin(curveX);

                vertices.Add(new Vector3(x + i, origin.y + curveY * CoordinateSettings.UnitLength, 0));
            }
        }

        List<int> indices = new List<int>();
        int index = 0;
        while (index < vertices.Count - 1)
        {
            indices.Add(index);
            indices.Add(index + 1);
            index++;
        }

        curveMesh.SetVertices(vertices);
        curveMesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
    }
}
