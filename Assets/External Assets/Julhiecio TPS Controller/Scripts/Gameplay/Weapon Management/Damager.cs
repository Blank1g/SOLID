using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JUTPS.ArmorSystem;
using JUTPS.FX;

using JUTPSEditor.JUHeader;

namespace JUTPS
{
    [AddComponentMenu("JU TPS/Armor System/JU Damager")]
    public class Damager : MonoBehaviour
    {
        [JUHeader("Damager Settings")]
        public float Damage = 20;
        public bool DisableOnStart = true;
        public float HitMinTime = 0.5f;
        private float CurrentHitTime;

        [JUHeader("Damage Detection Settings")]
        public bool RaycastingMode = true;
        public LayerMask RaycastCollideWith;
        public float RaycastDistance;

        [JUHeader("Collision Detection Mode Settings")]
        public bool IgnoreAllCollidersOfRootGameobject = true;
        public Collider[] AllRootGameobjectColliders;
        public bool LockStartPosition;

        [JUHeader("FX Settings")]
        public string[] TagsToDamage = { "Untagged", "Skin", "Player", "Enemy" };
        public List<SurfaceAudiosWithFX> HitParticlesList = new List<SurfaceAudiosWithFX>();
        public AudioSource HitSoundsAudioSource;

        [HideInInspector] public bool Collided;
        private Vector3 startedLocalPosition;
        private Rigidbody rb;
        private bool CanHit = true;

        public GameObject Owner;
        private void Awake()
        {
            startedLocalPosition = transform.localPosition;
            CharacterBrain.JUCharacterBrain tpsCharacter = GetComponentInParent<CharacterBrain.JUCharacterBrain>();
            Owner = tpsCharacter == null ? null : tpsCharacter.gameObject;
        }
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (IgnoreAllCollidersOfRootGameobject)
            {
                AllRootGameobjectColliders = transform.root.GetComponentsInChildren<Collider>();

                if (GetComponent<Collider>() == null) return;

                Collider DamageCollider = GetComponent<Collider>();
                foreach (Collider col in AllRootGameobjectColliders)
                {
                    if (col != DamageCollider)
                    {
                        Physics.IgnoreCollision(col, DamageCollider, true);
                    }
                }
            }
            if (DisableOnStart) gameObject.SetActive(false);
        }
        private GameObject oldHitedCollider;
        private RaycastHit hit;

        private void Update()
        {
            if (LockStartPosition)
            {
                transform.localPosition = startedLocalPosition;
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = false;
                }
            }
 
