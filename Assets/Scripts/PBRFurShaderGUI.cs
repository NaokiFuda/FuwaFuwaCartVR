using UnityEngine;
using UnityEditor;

public class ImprovedPBRFurShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // 基本のプロパティ取得
        MaterialProperty colorProp = FindProperty("_Color", properties);
        MaterialProperty mainTexProp = FindProperty("_MainTex", properties);

        // PBRマップのプロパティ
        MaterialProperty metallicMapProp = FindProperty("_MetallicGlossMap", properties);
        MaterialProperty metallicIntensityProp = FindProperty("_MetallicIntensity", properties);
        MaterialProperty roughnessMapProp = FindProperty("_RoughnessMap", properties);
        MaterialProperty roughnessIntensityProp = FindProperty("_RoughnessIntensity", properties);
        MaterialProperty normalMapProp = FindProperty("_NormalMap", properties);
        MaterialProperty normalIntensityProp = FindProperty("_NormalIntensity", properties);
        MaterialProperty occlusionMapProp = FindProperty("_OcclusionMap", properties);
        MaterialProperty occlusionStrengthProp = FindProperty("_OcclusionStrength", properties);

        // ファーのプロパティ
        MaterialProperty furMapProp = FindProperty("_FurMap", properties);
        MaterialProperty furLengthMapProp = FindProperty("_FurLengthMap", properties);
        MaterialProperty furLengthProp = FindProperty("_FurLength", properties);
        MaterialProperty furDensityProp = FindProperty("_FurDensity", properties);
        MaterialProperty furThinnessProp = FindProperty("_FurThinness", properties);
        MaterialProperty furShadingProp = FindProperty("_FurShading", properties);

        // 方向プロパティ
        MaterialProperty furDirectionMapProp = FindProperty("_FurDirectionMap", properties);
        MaterialProperty furDirectionIntensityProp = FindProperty("_FurDirectionIntensity", properties);
        MaterialProperty furGravityProp = FindProperty("_FurGravity", properties);

        // 詳細設定プロパティ
        MaterialProperty furLayersProp = FindProperty("_FurLayers", properties);
        MaterialProperty furAlphaClipProp = FindProperty("_FurAlphaClip", properties);
        MaterialProperty edgeFadeProp = FindProperty("_EdgeFade", properties);

        // ベースのマテリアルプロパティ
        GUILayout.Label("Base Material Properties", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Albedo (RGB)"), mainTexProp, colorProp);
        EditorGUILayout.Space();

        // PBRプロパティ
        GUILayout.Label("PBR Properties", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Metallic Map"), metallicMapProp);
        materialEditor.RangeProperty(metallicIntensityProp, "Metallic Intensity");

        materialEditor.TexturePropertySingleLine(new GUIContent("Roughness Map"), roughnessMapProp);
        materialEditor.RangeProperty(roughnessIntensityProp, "Roughness Intensity");

        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), normalMapProp);
        materialEditor.RangeProperty(normalIntensityProp, "Normal Intensity");

        materialEditor.TexturePropertySingleLine(new GUIContent("Occlusion Map"), occlusionMapProp);
        materialEditor.RangeProperty(occlusionStrengthProp, "Occlusion Strength");
        EditorGUILayout.Space();

        // ファープロパティ
        GUILayout.Label("Fur Properties", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Fur Pattern"), furMapProp);
        materialEditor.TexturePropertySingleLine(new GUIContent("Fur Length Map"), furLengthMapProp);
        materialEditor.RangeProperty(furLengthProp, "Fur Length");
        materialEditor.RangeProperty(furDensityProp, "Fur Density");
        materialEditor.RangeProperty(furThinnessProp, "Fur Thinness");
        materialEditor.RangeProperty(furShadingProp, "Fur Shading");
        EditorGUILayout.Space();

        // 方向設定
        GUILayout.Label("Fur Direction", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Direction Map (RG)"), furDirectionMapProp);
        materialEditor.RangeProperty(furDirectionIntensityProp, "Direction Intensity");
        materialEditor.VectorProperty(furGravityProp, "Fur Gravity");
        EditorGUILayout.Space();

        // 詳細設定
        GUILayout.Label("Advanced Settings", EditorStyles.boldLabel);
        materialEditor.RangeProperty(furLayersProp, "Fur Layers");
        materialEditor.RangeProperty(furAlphaClipProp, "Fur Alpha Clip");
        materialEditor.RangeProperty(edgeFadeProp, "Edge Fade");

        // テクスチャスケール/オフセット
        EditorGUILayout.Space();
        materialEditor.TextureScaleOffsetProperty(mainTexProp);
        materialEditor.TextureScaleOffsetProperty(furMapProp);

        // レンダリング設定
        EditorGUILayout.Space();
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();

        // ファーレンダリングボタン
        EditorGUILayout.Space();
        GUILayout.Label("Fur Shell Generation", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Fur Shell"))
        {
            GenerateFurShell(materialEditor);
        }

        if (GUILayout.Button("Clear Fur Shell"))
        {
            ClearFurShell(materialEditor);
        }
    }

    private void GenerateFurShell(MaterialEditor materialEditor)
    {
        // 選択されたマテリアルを取得
        Material material = materialEditor.target as Material;
        if (material == null) return;

        // 選択されたオブジェクトを取得
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select an object with this material", "OK");
            return;
        }

        // レンダラーコンポーネントを確認
        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object has no renderer component", "OK");
            return;
        }

        // レイヤー数を取得
        int furLayers = Mathf.RoundToInt(material.GetFloat("_FurLayers"));

        // 既存のシェルを削除
        ClearFurShell(materialEditor);

        // シェルのコンテナを作成
        GameObject shellContainer = new GameObject("FurShell_Container");
        shellContainer.transform.SetParent(selectedObject.transform);
        shellContainer.transform.localPosition = Vector3.zero;
        shellContainer.transform.localRotation = Quaternion.identity;
        shellContainer.transform.localScale = Vector3.one;

        // シェルの設定を適用するためのメッシュを取得
        Mesh originalMesh = null;

        // メッシュフィルターがある場合は取得
        MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            originalMesh = meshFilter.sharedMesh;
        }
        // スキンメッシュレンダラーがある場合は取得
        else
        {
            SkinnedMeshRenderer skinnedMesh = selectedObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMesh.sharedMesh != null)
            {
                originalMesh = skinnedMesh.sharedMesh;
            }
        }

        if (originalMesh == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find mesh on the selected object", "OK");
            GameObject.DestroyImmediate(shellContainer);
            return;
        }

        // シェルを生成
        for (int i = 0; i < furLayers; i++)
        {
            GameObject shellObj = new GameObject("FurShell_" + i);
            shellObj.transform.SetParent(shellContainer.transform);
            shellObj.transform.localPosition = Vector3.zero;
            shellObj.transform.localRotation = Quaternion.identity;
            shellObj.transform.localScale = Vector3.one;

            // メッシュフィルターとレンダラーを追加
            MeshFilter shellMeshFilter = shellObj.AddComponent<MeshFilter>();
            shellMeshFilter.sharedMesh = originalMesh;

            MeshRenderer shellRenderer = shellObj.AddComponent<MeshRenderer>();

            // マテリアルの複製
            Material shellMaterial = new Material(material);

            // シェルレイヤーのインデックスを設定
            shellMaterial.SetFloat("_LayerIndex", i);

            // マテリアルを適用
            shellRenderer.sharedMaterial = shellMaterial;
        }

        EditorUtility.DisplayDialog("Fur Shell Generated", "Successfully created " + furLayers + " fur shell layers.", "OK");
    }

    private void ClearFurShell(MaterialEditor materialEditor)
    {
        // 選択されたオブジェクトを取得
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null) return;

        // 既存のファーシェルを検索
        Transform shellContainer = selectedObject.transform.Find("FurShell_Container");
        if (shellContainer != null)
        {
            GameObject.DestroyImmediate(shellContainer.gameObject);
        }
    }
}