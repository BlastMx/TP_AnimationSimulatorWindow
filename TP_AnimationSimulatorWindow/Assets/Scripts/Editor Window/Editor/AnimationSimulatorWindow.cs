using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteInEditMode]
public class AnimationSimulatorWindow : EditorWindow
{
    List<AnimatorState> allStates = new List<AnimatorState>();
    Animator currentAnimator = null;

    private bool playAnim = false;

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
                //Focus gameObject
                SceneView.FrameLastActiveSceneView();
                Selection.activeGameObject = animator.gameObject;
                SceneView.FrameLastActiveSceneView();

                currentAnimator = animator;

                playAnim = false;
            }
        }
        
        //Lister tout les states de l'animator selectionné
        GetStateAnimator();
    }

    void GetStateAnimator()
    {
        if (currentAnimator == null)
            return;

        DrawSeparatorLine();

        GUILayout.Label("Animation of " + currentAnimator.name, EditorStyles.boldLabel);

        for (int i = 0; i < allStates.Count; i++)
        {
            allStates.RemoveAt(i);
        }

        AnimatorController ac = currentAnimator.runtimeAnimatorController as AnimatorController;
        AnimatorControllerLayer[] acLayers = ac.layers;

        foreach (AnimatorControllerLayer animatorController in acLayers)
        {
            ChildAnimatorState[] animatorStates = animatorController.stateMachine.states;
            foreach (ChildAnimatorState childAnimator in animatorStates)
            {
                allStates.Add(childAnimator.state);
                if (GUILayout.Button($"{childAnimator.state.name}"))
                {
                    //Play animation selected
                    currentAnimator.Play(childAnimator.state.name);
                    playAnim = true;
                    
                }
            }
        }
    }

    private void Update()
    {
        //Play animation in edit mode
        if(Selection.activeGameObject != null && playAnim)
            currentAnimator.Update(Time.deltaTime);
    }

    static AnimationSimulatorWindow()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            AnimationMode.StopAnimationMode();
        }
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
