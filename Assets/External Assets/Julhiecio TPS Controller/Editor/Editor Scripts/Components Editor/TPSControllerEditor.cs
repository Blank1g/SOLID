using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using JUTPS.Utilities;

namespace JUTPS.CustomEditors
{
    [CustomEditor(typeof(JUCharacterController))]
    public class TPSControllerEditor : Editor
    {
        public bool MovementSettings, GroundCheckSettings, WallCheckSettings, StepCorrectionSettings, FireModeSettingsTab, EventsSettings, AnimatorSettings, States, AdditionalSettings;
        public bool AutoFindWeapons;
        private void OnEnable()
        {
            JUCharacterController pl = (JUCharacterController)target;

            PLlayerMasksStartup(pl);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            JUCharacterController pl = (JUCharacterController)target;

            JUTPSEditor.CustomEditorUtilities.JUTPSTitle("Character Controller");

            MovementSettings = GUILayout.Toggle(MovementSettings, "Locomotion", JUTPSEditor.CustomEditorStyles.Toolbar());
            MovementSettingsVariables(pl);

            GroundCheckSettings = GUILayout.Toggle(GroundCheckSettings, "Ground Check", JUTPSEditor.CustomEditorStyles.Toolbar());
            GroundCheckSettingsVariables(pl);

            WallCheckSettings = GUILayout.Toggle(WallCheckSettings, "Wall Check", JUTPSEditor.CustomEditorStyles.Toolbar());
            WallCheckSettingsVariables(pl);

            StepCorrectionSettings = GUILayout.Toggle(StepCorrectionSettings, "Auto Step Up", JUTPSEditor.CustomEditorStyles.Toolbar());
            StepCorrectionSettingsVariables(pl);

            FireModeSettingsTab = GUILayout.Toggle(FireModeSettingsTab, "Fire Mode", JUTPSEditor.CustomEditorStyles.Toolbar());
            FireModeSettings(pl);

            AnimatorSettings = GUILayout.Toggle(AnimatorSettings, "Animator", JUTPSEditor.CustomEditorStyles.Toolbar());
            AnimatorSettingsVariables();

            EventsSettings = GUILayout.Toggle(EventsSettings, "Death Events", JUTPSEditor.CustomEditorStyles.Toolbar());
            EventsSettingsVariables(pl);

            AdditionalSettings = GUILayout.Toggle(AdditionalSettings, "Controller Options", JUTPSEditor.CustomEditorStyles.Toolbar());
            AdditionalSettingsDrawer(pl);

            States = GUILayout.Toggle(States, "Controller States", JUTPSEditor.CustomEditorStyles.Toolbar());
            StatesViewVariables(pl);

            serializedObject.ApplyModifiedProperties();
            //DrawDefaultInspector();
        }
        public void ExempleSettingsVariables(JUCharacterController pl)
        {
            if (MovementSettings)
            {
                CharacterSettingsGizmosViewerr(pl);
            }
        }
        public void MovementSettingsVariables(JUCharacterController pl)
        {
            if (MovementSettings)
            {
                //Move On Forward When Isnt Aiming
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pl.LocomotionMode)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pl.SetRigidbodyVelocity)));

                serializedObject.FindProperty("Speed").floatValue = EditorGUILayout.Slider("  Movement Speed", pl.Speed, 0, 30);
                serializedObject.FindProperty("RotationSpeed").floatValue = EditorGUILayout.Slider("  Rotation Speed", pl.RotationSpeed, 0, 30);
                serializedObject.FindProperty("JumpForce").floatValue = EditorGUILayout.Slider("  Jump Force", pl.JumpForce, 1, 10);
                serializedObject.FindProperty("AirInfluenceControll").floatValue = EditorGUILayout.Slider("  In Air Control Force", pl.AirInfluenceControll, 0, 100);
                serializedObject.FindProperty("StoppingSpeed").floatValue = EditorGUILayout.Slider("  Stopping Speed", pl.StoppingSpeed, 0.1f, 5);
                serializedObject.FindProperty("MaxWalkableAngle").floatValue = EditorGUILayout.Slider("  Max Walkable Angle", pl.MaxWalkableAngle, 0, 89);

                serializedObject.FindProperty("MovementAffectsWeaponAccuracy").boolValue = EditorGUILayout.ToggleLeft("  Movement Affects Weapon Accuracy", pl.MovementAffectsWeaponAccuracy, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                if (pl.MovementAffectsWeaponAccuracy)
                {
                    serializedObject.FindProperty("OnMovePrecision").floatValue = EditorGUILayout.Slider("  On Move Precision", pl.OnMovePrecision, 0, 16);
                }


                serializedObject.FindProperty("GroundAngleDesaceleration").boolValue = EditorGUILayout.ToggleLeft("  High Inclines Slow Down", pl.GroundAngleDesaceleration, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                if (pl.GroundAngleDesaceleration)
                {
                    serializedObject.FindProperty("GroundAngleDesacelerationMultiplier").floatValue = EditorGUILayout.Slider("  Intensity", pl.GroundAngleDesacelerationMultiplier, 0, 2);
                }

                serializedObject.FindProperty("CurvedMovement").boolValue = EditorGUILayout.ToggleLeft("  Curved Movement", pl.CurvedMovement, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                serializedObject.FindProperty("LerpRotation").boolValue = EditorGUILayout.ToggleLeft("  Lerp Rotation", pl.LerpRotation, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());

                serializedObject.FindProperty("BodyInclination").boolValue = EditorGUILayout.ToggleLeft("  Body Lean", pl.BodyInclination, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());

                serializedObject.FindProperty("RootMotion").boolValue = EditorGUILayout.ToggleLeft("  Root Motion", pl.RootMotion, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                if (pl.RootMotion)
                {
                    serializedObject.FindProperty("RootMotionSpeed").floatValue = EditorGUILayout.Slider("  Root Motion Speed", pl.RootMotionSpeed, 0, 10);
                    serializedObject.FindProperty("RootMotionRotation").boolValue = EditorGUILayout.Toggle("  Root Motion Rotation", pl.RootMotionRotation);
                }
                serializedObject.FindProperty("AutoRun").boolValue = EditorGUILayout.ToggleLeft("  Auto Run", pl.AutoRun, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                if (pl.AutoRun)
                {
                    serializedObject.FindProperty("WalkOnRunButton").boolValue = EditorGUILayout.Toggle("  Walk On Run Button", pl.WalkOnRunButton);
                    serializedObject.FindProperty("SprintOnRunButton").boolValue = EditorGUILayout.Toggle("  Sprint On Run Button", pl.SprintOnRunButton);
                    serializedObject.FindProperty("UnlimitedSprintDuration").boolValue = EditorGUILayout.Toggle("  Unlimited Sprint Duration", pl.UnlimitedSprintDuration);

                }
                serializedObject.FindProperty("SprintingSkill").boolValue = EditorGUILayout.ToggleLeft("  Enable Sprint Skill", pl.SprintingSkill, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
                serializedObject.FindProperty("DecreaseSpeedOnJump").boolValue = EditorGUILayout.ToggleLeft("  Decrease Speed On Jump", pl.DecreaseSpeedOnJump, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());

                CharacterSettingsGizmosViewerr(pl);
            }
        }
        public void GroundCheckSettingsVariables(JUCharacterController pl)
        {
            if (GroundCheckSettings == true)
            {
                LayerMask tempMask = EditorGUILayout.MaskField("  Ground Layer", UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(pl.WhatIsGround), UnityEditorInternal.InternalEditorUtility.layers);
                serializedObject.FindProperty("WhatIsGround").intValue = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

                serializedObject.FindProperty("GroundCheckRadius").floatValue = EditorGUILayout.Slider("  Radius", pl.GroundCheckRadius, 0.01f, 0.2f);
                serializedObject.FindProperty("GroundCheckSize").floatValue = EditorGUILayout.Slider("  Height", pl.GroundCheckSize, 0.05f, 0.5f);
                serializedObject.FindProperty("GroundCheckHeighOfsset").floatValue = EditorGUILayout.Slider("  Up Ofsset", pl.GroundCheckHeighOfsset, -1f, 1f);
                CharacterSettingsGizmosViewerr(pl);
            }
        }
        public void WallCheckSettingsVariables(JUCharacterController pl)
        {
            if (WallCheckSettings == true)
            {
                LayerMask tempMask = EditorGUILayout.MaskField("  Wall Layers", UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(pl.WhatIsWall), UnityEditorInternal.InternalEditorUtility.layers);
                serializedObject.FindProperty("WhatIsWall").intValue = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

                serializedObject.FindProperty("WallRayHeight").floatValue = EditorGUILayout.Slider("  Wall Ray Height", pl.WallRayHeight, -5, 5);
                serializedObject.FindProperty("WallRayDistance").floatValue = EditorGUILayout.Slider("  Wall Ray Distance", pl.WallRayDistance, 0.1f, 5f);
                CharacterSettingsGizmosViewerr(pl);
            }
        }

        public void StepCorrectionSettingsVariables(JUCharacterController pl)
        {
            if (StepCorrectionSettings)
            {
                serializedObject.FindProperty("EnableStepCorrection").boolValue = EditorGUILayout.Toggle("  Step Correction", pl.EnableStepCorrection);
                serializedObject.FindProperty("UpStepSpeed").floatValue = EditorGUILayout.Slider("  Up Step Speed", pl.UpStepSpeed, 2, 15);

                LayerMask tempMask = EditorGUILayout.MaskField("  Step Correction Layers", UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(pl.StepCorrectionMask), UnityEditorInternal.InternalEditorUtility.layers);
                serializedObject.FindProperty("StepCorrectionMask").intValue = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

                serializedObject.FindProperty("FootstepHeight").floatValue = EditorGUILayout.Slider("  Step Raycast Distance", pl.FootstepHeight, 0.1f, 1f);
                serializedObject.FindProperty("ForwardStepOffset").floatValue = EditorGUILayout.Slider("  Forward Offset", pl.ForwardStepOffset, 0f, 1f);
                serializedObject.FindProperty("StepHeight").floatValue = EditorGUILayout.Slider("  Step Height", pl.StepHeight, 0.01f, pl.FootstepHeight);

                CharacterSettingsGizmosViewerr(pl);
            }
        }


        public List<GameObject> PrefabListToAdd = new List<GameObject>();
        public void DropItensField(JUCharacterController pl)
        {
            //Get current events
            Event GUIEvent = Event.current;

            //Draw drop box area
            Rect DragAndDropItemArea = GUILayoutUtility.GetRect(0.0f, 35.0f, GUILayout.ExpandWidth(true));
            GUI.Box(DragAndDropItemArea, "Drop item prefab here to add +", JUTPSEditor.CustomEditorStyles.MiniToolbar());

            // Receive dropped itens
            switch (GUIEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!DragAndDropItemArea.Contains(GUIEvent.mousePosition)) { return; }
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (GUIEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (GameObject DropedGameObject in DragAndDrop.objectReferences)
                        {
                            PrefabListToAdd.Add(DropedGameObject);
                        }
                    }
                    break;
            }

            //ADD ITENS
            if (PrefabListToAdd.Count > 0)
            {
                foreach (GameObject ItemToAdd in PrefabListToAdd)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(" + | " + ItemToAdd.name, JUTPSEditor.CustomEditorStyles.NormalStateStyle(), GUILayout.Width(200));
                    if (GUILayout.Button("X", JUTPSEditor.CustomEditorStyles.DangerButtonStyle(), GUILayout.Width(20)))
                    {
                        PrefabListToAdd.Remove(ItemToAdd);
                        Debug.Log("deleted");
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add Itens", EditorStyles.miniButtonMid))
                {
                    //Null error
                    if (pl.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand) == null)
                    {
                        Debug.LogError("Items cannot be added because the character's right hand cannot be found.");
                        return;
                    }
                    //Add itens
                    foreach (GameObject ItemToAdd in PrefabListToAdd)
                    {
                        Vector3 rotation = new Vector3(-100, -180, 280);
                        Vector3 position = pl.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand).position;
                        var item = Instantiate(ItemToAdd, position, Quaternion.identity, pl.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand));
                        item.transform.localEulerAngles = rotation;
                    }
                    PrefabListToAdd.Clear();
                }
                GUILayout.Space(20);
            }


        }

        public void FireModeSettings(JUCharacterController pl)
        {
            if (FireModeSettingsTab)
            {
                //serializedObject.FindProperty("PivotItemRotation").objectReferenceValue = EditorGUILayout.ObjectField("Item Aim Rotation Center", pl.PivotItemRotation, typeof(GameObject), true) as GameObject;
                //serializedObject.FindProperty("HumanoidSpine").objectReferenceValue = EditorGUILayout.ObjectField("Upper Chest Spine Bone", pl.HumanoidSpine, typeof(GameObject), true) as Transform;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pl.PivotItemRotation)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pl.HumanoidSpine)));

                //if (pl.HumanoidSpine == null)
                //{
                //    pl.HumanoidSpine = pl.GetLastSpineBone();
                //}
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AimMode"));
                serializedObject.FindProperty("FireModeMaxTime").floatValue = EditorGUILayout.Slider("FireMode Max Time", pl.FireModeMaxTime, 0, 50);
            }
        }
        public void AnimatorSettingsVariables()
        {
            if (AnimatorSettings)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("anim"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatorParameters"));
            }
        }
        public void EventsSettingsVariables(JUCharacterController pl)
        {
            if (EventsSettings)
            {
                serializedObject.FindProperty("RagdollWhenDie").boolValue = EditorGUILayout.ToggleLeft("Enable Ragdoll When Die", pl.RagdollWhenDie, JUTPSEditor.CustomEditorStyles.MiniLeftButtonStyle());
            }
        }
        public void StatesViewVariables(JUCharacterController pl)
        {
            if (States)
            {
                if (pl.CharacterHealth != null)
                {
                    //Style
                    var lifestyle = new GUIStyle(JUTPSEditor.CustomEditorStyles.Toolbar());
                    Color fullifecolor = new Color(0.2f, 1, 0.1f);
                    Color nolifecolor = new Color(1f, 0.5f, 0.5f);
                    lifestyle.normal.textColor = Color.Lerp(nolifecolor, fullifecolor, pl.CharacterHealth.Health / pl.CharacterHealth.MaxHealth);
                    lifestyle.alignment = TextAnchor.MiddleCenter;
                    lifestyle.fontSize = 12;

                    //Health Display
                    int health_int = (int)pl.CharacterHealth.Health;
                    EditorGUILayout.LabelField("Health: " + health_int.ToString() + "%", lifestyle, GUILayout.Width(120));
                    //Health Slider
                    pl.CharacterHealth.Health = GUILayout.HorizontalSlider(pl.CharacterHealth.Health, 0, pl.CharacterHealth.MaxHealth, GUILayout.Width(120), GUILayout.Height(2));
                }
                else
                {
                    EditorGUILayout.LabelField("Without health status, add the JU Health component");
                    if (pl.GetComponent<JUHealth>() != null)
                    {
                        pl.CharacterHealth = pl.GetComponent<JUHealth>();
                    }
                }
                GUILayout.Space(20);


                GUILayout.BeginHorizontal();
                GUILayout.Toggle(pl.IsDead, "Dead", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsMoving, "Moving", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsRunning, "Running", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Toggle(pl.IsRolling, "Rolling", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsGrounded, "Grounded", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsJumping, "Jumping", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Toggle(pl.IsMeleeAttacking, "Attacking", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsItemEquiped, "Armed", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.IsDriving, "Driving", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Toggle(pl.ToEnterVehicle, "Can Drive Vehicle", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.ToPickupItem, "To Pick Up Weapon", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.FiringModeIK, "Inverse Kinematics", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Toggle(pl.WallAHead, "Wall Ahead", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.Toggle(pl.UsedItem, "Shooting", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                //GUILayout.Toggle(pl.CanUseItem, "Can Shoot", JUTPSEditor.CustomEditorStyles.StateStyle(), GUILayout.Width(120));
                GUILayout.EndHorizontal();


                GUILayout.Space(10);

            }
        }

        public void AdditionalSettingsDrawer(JUCharacterController pl)
        {
            if (AdditionalSettings)
            {
                //serializedObject.FindProperty("AutoRun").boolValue = EditorGUILayout.Toggle("  Auto Run", pl.AutoRun);
                //serializedObject.FindProperty("WalkOnRunButton").boolValue = EditorGUILayout.Toggle("  Walk On Run Button", pl.WalkOnRunButton);
                EditorGUILayout.LabelField("Block Movement Input", EditorStyles.boldLabel);
                serializedObject.FindProperty("BlockVerticalInput").boolValue = EditorGUILayout.Toggle("  Block Vertical Input", pl.BlockVerticalInput);
                serializedObject.FindProperty("BlockHorizontalInput").boolValue = EditorGUILayout.Toggle("  Block Horizontal Input", pl.BlockHorizontalInput);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Block Firing Mode", EditorStyles.boldLabel);
                serializedObject.FindProperty("BlockFireModeOnCursorVisible").boolValue = EditorGUILayout.Toggle("  Block FireMode On Cursor Visible", pl.BlockFireModeOnCursorVisible);
                serializedObject.FindProperty("BlockFireModeOnPunching").boolValue = EditorGUILayout.Toggle("  Block FireMode On Punching", pl.BlockFireModeOnPunching);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Default Skills", EditorStyles.boldLabel);
                serializedObject.FindProperty("EnablePunchAttacks").boolValue = EditorGUILayout.Toggle("  Enable Punch Attacks", pl.EnablePunchAttacks);
                serializedObject.FindProperty("EnableRoll").boolValue = EditorGUILayout.Toggle("  Enable Roll", pl.EnableRoll);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Artificial Intelligence", EditorStyles.boldLabel);
                serializedObject.FindProperty("IsArtificialIntelligence").boolValue = EditorGUILayout.Toggle("  Is AI", pl.IsArtificialIntelligence);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Physical Damage System", EditorStyles.boldLabel);
                serializedObject.FindProperty(nameof(pl.PhysicalDamage)).boolValue = EditorGUILayout.Toggle("  Enable Physical Damage", pl.PhysicalDamage); 
                serializedObject.FindProperty(nameof(pl.DoRagdollPhysicalDamage)).boolValue = EditorGUILayout.Toggle("  Do Ragdoll On Physical Damage", pl.DoRagdollPhysicalDamage);
                serializedObject.FindProperty(nameof(pl.PhysicalDamageStartAt)).floatValue = EditorGUILayout.FloatField("  Physical Damage Start At", pl.PhysicalDamageStartAt);
                serializedObject.FindProperty(nameof(pl.PhysicalDamageMultiplier)).floatValue = EditorGUILayout.FloatField("  Physical Damage Multiplier", pl.PhysicalDamageMultiplier);
                serializedObject.FindProperty(nameof(pl.RagdollStartAtDamage)).floatValue = EditorGUILayout.FloatField("  Ragdoll Start At Damage", pl.RagdollStartAtDamage);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(pl.PhysicalDamageIgnoreTags)));

                EditorGUILayout.Space();

            }
        }






        //Utility Functions
        //public void SetAllItensInInventory(JUThirdPersonController pl)
        //{
        /*
         * 
        if (pl.anim == null) pl.anim = pl.GetComponent<Animator>();

        Transform leftHand = pl.anim.GetBoneTransform(HumanBodyBones.LeftHand);
        Transform rightHand = pl.anim.GetBoneTransform(HumanBodyBones.RightHand);

        //Set in hands items
        if (leftHand != null)
        {
            pl.HoldableItensLeftHand = leftHand.GetComponentsInChildren<HoldableItem>();
            foreach (HoldableItem item in pl.HoldableItensLeftHand)
            {
                Debug.Log("Added the ''" + item.name + "'' to Left Hand Item List");
            }
            
        }
        if (rightHand != null)
        {
            
            pl.HoldableItensRightHand = rightHand.GetComponentsInChildren<HoldableItem>();
            foreach (HoldableItem item in pl.HoldableItensRightHand)
            {
                Debug.Log("Added the ''" + item.name + "'' to Right Hand Item List");
            }
            
        }

        //Get all itens
        pl.Items = pl.GetComponentsInChildren<Item>();

        */
        //pl.HoldableItensRightHand = pl.GetComponentsInChildren<HoldableItem>();
        //}
        public void CharacterSettingsGizmosViewerr(JUCharacterController pl)
        {
            if (pl.TryGetComponent(out JUCharacterSettingsDrawer pldrw) == false)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Warning: there is no *JU Character Settings Drawer* on your character", MessageType.Warning);
                if (GUILayout.Button("Add Component ''JU Character Settings Drawer'' ", EditorStyles.miniButtonMid))
                {
                    JUCharacterSettingsDrawer settingsDrawer = (JUCharacterSettingsDrawer)Undo.AddComponent(pl.gameObject, typeof(JUCharacterSettingsDrawer));
                    settingsDrawer.GroundCheck = true;
                    settingsDrawer.StepCorrection = true;
                    Debug.Log("Added JU Character Settings Drawer Component for the character");
                }
                GUILayout.Space(10);
            }
        }
        public void PLlayerMasksStartup(JUCharacterController pl)
        {
            if (pl.WhatIsGround == 0)
                pl.WhatIsGround = JUTPSEditor.LayerMaskUtilities.GroundMask();
            if (pl.StepCorrectionMask == 0)
                pl.StepCorrectionMask = JUTPSEditor.LayerMaskUtilities.GroundMask();
            //if (pl.CrosshairHitMask == 0)
            //    pl.CrosshairHitMask = JUTPSEditor.LayerMaskUtilities.CrosshairMask();
        }
    }
}