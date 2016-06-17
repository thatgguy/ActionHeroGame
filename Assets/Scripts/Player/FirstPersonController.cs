using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
		[SerializeField] private bool isWalking;
		[SerializeField] private float walkSpeed;
		[SerializeField] private float runSpeed;
		[SerializeField] [Range(0f, 1f)] private float runStepLengthen;
		[SerializeField] private float jumpSpeed;
		[SerializeField] private float stickToGroundForce;
		[SerializeField] private float gravityMultiplier;
		[SerializeField] private MouseLook mouseLook;
		[SerializeField] private bool useFovKick;
		[SerializeField] private FOVKick fovKick = new FOVKick();
		[SerializeField] private bool useHeadBob;
		[SerializeField] private CurveControlledBob headBob = new CurveControlledBob();
		[SerializeField] private LerpControlledBob jumpBob = new LerpControlledBob();
		[SerializeField] private float stepInterval;
		[SerializeField] private AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
		[SerializeField] private AudioClip jumpSound;           // the sound played when character leaves the ground.
		[SerializeField] private AudioClip landSound;           // the sound played when character touches back on ground.

		private Camera camera;
		private bool jump;
		private float yRotation;
		private Vector2 input;
		private Vector3 moveDir = Vector3.zero;
		private CharacterController characterController;
		private CollisionFlags collisionFlags;
		private bool previouslyGrounded;
		private Vector3 originalCameraPosition;
		private float stepCycle;
		private float nextStep;
		private bool jumping;
		private AudioSource audioSource;

        // Use this for initialization
        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            camera = Camera.main;
            originalCameraPosition = camera.transform.localPosition;
            fovKick.Setup(camera);
            headBob.Setup(camera, stepInterval);
            stepCycle = 0f;
            nextStep = stepCycle/2f;
            jumping = false;
            audioSource = GetComponent<AudioSource>();
			mouseLook.Init(transform , camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!jump)
            {
				jump = Input.GetButtonDown("Jump");
            }

            if (!previouslyGrounded && characterController.isGrounded)
            {
                StartCoroutine(jumpBob.DoBobCycle());
                PlayLandingSound();
                moveDir.y = 0f;
                jumping = false;
            }
            if (!characterController.isGrounded && !jumping && previouslyGrounded)
            {
                moveDir.y = 0f;
            }

            previouslyGrounded = characterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            audioSource.clip = landSound;
            audioSource.Play();
            nextStep = stepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*input.y + transform.right*input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                               characterController.height/2f, ~0, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            moveDir.x = desiredMove.x*speed;
            moveDir.z = desiredMove.z*speed;


            if (characterController.isGrounded)
            {
                moveDir.y = -stickToGroundForce;

                if (jump)
                {
                    moveDir.y = jumpSpeed;
                    PlayJumpSound();
                    jump = false;
                    jumping = true;
                }
            }
            else
            {
                moveDir += Physics.gravity*gravityMultiplier*Time.fixedDeltaTime;
            }
            collisionFlags = characterController.Move(moveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            mouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            audioSource.clip = jumpSound;
            audioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (characterController.velocity.sqrMagnitude > 0 && (input.x != 0 || input.y != 0))
            {
                stepCycle += (characterController.velocity.magnitude + (speed*(isWalking ? 1f : runStepLengthen)))*
                             Time.fixedDeltaTime;
            }

            if (!(stepCycle > nextStep))
            {
                return;
            }

            nextStep = stepCycle + stepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, footstepSounds.Length);
            audioSource.clip = footstepSounds[n];
            audioSource.PlayOneShot(audioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            footstepSounds[n] = footstepSounds[0];
            footstepSounds[0] = audioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!useHeadBob)
            {
                return;
            }
            if (characterController.velocity.magnitude > 0 && characterController.isGrounded)
            {
                camera.transform.localPosition =
                    headBob.DoHeadBob(characterController.velocity.magnitude +
                                      (speed*(isWalking ? 1f : runStepLengthen)));
                newCameraPosition = camera.transform.localPosition;
                newCameraPosition.y = camera.transform.localPosition.y - jumpBob.Offset();
            }
            else
            {
                newCameraPosition = camera.transform.localPosition;
                newCameraPosition.y = originalCameraPosition.y - jumpBob.Offset();
            }
            camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
			float horizontal = Input.GetAxis("Horizontal");
			float vertical = Input.GetAxis("Vertical");

            bool waswalking = isWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            isWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = isWalking ? walkSpeed : runSpeed;
            input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (input.sqrMagnitude > 1)
            {
                input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (isWalking != waswalking && useFovKick && characterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!isWalking ? fovKick.FOVKickUp() : fovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            mouseLook.LookRotation (transform, camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(characterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
