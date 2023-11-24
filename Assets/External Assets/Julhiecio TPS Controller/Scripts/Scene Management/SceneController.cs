using UnityEngine;
using UnityEngine.SceneManagement;
using JUTPS.PhysicsScripts;
using JUTPS.InventorySystem;

namespace JUTPS
{

    [AddComponentMenu("JU TPS/Scene Management/Scene Controller")]
    public class SceneController : MonoBehaviour
    {
        JUCharacterController pl;
        public bool ReloadLevelWhenDie;
        public float SecondsToRespawnOrReloadLevel = 4;
        public bool JustRespawnPlayer;
        private Vector3 SpawnPlayerPostion;
        //public bool ExitGameWhenPressEsc;
        //public bool ResetLevelWhenPressP;
        void Start()
        {
            GameObject playerGameobject = GameObject.FindGameObjectWithTag("Player");
            pl = playerGameobject?.GetComponent<JUCharacterController>();

            SpawnPlayerPostion = (pl != null) ? pl.transform.position : Vector3.zero;
        }
        void Update()
        {
            if (pl == null) return;

            /*
            if (Input.GetKeyDown(KeyCode.Escape) && ExitGameWhenPressEsc == true)
            {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.P) && ResetLevelWhenPressP == true)
            {
                ResetLevel();
            }*/
            if (pl.IsDead == true && IsInvoking("ResetLevel") == false && ReloadLevelWhenDie == true && JustRespawnPlayer == false)
            {
                Invoke("ResetLevel", SecondsToRespawnOrReloadLevel);
            }
            if (pl.IsDead == true && IsInvoking("RespawnPlayer") == false && JustRespawnPlayer == true)
            {
                Invoke("RespawnPlayer", SecondsToRespawnOrReloadLevel);
            }
        }
        public void ResetLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        public void RespawnPlayer()
        {
            //Reset Ragdoll
            if (pl.TryGetComponent(out AdvancedRagdollController ARC))
            {
                pl.anim.GetBoneTransform(HumanBodyBones.Hips).SetParent(ARC.HipsParent);
                ARC.State = AdvancedRagdollController.RagdollState.BlendToAnim;
                ARC.TimeToGetUp = 2;
                ARC.BlendAmount = 0;
                ARC.SetActiveRagdoll(false);
                pl.enableMove();
            }
            else
            {
                pl.enableMove();
            }

            //Reset Position
            pl.transform.position = SpawnPlayerPostion;

            //Reset Health
            pl.CharacterHealth.Health = pl.CharacterHealth.MaxHealth;
            pl.IsDead = false;

            //Reset layer
            pl.gameObject.layer = 9;

            //Reset Collider
            pl.GetComponent<Collider>().isTrigger = false;
            pl.GetComponent<Collider>().enabled = true;

            //Reset Rigidbody
            pl.GetComponent<Rigidbody>().useGravity = true;
            pl.GetComponent<Rigidbody>().isKinematic = false;
            pl.GetComponent<Rigidbody>().velocity = transform.up * pl.GetComponent<Rigidbody>().velocity.y;
            pl.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;

            //Enable Tps Script
            pl.enabled = true;

            //Reset Inventory
            pl.GetComponent<JUInventory>().IsALoot = false;

            //Reset Animator
            pl.anim.enabled = true;
            pl.anim.SetBool(pl.AnimatorParameters.Dying, false);
            pl.anim.Play("Locomotion Blend Tree", 0);
            pl.ResetDefaultLayersWeight();

            if (pl.HoldableItemInUseRightHand != null) pl.SwitchToItem(-1);
            Debug.Log("Player has respawned");
        }
        public void SetRespawnPosition(Vector3 position)
        {
            SpawnPlayerPostion = position;
        }
    }

}