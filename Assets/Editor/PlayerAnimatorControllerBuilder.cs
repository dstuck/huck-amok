using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Rebuilds PlayerAnimatorController with separate 2D Simple Directional blend trees
/// for Idle, Walk, and Carry (DirectionX / DirectionY).
/// </summary>
public static class PlayerAnimatorControllerBuilder
{
    private const string ControllerPath = "Assets/Animations/Player/PlayerAnimatorController.controller";
    private const string ClipsRoot = "Assets/Animations/Player";

    private static readonly (string suffix, Vector2 position)[] DirectionClips =
    {
        ("N", new Vector2(0f, 1f)),
        ("E", new Vector2(1f, 0f)),
        ("S", new Vector2(0f, -1f)),
        ("W", new Vector2(-1f, 0f)),
    };

    [MenuItem("Huck Amok/Rebuild Player Animator Controller (Blend Trees)")]
    public static void RebuildFromMenu()
    {
        if (Rebuild())
            Debug.Log("PlayerAnimatorController rebuilt with 2D direction blend trees.");
        else
            Debug.LogError("Failed to rebuild PlayerAnimatorController.");
    }

    public static void RebuildBatchmode()
    {
        if (!Rebuild())
            Debug.LogError("PlayerAnimatorController batch rebuild failed.");
        else
            Debug.Log("PlayerAnimatorController batch rebuild succeeded.");
    }

    public static bool Rebuild()
    {
        var controller = LoadOrCreateController();
        if (controller == null)
            return false;

        ClearControllerContents(controller);

        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsCarrying", AnimatorControllerParameterType.Bool);
        controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
        controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);

        var sm = controller.layers[0].stateMachine;
        if (sm == null)
        {
            Debug.LogError("PlayerAnimatorController: state machine still missing after recreate.");
            return false;
        }

        var idleState = sm.AddState("Idle", new Vector3(300f, 0f, 0f));
        idleState.motion = CreateDirectionBlendTree(controller, "IdleBlend", "Idle");

        var walkState = sm.AddState("Walk", new Vector3(300f, 120f, 0f));
        walkState.motion = CreateDirectionBlendTree(controller, "WalkBlend", "Walk");

        var carryState = sm.AddState("Carry", new Vector3(300f, 240f, 0f));
        carryState.motion = CreateDirectionBlendTree(controller, "CarryBlend", "Carry");

        sm.defaultState = idleState;

        AddAnyStateTransition(sm, idleState,
            (AnimatorConditionMode.IfNot, "IsMoving"),
            (AnimatorConditionMode.IfNot, "IsCarrying"));

        AddAnyStateTransition(sm, walkState,
            (AnimatorConditionMode.If, "IsMoving"),
            (AnimatorConditionMode.IfNot, "IsCarrying"));

        AddAnyStateTransition(sm, carryState,
            (AnimatorConditionMode.If, "IsCarrying"));

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return true;
    }

    private static AnimatorController LoadOrCreateController()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        bool broken = controller == null
            || controller.layers.Length == 0
            || controller.layers[0].stateMachine == null;

        if (!broken)
            return controller;

        if (controller != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        return AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
    }

    private static void ClearControllerContents(AnimatorController controller)
    {
        for (int i = controller.parameters.Length - 1; i >= 0; i--)
            controller.RemoveParameter(i);

        foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(ControllerPath))
        {
            if (sub == null || sub == controller || sub is AnimatorStateMachine)
                continue;

            Object.DestroyImmediate(sub, true);
        }

        var sm = controller.layers[0].stateMachine;
        if (sm == null)
            return;

        foreach (var transition in sm.anyStateTransitions.ToArray())
            sm.RemoveAnyStateTransition(transition);
        foreach (var transition in sm.entryTransitions.ToArray())
            Object.DestroyImmediate(transition, true);
        foreach (var childState in sm.states.ToArray())
            sm.RemoveState(childState.state);
        foreach (var child in sm.stateMachines.ToArray())
            sm.RemoveStateMachine(child.stateMachine);
    }

    private static BlendTree CreateDirectionBlendTree(AnimatorController controller, string treeName, string clipPrefix)
    {
        var tree = new BlendTree
        {
            name = treeName,
            blendType = BlendTreeType.SimpleDirectional2D,
            blendParameter = "DirectionX",
            blendParameterY = "DirectionY",
        };
        AssetDatabase.AddObjectToAsset(tree, controller);

        foreach (var (suffix, position) in DirectionClips)
        {
            var path = $"{ClipsRoot}/{clipPrefix}{suffix}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                throw new System.IO.FileNotFoundException($"Missing animation clip: {path}");

            tree.AddChild(clip, position);
        }

        return tree;
    }

    private static void AddAnyStateTransition(
        AnimatorStateMachine sm,
        AnimatorState destination,
        params (AnimatorConditionMode mode, string parameter)[] conditions)
    {
        var transition = sm.AddAnyStateTransition(destination);
        transition.hasExitTime = false;
        transition.duration = 0f;
        transition.canTransitionToSelf = false;

        foreach (var (mode, parameter) in conditions)
            transition.AddCondition(mode, 0f, parameter);
    }
}
