using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Interactables
{
    [RequireComponent(typeof(Collider))]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ButtonInteractable : MonoBehaviour
    {
        [Header("Parameters")]
        [Header("Joint Configuration")] 
        [Tooltip("Direction in which the button should be pressed.")]
        [SerializeField] private Direction direction = Direction.Forward;
        private enum Direction
        {
            Up,
            Right,
            Forward
        }
        
        [Range(0.1f, 1f)]
        [SerializeField] private float distanceRequiredToPress = 0.75f;
        [Range(0.1f, 1f)]
        [SerializeField] private float distanceRequiredToRelease = 0.3f;
        
        [SerializeField] public bool isNegativeDirection = false;
        
        [SerializeField] private Vector3 directionOfPush = Vector3.down;

        [Tooltip("Length the button should be pressed to activate.")]
        [SerializeField] private float lengthOfPush = 0.1f;

        [SerializeField] private float speedOfPush = 2f;
        [SerializeField] private float speedOfRelease = 2f;
        
        [SerializeField] private bool pressed;

        [SerializeField] private bool oneShootPressed = true;
        [SerializeField] private bool oneShootReleased = true;
        [SerializeField] private bool saveInitialPosition = false;
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Vector3 maxPositionOnPressed;

        private ConfigurableJoint joint;
        [SerializeField] private bool hasBeenPressed = false;

        [Header("Callbacks")] public UnityEvent onPressed;
        public UnityEvent onRelease;

        #region PUBLIC METHODS

        public void OnPressed()
        {
            if (!hasBeenPressed && oneShootPressed)
            {
                Debug.Log("OnPressed in single shoot");
                hasBeenPressed = true;
                onPressed?.Invoke();
                return;
            }
            if(!oneShootPressed)
            {
                Debug.Log("OnPressed in multiple shoot");
                onPressed?.Invoke();
            }
        }

        public void OnRelease()
        {
            if (hasBeenPressed && oneShootReleased)
            {
                Debug.Log("OnRelease in single shoot");
                hasBeenPressed = false;
                onRelease?.Invoke();
                return;
            }
            if(!oneShootReleased)
            {
                Debug.Log("OnRelease in multiple shoot");
                onRelease?.Invoke();
            }
        }

        #endregion

        private void Awake()
        {
            switch (direction)
            {
                case Direction.Up:
                    directionOfPush = transform.InverseTransformDirection(transform.up);
                    break;
                case Direction.Right:
                    directionOfPush = transform.InverseTransformDirection(transform.right);
                    break;
                case Direction.Forward:
                    directionOfPush = transform.InverseTransformDirection(transform.forward);
                    break;
            }
            
            if (isNegativeDirection)
            {
                directionOfPush = -directionOfPush;
            }
        }
        
#if UNITY_EDITOR // for debug purposes
        private void OnEnable()
        {
            switch (direction)
            {
                case Direction.Up:
                    directionOfPush = transform.InverseTransformDirection(transform.up);
                    break;
                case Direction.Right:
                    directionOfPush = transform.InverseTransformDirection(transform.right);
                    break;
                case Direction.Forward:
                    directionOfPush = transform.InverseTransformDirection(transform.forward);
                    break;
            }
            
            if (isNegativeDirection)
            {
                directionOfPush = -directionOfPush;
            }
        }

        private void Update()
        {
            switch (direction)
            {
                case Direction.Up:
                    directionOfPush = transform.InverseTransformDirection(transform.up);
                    break;
                case Direction.Right:
                    directionOfPush = transform.InverseTransformDirection(transform.right);
                    break;
                case Direction.Forward:
                    directionOfPush = transform.InverseTransformDirection(transform.forward);
                    break;
            }
            
            if (isNegativeDirection)
            {
                directionOfPush = -directionOfPush;
            }
            
            if (saveInitialPosition)
            {
                initialPosition = transform.localPosition;
                maxPositionOnPressed = initialPosition + directionOfPush * lengthOfPush;
                saveInitialPosition = false;
            }
        }
#endif

        private void FixedUpdate()
        {
            maxPositionOnPressed = initialPosition + directionOfPush * lengthOfPush;
            float distanceMoved = Vector3.Distance(initialPosition, transform.position);
            if(distanceMoved >= lengthOfPush && pressed)
            {
                var distanceReached = Vector3.Distance(transform.position, maxPositionOnPressed);
                    if (distanceReached > 0.01f)
                    {
                        Vector3 newPosition = Vector3.MoveTowards(transform.localPosition,
                            initialPosition + directionOfPush * lengthOfPush, speedOfPush * Time.deltaTime);

                        transform.localPosition = newPosition;
                    }
                    
                    if (distanceReached >= lengthOfPush * (1 - distanceRequiredToPress))
                    {
                        pressed = true;
                        OnPressed();
                        return;
                    }
            }
            
            Vector3 returnPosition = Vector3.MoveTowards(transform.localPosition, initialPosition,
                speedOfRelease * Time.deltaTime);
            transform.localPosition = returnPosition;

            // conferm released only 30% of length is reached
            if (Vector3.Distance(transform.localPosition, initialPosition) <= lengthOfPush * (1 - distanceRequiredToRelease))
            {
                pressed = false;
                OnRelease();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!pressed)
            {
                pressed = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (pressed)
            {
                pressed = false;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if(transform.parent != null)
            {
                Gizmos.DrawLine(transform.parent.TransformPoint(initialPosition),
                    transform.parent.TransformPoint(maxPositionOnPressed));
                return;
            }
            
            Gizmos.DrawLine(transform.TransformPoint(initialPosition),
                transform.TransformPoint(maxPositionOnPressed));
        }
#endif
    }
}