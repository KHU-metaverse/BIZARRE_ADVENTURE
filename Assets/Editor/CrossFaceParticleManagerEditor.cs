using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CrossFaceParticleManager))]
public class CrossFaceParticleManagerEditor : Editor
{
    private SerializedProperty projectionFacesProperty;
    private SerializedProperty crossFaceParticleSystemProperty;
    private SerializedProperty particleSpeedProperty;
    private SerializedProperty faceCrossingDistanceProperty;
    private SerializedProperty maxParticlesProperty;
    
    private bool showFacesSettings = true;
    private bool showParticleSettings = true;
    
    private void OnEnable()
    {
        projectionFacesProperty = serializedObject.FindProperty("projectionFaces");
        crossFaceParticleSystemProperty = serializedObject.FindProperty("crossFaceParticleSystem");
        particleSpeedProperty = serializedObject.FindProperty("particleSpeed");
        faceCrossingDistanceProperty = serializedObject.FindProperty("faceCrossingDistance");
        maxParticlesProperty = serializedObject.FindProperty("maxParticles");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        CrossFaceParticleManager manager = (CrossFaceParticleManager)target;
        
        EditorGUILayout.Space();
        
        // 빠른 설정 버튼들
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("모든 법선 자동 계산", GUILayout.Height(30)))
        {
            manager.AutoCalculateAllNormals();
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("모든 면 크기 자동 계산", GUILayout.Height(30)))
        {
            manager.AutoCalculateFaceSizes();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("면 설정 도우미 실행", GUILayout.Height(40)))
        {
            manager.SetupFacesWizard();
            EditorUtility.SetDirty(target);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // 투영 면 설정
        showFacesSettings = EditorGUILayout.Foldout(showFacesSettings, "투영 면 설정", true, EditorStyles.foldoutHeader);
        if (showFacesSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(projectionFacesProperty, true);
            
            // 각 면에 대한 빠른 설정 버튼
            if (manager.projectionFaces != null && manager.projectionFaces.Length > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("개별 면 설정", EditorStyles.boldLabel);
                
                for (int i = 0; i < manager.projectionFaces.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"면 {i+1}: {manager.projectionFaces[i].name}");
                    
                    if (GUILayout.Button("법선 계산", GUILayout.Width(80)))
                    {
                        if (manager.projectionFaces[i].faceTransform != null)
                        {
                            manager.projectionFaces[i].CalculateNormal();
                            EditorUtility.SetDirty(target);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("오류", "이 면의 Transform이 설정되지 않았습니다.", "확인");
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 파티클 설정
        showParticleSettings = EditorGUILayout.Foldout(showParticleSettings, "파티클 설정", true, EditorStyles.foldoutHeader);
        if (showParticleSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(crossFaceParticleSystemProperty);
            EditorGUILayout.PropertyField(particleSpeedProperty);
            EditorGUILayout.PropertyField(faceCrossingDistanceProperty);
            EditorGUILayout.PropertyField(maxParticlesProperty);
            
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    // 씬 뷰에서 추가 정보 표시
    [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    static void DrawGizmosSelected(CrossFaceParticleManager manager, GizmoType gizmoType)
    {
        if (manager.projectionFaces == null) return;
        
        for (int i = 0; i < manager.projectionFaces.Length; i++)
        {
            var face = manager.projectionFaces[i];
            if (face.faceTransform == null) continue;
            
            // 면 사이의 연결선 표시
            for (int j = i + 1; j < manager.projectionFaces.Length; j++)
            {
                var otherFace = manager.projectionFaces[j];
                if (otherFace.faceTransform == null) continue;
                
                Gizmos.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
                Gizmos.DrawLine(face.faceTransform.position, otherFace.faceTransform.position);
            }
        }
    }
} 