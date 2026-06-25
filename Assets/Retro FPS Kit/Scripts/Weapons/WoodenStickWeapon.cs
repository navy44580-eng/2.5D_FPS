using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FPSRetroKit
{
    [RequireComponent(typeof(AudioSource))]
    public class WoodenStickWeapon : MonoBehaviour
    {
        [Header("Wooden Stick Settings")]
        [SerializeField] private string weaponName = "Wooden Stick";
        [SerializeField] private float attackDamage = 5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 0.8f;

        [Header("Prefabs and Sprites")]
        [SerializeField] private GameObject bloodSplat;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite attackSprite;

        [Header("Audio")]
        [SerializeField] private AudioClip attackSound;

        [Header("UI")]
        [SerializeField] private Text weaponText;

        private AudioSource _source;
        private SpriteRenderer _spriteRenderer;
        private bool _isAttacking;
        private float _nextAttackTime;

        public string WeaponName => weaponName;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            UpdateWeaponText();
        }

        private void Update()
        {
            UpdateWeaponText();

            if (Input.GetButtonDown("Fire1") && Time.time >= _nextAttackTime && !_isAttacking)
                _isAttacking = true;
        }

        private void FixedUpdate()
        {
            if (!_isAttacking || Time.time < _nextAttackTime)
                return;

            _isAttacking = false;
            _nextAttackTime = Time.time + attackCooldown;

            DynamicCrosshair.spread += DynamicCrosshair.PISTOL_SHOOTING_SPREAD;

            if (attackSound != null)
                _source.PlayOneShot(attackSound);

            StartCoroutine(PlayAttackAnimation());
            PerformMeleeAttack();
        }

        private void PerformMeleeAttack()
        {
            Transform cameraTransform = Camera.main.transform;
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, attackRange))
                return;

            hit.collider.gameObject.SendMessage("AddDamage", attackDamage, SendMessageOptions.DontRequireReceiver);

            if (!hit.transform.CompareTag("Enemy"))
                return;

            if (bloodSplat != null)
                Instantiate(bloodSplat, hit.point, Quaternion.identity);

            EnemyStates enemyStates = hit.collider.gameObject.GetComponent<EnemyStates>();
            if (enemyStates == null)
                return;

            if (enemyStates.currentState == enemyStates.patrolState ||
                enemyStates.currentState == enemyStates.alertState)
            {
                hit.collider.gameObject.SendMessage(
                    "HiddenShot",
                    transform.root.position,
                    SendMessageOptions.DontRequireReceiver);
            }
        }

        private IEnumerator PlayAttackAnimation()
        {
            if (_spriteRenderer == null || attackSprite == null || idleSprite == null)
                yield break;

            _spriteRenderer.sprite = attackSprite;
            yield return new WaitForSeconds(0.15f);
            _spriteRenderer.sprite = idleSprite;
        }

        private void UpdateWeaponText()
        {
            if (weaponText != null)
                weaponText.text = weaponName;
        }
    }
}
