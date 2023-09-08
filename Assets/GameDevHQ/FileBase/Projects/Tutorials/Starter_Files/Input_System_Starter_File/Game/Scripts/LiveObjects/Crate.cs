using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private PlayerInputAction _input;

        private bool _holdHasStarted;
        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
        }

        private void Punch_canceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (_holdHasStarted == true)
            {
                _holdHasStarted = false;
                BreakPart();
                StartCoroutine(PunchDelay());
            }

            Debug.Log("cancelled");
        }

        private void Punch_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            _holdHasStarted = false;
            BreakPart();
            BreakPart();
            BreakPart();
            StartCoroutine(PunchDelay());

            Debug.Log("hold performed");
        }

        private void Punch_started(UnityEngine.InputSystem.InputAction.CallbackContext ocontext)
        {
            _holdHasStarted = true;

            Debug.Log("Hold Started");
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    _input.Enable();
                    
                }
                else if(_brakeOff.Count <= 0)
                {
                    _holdHasStarted = false;
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void Start()
        {
            _input = new PlayerInputAction();

            _input.Crate.Punch.started += Punch_started;
            _input.Crate.Punch.performed += Punch_performed;
            _input.Crate.Punch.canceled += Punch_canceled;

            _brakeOff.AddRange(_pieces);
        }



        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
