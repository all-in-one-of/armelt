﻿ using UnityEngine;
using System.Collections.Generic;
using System;

namespace Meta.HandInput
{
    /// <summary>
    /// Will gather Meta HandFeature GameObjects when they enter and exit the Trigger on this GameObject.
    /// Use whenever you need to determine the entry and exit of the HandFeature into a particular area.
    /// </summary>
    public class HandTrigger : MonoBehaviour
    {
        [SerializeField]
        private HandTriggerEvent _twoHandEnterEvent = new HandTriggerEvent();

        [SerializeField]
        private HandFeatureEvent _handFeatureEnterEvent = new HandFeatureEvent();

        [SerializeField]
        private HandFeatureEvent _firstHandFeatureEnterEvent = new HandFeatureEvent();

        [SerializeField]
        private HandTriggerEvent _twoHandExitEvent = new HandTriggerEvent();

        [SerializeField]
        private HandFeatureEvent _handFeatureExitEvent = new HandFeatureEvent();

        [SerializeField]
        private HandFeatureEvent _lastHandFeatureExitEvent = new HandFeatureEvent();

        /*TODO Write UI for this to not need these strings typed in
        "Meta.HandInput.TopHandFeature, Assembly-CSharp",
        "Meta.HandInput.CenterHandFeature, Assembly-CSharp",
        "Meta.HandInput.LinkHandFeature, Assembly-CSharp",
        */
        [Header("An empty list allows all Types")]
        [SerializeField]
        HandFeatureType _allowedFeatureType = HandFeatureType.Any;
        
        [SerializeField]
        private Vector3 _expandOnEntry = new Vector3(1.1f, 1.1f, 1.1f);

        [SerializeField]
        private bool _showCursor = true;

        [SerializeField]
        private bool _showCursorTwoHands = true;

        private readonly List<HandFeature> _handFeatureList = new List<HandFeature>();
        private bool _twoHandsEntered;
        private Vector3 _initialScale;

        public bool ShowCursor
        {
            get { return _showCursor || (HandCount == 2 && _showCursorTwoHands); }
        }

        /// <summary>
        /// Number of unique HandTypes (left, right) in Trigger.
        /// </summary>
        public int HandCount
        {
            get
            {
                return _handFeatureList.Count;
            }
        }

        /// <summary>
        /// Called first time two HandFeatures on different hands enter HandVolume.
        /// </summary>
        public HandTriggerEvent TwoHandEnterEvent
        {
            get { return _twoHandEnterEvent; }
        }

        /// <summary>
        /// Called when any HandFeature enters HandVolume.
        /// </summary>
        public HandFeatureEvent HandFeatureEnterEvent
        {
            get { return _handFeatureEnterEvent; }
        }

        /// <summary>
        /// Called when first HandFeature enters HandVolume.
        /// </summary>
        public HandFeatureEvent FirstHandFeatureEnterEvent
        {
            get { return _firstHandFeatureEnterEvent; }
        }

        /// <summary>
        /// Called when two HandFeatures from different hands are no longer in HandVolume.
        /// </summary>
        public HandTriggerEvent TwoHandExitEvent
        {
            get { return _twoHandExitEvent; }
        }

        /// <summary>
        /// Called when any HandFeature exists HandVolume.
        /// </summary>
        public HandFeatureEvent HandFeatureExitEvent
        {
            get { return _handFeatureExitEvent; }
        }

        /// <summary>
        /// Called when all HandFeatures have exited HandVolume.
        /// </summary>
        public HandFeatureEvent LastHandFeatureExitEvent
        {
            get { return _lastHandFeatureExitEvent; }
        }

        /// <summary>
        /// All HandFeatures currently in volume.
        /// </summary>
        public List<HandFeature> HandFeatureList
        {
            get { return _handFeatureList; }
        }

        private void Awake()
        {
            HandUtil.SetupCollider(gameObject);
            _initialScale = transform.localScale;
        }
        
        private void Update()
        {
            for (int i = _handFeatureList.Count - 1; i > -1; --i)
            {
                if (_handFeatureList[i] == null)
                {
                    _handFeatureList.RemoveAt(i);
                    OnHandExit(null);
                }
            }
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            HandFeature handFeature = collider.GetComponent<HandFeature>();

            if (handFeature != null && IsAllowedType(handFeature))
            {
                if (_firstHandFeatureEnterEvent != null && _handFeatureList.Count == 0)
                {
                    transform.localScale = Vector3.Scale(transform.localScale, _expandOnEntry);
                    _firstHandFeatureEnterEvent.Invoke(handFeature);
                }
                if (_handFeatureEnterEvent != null)
                {
                    _handFeatureEnterEvent.Invoke(handFeature);
                }

                _handFeatureList.Add(handFeature);

                if (!_twoHandsEntered && HandCount > 1)
                {
                    _twoHandsEntered = true;
                    if (_twoHandEnterEvent != null)
                    {
                        _twoHandEnterEvent.Invoke(this);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            HandFeature handFeature = collider.GetComponent<HandFeature>();

            if (handFeature != null && IsAllowedType(handFeature))
            {
                OnHandExit(handFeature);
            }
        }

        /// <summary>
        /// Determines if HandTrigger contains hand.
        /// </summary>
        /// <param name="handFeature"> HandFeature to find. </param>
        /// <returns> HandFeature if found, otherwise null. </returns>
        public bool ContainsHand(HandFeature handFeature)
        {
            for (int i = 0; i < _handFeatureList.Count; ++i)
            {
                if (_handFeatureList[i] == handFeature)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnHandExit(HandFeature handFeature)
        {
            if (handFeature != null)
            {
                _handFeatureList.Remove(handFeature);
            }

            if (_lastHandFeatureExitEvent != null && HandCount == 0)
            {
                transform.localScale = _initialScale;
                _lastHandFeatureExitEvent.Invoke(handFeature);
            }

            if (_handFeatureExitEvent != null)
            {
                _handFeatureExitEvent.Invoke(handFeature);
            }

            if (_twoHandsEntered && HandCount < 2)
            {
                _twoHandsEntered = false;
                if (_twoHandExitEvent != null)
                {
                    _twoHandExitEvent.Invoke(this);
                }
            }
        }

        private bool IsAllowedType(HandFeature handFeature)
        {
            switch (_allowedFeatureType)
            {
                case HandFeatureType.Any:
                    return true;
                case HandFeatureType.PalmFeature:
                    return handFeature is CenterHandFeature;
                case HandFeatureType.TopFeature:
                    return handFeature is TopHandFeature;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void OnDrawGizmos()
        {
            //Enforce proper setup in editor
            HandUtil.SetupCollider(gameObject);
        }


    }
}