            if (RaycastingMode == false || RaycastDistance == 0) return;
            //Debug.Log("Is raycasting mode active");
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, RaycastDistance, RaycastCollideWith))
            {
                //Debug.Log("raycasting somethings");
                if (hit.collider.gameObject != oldHitedCollider)
                {
                    for (int i = 0; i < TagsToDamage.Length; i++)
                    {
                        if (hit.collider.transform.tag == TagsToDamage[i] && CanHit)
                        {
                            DoDamage(hit, null, Damage, HitParticlesList, HitSoundsAudioSource, AllRootGameobjectColliders);
                            Collided = true;
                            Invoke(nameof(DisableCollidedState), 0.1f);
                            DisableDamagingForSeconds(HitMinTime);
                        }
                    }
                    oldHitedCollider = hit.collider.gameObject;
                }
            }
            else
            {
                oldHitedCollider = null;
            }
        }
        private void OnDisable()
        {
            oldHitedCollider = null;
        }
        private void DisableCollidedState()
        {
            Collided = false;
        }
        public void DisableDamagingForSeconds(float DisabledSeconds)
        {
            CanHit = false;
            if (IsInvoking(nameof(EnableDamaging)) == false) Invoke(nameof(EnableDamaging), DisabledSeconds);
        }
        public void EnableDamaging()
        {
            CanHit = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            for (int i = 0; i < TagsToDamage.Length; i++)
            {
                if (collision.transform.tag == TagsToDamage[i] && CanHit)
                {
                    if (collision.gameObject.layer == 9)
                    {
                        if (collision.gameObject.GetComponentInChildren<DamageableBodyPart>() != null) return;
                    }

                    DoDamage(collision, Damage, HitParticlesList, HitSoundsAudioSource);
                    Collided = true;
                    Invoke(nameof(DisableCollidedState), 0.1f);
                    DisableDamagingForSeconds(HitMinTime);
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < TagsToDamage.Length; i++)
            {
                if (other.transform.tag == TagsToDamage[i] && CanHit)
                {
                    if (other.gameObject.layer == 9)
                    {
                        if (other.gameObject.GetComponentInChildren<DamageableBodyPart>() != null) return;
                    }

                    DoDamage(other, Damage, HitParticlesList, HitSoundsAudioSource);
                    Collided = true;
                    Invoke(nameof(DisableCollidedState), 0.1f);
                    DisableDamagingForSeconds(HitMinTime);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (RaycastingMode)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * RaycastDistance);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.color = new Color(1, 1, 1, 0.25f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

        public void DoDamage(Collider trigger, float damage, List<SurfaceAudiosWithFX> hitParticles, AudioSource hitAudioSource)
        {
            DamageableBodyPart bodyPart = trigger.gameObject.GetComponentInChildren<DamageableBodyPart>();
            Vector3 damagePoint = trigger.ClosestPoint(transform.position);
            float realDamage = damage;
            if (bodyPart == null)
            {
                JUHealth health = trigger.gameObject.GetComponentInParent<JUHealth>();
                if (health != null)
                {
                    //Do Damage
                    health.DoDamage(damage);

                    //Hit Marker
                    if (Owner != null)
                    {
                        if (health.IsDead == false) HitMarkerEffect.HitCheck(health.transform.tag, Owner.tag, damagePoint, realDamage);
                    }
                }
            }
            else
            {
                realDamage = bodyPart.DoDamage(damage);
                if (Owner != null && bodyPart != null && realDamage > 0)
                {
                    if (bodyPart.Health.IsDead == false) HitMarkerEffect.HitCheck(bodyPart.transform.tag, Owner.tag, damagePoint, realDamage);
                }
            }

            //Instantiate Particle FX
            Vector3 contactPoint = trigger.ClosestPoint(transform.position);

            Vector3 contactNormal = (transform.position - trigger.ClosestPoint(transform.position)).normalized;

            Quaternion particleRotation = Quaternion.LookRotation(contactNormal);
            GameObject fx = SurfaceAudiosWithFX.Play(hitAudioSource, hitParticles, contactPoint, particleRotation, null, bodyPart != null ? bodyPart.tag : trigger.transform.tag);
            fx.transform.parent = trigger.transform;
        }

        public void DoDamage(Collision collision, float damage, List<SurfaceAudiosWithFX> hitParticles, AudioSource hitAudioSource)
        {
            DamageableBodyPart bodyPart = collision.gameObject.GetComponentInChildren<DamageableBodyPart>();
            Vector3 damagePoint = collision.contacts[0].point;
            float realDamage = damage;
            if (bodyPart == null)
            {
                JUHealth health = collision.gameObject.GetComponentInParent<JUHealth>();
                if (health != null)
                {
                    //Do Damage
                    health.DoDamage(damage);

                    //Hit Marker
                    if (Owner != null)
                    {
                        if (health.IsDead == false) HitMarkerEffect.HitCheck(health.transform.tag, Owner.tag, damagePoint, realDamage);
                    }
                }
            }
            else
            {
                realDamage = bodyPart.DoDamage(damage);
                if (Owner != null && bodyPart != null && realDamage > 0)
                {
                    if (bodyPart.Health.IsDead == false) HitMarkerEffect.HitCheck(bodyPart.transform.tag, Owner.tag, damagePoint, realDamage);
                }
            }

            //Instantiate Particle FX
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;
            Quaternion particleRotation = Quaternion.LookRotation(contactNormal);
            GameObject fx = SurfaceAudiosWithFX.Play(hitAudioSource, hitParticles, contactPoint, particleRotation, null, bodyPart != null ? bodyPart.tag : collision.collider.transform.tag);
            fx.transform.parent = collision.transform;
        }
        public void DoDamage(RaycastHit hit, Collision collision, float damage, List<SurfaceAudiosWithFX> hitParticles, AudioSource hitAudioSource, Collider[] CollidersToIgnore = null)
        {
            if (CollidersToIgnore != null)
            {
                foreach (Collider col in CollidersToIgnore)
                {
                    //Debug.Log(hit.collider.gameObject +" | "+ col.gameObject);
                    if (hit.collider.gameObject == col.gameObject) { return; }
                }
            }

            DamageableBodyPart bodyPart = (hit.point != Vector3.zero) ? hit.collider.gameObject.GetComponentInChildren<DamageableBodyPart>() : collision.gameObject.GetComponentInChildren<DamageableBodyPart>();
            Vector3 damagePoint = (hit.point != Vector3.zero) ? hit.point : collision.contacts[0].point;
            float realDamage = damage;
            if (bodyPart == null)
            {
                JUHealth health = (hit.point != Vector3.zero) ? hit.collider.gameObject.GetComponentInParent<JUHealth>() : collision.gameObject.GetComponentInParent<JUHealth>();
                if (health != null)
                {
                    //Do Damage
                    health.DoDamage(damage);

                    //Hit Marker
                    if (Owner != null)
                    {
                        if (health.IsDead == false && realDamage > 0) HitMarkerEffect.HitCheck(health.transform.tag, Owner.tag, damagePoint, realDamage);
                    }
                }
            }
            else
            {
                realDamage = bodyPart.DoDamage(damage);
                if (Owner != null)
                {
                    if (bodyPart.Health.IsDead == false && realDamage > 0) HitMarkerEffect.HitCheck(bodyPart.transform.tag, Owner.tag, damagePoint, realDamage);
                }
            }

            //Instantiate Particle FX
            Vector3 contactPoint = (hit.point != Vector3.zero) ? hit.point : collision.GetContact(0).point;
            Vector3 contactNormal = (hit.point != Vector3.zero) ? hit.normal : collision.GetContact(0).normal;
            Quaternion particleRotation = Quaternion.LookRotation(contactNormal);
            string tag = (hit.point != Vector3.zero) ? hit.transform.tag : collision.gameObject.tag;

            GameObject fx = SurfaceAudiosWithFX.Play(hitAudioSource, hitParticles, contactPoint, particleRotation, null, tag);
            fx.transform.parent = (hit.point != Vector3.zero) ? hit.collider.transform : collision.collider.transform;
        }
    }

}