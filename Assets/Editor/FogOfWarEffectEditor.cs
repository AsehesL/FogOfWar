using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogOfWarEffect))]
public class FogOfWarEffectEditor : Editor
{
    private FogOfWarEffect m_Target;

    void OnEnable()
    {
        m_Target = (FogOfWarEffect) target;
    }

    public override bool HasPreviewGUI()
    {
        if (m_Target == null)
            return false;
        return m_Target.fowMaskTexture != null;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        if (GUILayout.Button("预生成地图数据"))
        {
            ASL.FogOfWar.FOWPregenerationMapData dt = m_Target.GetComponent<ASL.FogOfWar.FOWPregenerationMapData>();
            if (!dt)
                dt = m_Target.gameObject.AddComponent<ASL.FogOfWar.FOWPregenerationMapData>();
            if (dt)
            {
                dt.width = m_Target.texWidth;
                dt.height = m_Target.texHeight;
                float deltax = m_Target.xSize / m_Target.texWidth;
                float deltaz = m_Target.zSize / m_Target.texHeight;
                Vector3 beginpos = m_Target.centerPosition - new Vector3(m_Target.xSize * 0.5f, 0, m_Target.zSize * 0.5f);
                dt.GenerateMapData(beginpos.x, beginpos.y, deltax, deltaz, m_Target.heightRange);
            }
        }
    }

    public override void DrawPreview(Rect previewArea)
    {
        base.DrawPreview(previewArea);
        if (m_Target != null && m_Target.fowMaskTexture)
            GUI.DrawTexture(previewArea, m_Target.fowMaskTexture);
    }
}
