using UnityEngine;
using UnityEditor;

public class ImprovedPBRFurShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // ��{�̃v���p�e�B�擾
        MaterialProperty colorProp = FindProperty("_Color", properties);
        MaterialProperty mainTexProp = FindProperty("_MainTex", properties);

        // PBR�}�b�v�̃v���p�e�B
        MaterialProperty metallicMapProp = FindProperty("_MetallicGlossMap", properties);
        MaterialProperty metallicIntensityProp = FindProperty("_MetallicIntensity", properties);
        MaterialProperty roughnessMapProp = FindProperty("_RoughnessMap", properties);
        MaterialProperty roughnessIntensityProp = FindProperty("_RoughnessIntensity", properties);
        MaterialProperty normalMapProp = FindProperty("_NormalMap", properties);
        MaterialProperty normalIntensityProp = FindProperty("_NormalIntensity", properties);
        MaterialProperty occlusionMapProp = FindProperty("_OcclusionMap", properties);
        MaterialProperty occlusionStrengthProp = FindProperty("_OcclusionStrength", properties);

        // �t�@�[�̃v���p�e�B
        MaterialProperty furMapProp = FindProperty("_FurMap", properties);
        MaterialProperty furLengthMapProp = FindProperty("_FurLengthMap", properties);
        MaterialProperty furLengthProp = FindProperty("_FurLength", properties);
        MaterialProperty furDensityProp = FindProperty("_FurDensity", properties);
        MaterialProperty furThinnessProp = FindProperty("_FurThinness", properties);
        MaterialProperty furShadingProp = FindProperty("_FurShading", properties);

        // �����v���p�e�B
        MaterialProperty furDirectionMapProp = FindProperty("_FurDirectionMap", properties);
        MaterialProperty furDirectionIntensityProp = FindProperty("_FurDirectionIntensity", properties);
        MaterialProperty furGravityProp = FindProperty("_FurGravity", properties);

        // �ڍאݒ�v���p�e�B
        MaterialProperty furLayersProp = FindProperty("_FurLayers", properties);
        MaterialProperty furAlphaClipProp = FindProperty("_FurAlphaClip", properties);
        MaterialProperty edgeFadeProp = FindProperty("_EdgeFade", properties);

        // �x�[�X�̃}�e���A���v���p�e�B
        GUILayout.Label("Base Material Properties", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Albedo (RGB)"), mainTexProp, colorProp);
        EditorGUILayout.Space();

        // PBR�v���p�e�B
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

        // �t�@�[�v���p�e�B
        GUILayout.Label("Fur Properties", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Fur Pattern"), furMapProp);
        materialEditor.TexturePropertySingleLine(new GUIContent("Fur Length Map"), furLengthMapProp);
        materialEditor.RangeProperty(furLengthProp, "Fur Length");
        materialEditor.RangeProperty(furDensityProp, "Fur Density");
        materialEditor.RangeProperty(furThinnessProp, "Fur Thinness");
        materialEditor.RangeProperty(furShadingProp, "Fur Shading");
        EditorGUILayout.Space();

        // �����ݒ�
        GUILayout.Label("Fur Direction", EditorStyles.boldLabel);
        materialEditor.TexturePropertySingleLine(new GUIContent("Direction Map (RG)"), furDirectionMapProp);
        materialEditor.RangeProperty(furDirectionIntensityProp, "Direction Intensity");
        materialEditor.VectorProperty(furGravityProp, "Fur Gravity");
        EditorGUILayout.Space();

        // �ڍאݒ�
        GUILayout.Label("Advanced Settings", EditorStyles.boldLabel);
        materialEditor.RangeProperty(furLayersProp, "Fur Layers");
        materialEditor.RangeProperty(furAlphaClipProp, "Fur Alpha Clip");
        materialEditor.RangeProperty(edgeFadeProp, "Edge Fade");

        // �e�N�X�`���X�P�[��/�I�t�Z�b�g
        EditorGUILayout.Space();
        materialEditor.TextureScaleOffsetProperty(mainTexProp);
        materialEditor.TextureScaleOffsetProperty(furMapProp);

        // �����_�����O�ݒ�
        EditorGUILayout.Space();
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();

        // �t�@�[�����_�����O�{�^��
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
        // �I�����ꂽ�}�e���A�����擾
        Material material = materialEditor.target as Material;
        if (material == null) return;

        // �I�����ꂽ�I�u�W�F�N�g���擾
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select an object with this material", "OK");
            return;
        }

        // �����_���[�R���|�[�l���g���m�F
        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object has no renderer component", "OK");
            return;
        }

        // ���C���[�����擾
        int furLayers = Mathf.RoundToInt(material.GetFloat("_FurLayers"));

        // �����̃V�F�����폜
        ClearFurShell(materialEditor);

        // �V�F���̃R���e�i���쐬
        GameObject shellContainer = new GameObject("FurShell_Container");
        shellContainer.transform.SetParent(selectedObject.transform);
        shellContainer.transform.localPosition = Vector3.zero;
        shellContainer.transform.localRotation = Quaternion.identity;
        shellContainer.transform.localScale = Vector3.one;

        // �V�F���̐ݒ��K�p���邽�߂̃��b�V�����擾
        Mesh originalMesh = null;

        // ���b�V���t�B���^�[������ꍇ�͎擾
        MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            originalMesh = meshFilter.sharedMesh;
        }
        // �X�L�����b�V�������_���[������ꍇ�͎擾
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

        // �V�F���𐶐�
        for (int i = 0; i < furLayers; i++)
        {
            GameObject shellObj = new GameObject("FurShell_" + i);
            shellObj.transform.SetParent(shellContainer.transform);
            shellObj.transform.localPosition = Vector3.zero;
            shellObj.transform.localRotation = Quaternion.identity;
            shellObj.transform.localScale = Vector3.one;

            // ���b�V���t�B���^�[�ƃ����_���[��ǉ�
            MeshFilter shellMeshFilter = shellObj.AddComponent<MeshFilter>();
            shellMeshFilter.sharedMesh = originalMesh;

            MeshRenderer shellRenderer = shellObj.AddComponent<MeshRenderer>();

            // �}�e���A���̕���
            Material shellMaterial = new Material(material);

            // �V�F�����C���[�̃C���f�b�N�X��ݒ�
            shellMaterial.SetFloat("_LayerIndex", i);

            // �}�e���A����K�p
            shellRenderer.sharedMaterial = shellMaterial;
        }

        EditorUtility.DisplayDialog("Fur Shell Generated", "Successfully created " + furLayers + " fur shell layers.", "OK");
    }

    private void ClearFurShell(MaterialEditor materialEditor)
    {
        // �I�����ꂽ�I�u�W�F�N�g���擾
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null) return;

        // �����̃t�@�[�V�F��������
        Transform shellContainer = selectedObject.transform.Find("FurShell_Container");
        if (shellContainer != null)
        {
            GameObject.DestroyImmediate(shellContainer.gameObject);
        }
    }
}