using UnityEngine;
using UnityEditor;

namespace Renderer
{
    public class ShaderEditor : ShaderGUI
    {
        enum SmoothnessSource
        {
            Uniform, Albedo, Metallic
        }

        Material target;
        MaterialEditor editor;
        MaterialProperty[] properties;

        static GUIContent staticLabel = new GUIContent();

        static GUIContent MakeLabel(string text, string tooltip = null)
        {
            staticLabel.text = text;
            staticLabel.tooltip = tooltip;
            return staticLabel;
        }

        static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
        {
            staticLabel.text = property.displayName;
            staticLabel.tooltip = tooltip;
            return staticLabel;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            this.target = materialEditor.target as Material;
            this.editor = materialEditor;
            this.properties = properties;

            DoMain();
            DoSecondary();
        }

        void DoSecondary () {
            GUILayout.Label("Secondary Maps", EditorStyles.boldLabel);

            MaterialProperty detailTex = FindProperty("_DetailTex");
            editor.TexturePropertySingleLine(
                MakeLabel(detailTex, "Albedo (RGB) multiplied by 2"), detailTex
            );

            DoSecondaryNormals();
            editor.TextureScaleOffsetProperty(detailTex);
	    }

        void DoSecondaryNormals () {
            MaterialProperty map = FindProperty("_DetailNormalMap");
            editor.TexturePropertySingleLine(
                MakeLabel(map), map, map.textureValue ? FindProperty("_DetailBumpScale") : null
            );
        }

        private void DoMain()
        {
            GUILayout.Label("Main Maps", EditorStyles.boldLabel);

            MaterialProperty mainTex = FindProperty("_MainTex");
            MaterialProperty tint = FindProperty("_Tint");
            editor.TexturePropertySingleLine(MakeLabel(mainTex, "Albedo (RGB)"), mainTex, tint);

            DoMetallic();
            DoSmoothness();
            DoNormals();
            DoOcclusion();
            DoEmission();
            DoDetailMask();
            editor.TextureScaleOffsetProperty(mainTex);
        }

        void DoOcclusion () {
            MaterialProperty map = FindProperty("_OcclusionMap");
            Texture tex = map.textureValue;
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                MakeLabel(map, "Occlusion (G)"), map,
                tex ? FindProperty("_OcclusionStrength") : null
            );
            if (EditorGUI.EndChangeCheck() && tex != map.textureValue) {
                SetKeyword("_OCCLUSION_MAP", map.textureValue);
            }
        }

        void DoDetailMask () {
            MaterialProperty mask = FindProperty("_DetailMask");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                MakeLabel(mask, "Detail Mask (A)"), mask
            );
            if (EditorGUI.EndChangeCheck()) {
                SetKeyword("_DETAIL_MASK", mask.textureValue);
            }
        }

        private void DoNormals()
        {
            MaterialProperty map = FindProperty("_NormalMap");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(MakeLabel(map, "Normal Map"), map, map.textureValue ? FindProperty("_BumpScale") : null);

            if (EditorGUI.EndChangeCheck()) {
			    SetKeyword("_NORMAL_MAP", map.textureValue);
            }
        }


        void DoEmission () {
            MaterialProperty map = FindProperty("_EmissionMap");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertyWithHDRColor(
                MakeLabel(map, "Emission (RGB)"), map, FindProperty("_Emission"), false
            );
            if (EditorGUI.EndChangeCheck()) {
                SetKeyword("_EMISSION_MAP", map.textureValue);
            }
        }

        void DoMetallic()
        {
            MaterialProperty map = FindProperty("_MetallicMap");
            EditorGUI.BeginChangeCheck();
            editor.TexturePropertySingleLine(
                MakeLabel(map, "Metallic (R)"), map,
                map.textureValue ? null : FindProperty("_Metallic")
            );

            if (EditorGUI.EndChangeCheck())
            {
                SetKeyword("_METALLIC_MAP", map.textureValue);
            }
        }

        void SetKeyword(string keyword, bool state)
        {
            if (state)
            {
                target.EnableKeyword(keyword);
            }
            else
            {
                target.DisableKeyword(keyword);
            }
        }

        void DoSmoothness()
        {
            SmoothnessSource source = SmoothnessSource.Uniform;
            if (IsKeywordEnabled("_SMOOTHNESS_ALBEDO"))
            {
                source = SmoothnessSource.Albedo;
            }
            else if (IsKeywordEnabled("_SMOOTHNESS_METALLIC"))
            {
                source = SmoothnessSource.Metallic;
            }

            MaterialProperty slider = FindProperty("_Smoothness");
            EditorGUI.indentLevel += 2;
            editor.ShaderProperty(slider, MakeLabel(slider));
            EditorGUI.indentLevel += 1;

            EditorGUI.BeginChangeCheck();
            source = (SmoothnessSource)EditorGUILayout.EnumPopup(MakeLabel("Source"), source);
            if (EditorGUI.EndChangeCheck())
            {
                RecordAction("Smoothness Source");
                SetKeyword("_SMOOTHNESS_ALBEDO", source == SmoothnessSource.Albedo);
                SetKeyword("_SMOOTHNESS_METALLIC", source == SmoothnessSource.Metallic);
            }
            EditorGUI.indentLevel -= 3;
        }

        void RecordAction(string label)
        {
            editor.RegisterPropertyChangeUndo(label);
        }

        bool IsKeywordEnabled(string keyword)
        {
            return target.IsKeywordEnabled(keyword);
        }

        private MaterialProperty FindProperty(string name)
        {
            return FindProperty(name, properties);
        }
    }
}