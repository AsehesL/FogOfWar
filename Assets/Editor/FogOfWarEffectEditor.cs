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
    }

    public override void DrawPreview(Rect previewArea)
    {
        base.DrawPreview(previewArea);
        if (m_Target != null && m_Target.fowMaskTexture)
            GUI.DrawTexture(previewArea, m_Target.fowMaskTexture);
    }
}
