using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteInEditMode]
public class AnimationSimulatorWindow : EditorWindow
{
    List<AnimationClip> allClips = new List<AnimationClip>();
    Animator currentAnimator = null;
    AnimationClip currentAnimClip = null;

    private float _lastEditorTime = 0f;
    private bool _isSimulatingAnimation = false;
    private bool _isPayingMode = false;

    [MenuItem("MyWindow/Animation Simulator")]
    public static void ShowWindow()
    {
        GetWindow<AnimationSimulatorWindow>(false, "Animation Simulator", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("Select the animator", EditorStyles.boldLabel);

        foreach (var animator in Resources.FindObjectsOfTypeAll<Animator>())
        {
            if (GUILayout.Button(animator.name))
            {
                currentAnimClip = null;
                currentAnimator = null;

                SceneView.FrameLastActiveSceneView();
                Selection.activeGameObject = animator.gameObject;
                SceneView.FrameLastActiveSceneView();

                currentAnimator = animator;
                currentAnimator.speed = 0.5f;
            }
        }
        
        GetStateAnimator();
    }

    void GetStateAnimator()
    {
        if (currentAnimator == null)
            return;

        DrawSeparatorLine();

        GUILayout.Label("Animation of " + currentAnimator.name, EditorStyles.boldLabel);

        for (int i = 0; i < allClips.Count; i++)
        {
            allClips.RemoveAt(i);
        }

        AnimatorController ac = currentAnimator.runtimeAnimatorController as AnimatorController;

        foreach(AnimationClip animationClip in ac.animationClips)
        {
            allClips.Add(animationClip);
            if (GUILayout.Button($"{animationClip.name}"))
            {
                if (_isPayingMode) return;

                currentAnimClip = animationClip;
                
                StartAnimSimulation();
            }
        }

        GetInformationClip();
    }

    void GetInformationClip()
    {
        if (currentAnimClip == null) return;

        DrawSeparatorLine();

        GUILayout.Label("Informations of " + currentAnimClip.name, EditorStyles.boldLabel);

        GUILayout.Label("Duration clip : " + currentAnimClip.length);
        GUILayout.Label("Loop activated : " + currentAnimClip.isLooping);
    }

    private void OnEditorUpdate()
    {
        if (currentAnimator == null) return;

        float animTime = Time.realtimeSinceStartup - _lastEditorTime;
        if (animTime >= currentAnimClip.length)
            StopAnimSimulation();
        else
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.SampleAnimationClip(Selection.activeGameObject, currentAnimClip, animTime);
        }
    }

    public void StartAnimSimulation()
    {
        AnimationMode.StartAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
        _lastEditorTime = Time.realtimeSinceStartup;
        _isSimulatingAnimation = true;
    }
    public void StopAnimSimulation()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        _isSimulatingAnimation = false;
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            StopAnimSimulation();
            _isPayingMode = !_isPayingMode;
        }
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= LogPlayModeState;
        StopAnimSimulation();
    }

    private void DrawSeparatorLine()
    {
        EditorGUILayout.Space(10);
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
    }
}
