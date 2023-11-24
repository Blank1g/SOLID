using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JUTPS.WeaponSystem
{

    public class ScopeSystem : MonoBehaviour
    {
        public Image ScopeImage;
        public GameObject UIPanel;
        public JUCharacterController JUCharacter;
        bool ScopeMode;
        void Start()
        {
            if (JUCharacter == null && GameObject.FindGameObjectWithTag("Player") != null)
            {
                JUCharacter = GameObject.FindGameObjectWithTag("Player").GetComponent<JUCharacterController>();
            }
        }

        void Update()
        {
            if (JUCharacter == null) return;
            ScopeMode = false;
            //if the character have an item in right hand 
            if (JUCharacter.HoldableItemInUseRightHand != null)
            {
                //if item is a weapon
                if (JUCharacter.HoldableItemInUseRightHand is Weapon)
                {
                    Weapon currentWeapon = (Weapon)JUCharacter.HoldableItemInUseRightHand;
                    //if is aiming and Weapon Aim Mode is Scope Mode
                    if (JUCharacter.IsAiming && currentWeapon.AimMode == Weapon.WeaponAimMode.Scope)
                    {
                        //Switch Scope Image
                        ScopeImage.sprite = currentWeapon.ScopeTexture;
                        ScopeMode = true;
                    }
                }
            }

            //Enable/Disable UI Scope Image
            ScopeImage.gameObject.SetActive((JUCharacter.IsAiming && ScopeMode));
            UIPanel.SetActive(!(JUCharacter.IsAiming && ScopeMode));
        }
    }

}
