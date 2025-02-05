﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.SlamUI
{
    /// <summary>
    /// User's guide for SLAM calibration, leads the user to a configurable set of movements that helps this process
    /// </summary>
    public class SlamGuide : BaseSlamGuide
    {
        /// <summary>
        /// Steps for the slam calibration.
        /// </summary>
        private enum CalibrationStep
        {
            RotateRight,
            RotateLeft,
            RotateCenter,
            RotateFurtherRight,
            RotateFurtherLeft
        }

        [Tooltip("Steps for the animation and the calibration flow")]
        [SerializeField]
        private CalibrationStep[] _calibrationSteps;

        [Tooltip("Slam UI controller")]
        [SerializeField]
        private BaseSlamUI _slamUI;

        [Tooltip("Head rotation object to provide feedback for the user")]
        [SerializeField]
        private ObjectRotation _headOrientation;

        [Tooltip("Angle that the user needs to rotate to each side while calibrating")]
        [SerializeField]
        private float _coverageAngle = 50;

        [Tooltip("Multiplier for the coverage angle used in the steps RotateFurtherRight and RotateFurtherLeft")]
        [SerializeField]
        private float _furtherAngleIncrease = 1.5f;

        /// <summary>
        /// Time out in seconds for SLAM to show fail message and restart mapping
        /// </summary>
        private const float SlamFailTimeOut = 30f;

        /// <summary>
        /// Time in seconds for the limit on Hold Your Head Still. 
        /// </summary>
        private const float SlamHoldStillMaxTime = 10f;

        private Coroutine _mainCoroutine;

        private Dictionary<CalibrationStep, float> _calibrationStepAngle;
        private CalibrationStage _currentCalibrationStage = CalibrationStage.WaitingForSensors;

        private SlamLocalizer _localizer;
        private Transform _centerEye;
        private Quaternion _initialYRotation;
        private bool _sensorsReady = false;        
        private float _angleDisparityThreshold = 25;

        private void Start()
        {
            _centerEye = metaContext.Get<IEventCamera>().EventCameraRef.transform;
            // using find object because MetaLocalizationSettings sets the localizer only on Start instead of Awake
            // _localizer = metaContext.Get<MetaLocalization>().GetLocalizer() as SlamLocalizer;
            _localizer = FindObjectOfType<SlamLocalizer>();

            // aligning UI with user's sight
            transform.position = _centerEye.position;
            transform.rotation = _centerEye.rotation;
            transform.SetParent(_centerEye);

            // TODO setup _initialYRotation to _centerEye.transform.rotation after compositor implements different origin
            _initialYRotation = Quaternion.identity;

            // setting up initial UI stage
            StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));

            _calibrationStepAngle = new Dictionary<CalibrationStep, float>()
            {
                { CalibrationStep.RotateCenter, ConvertToPositiveAngle(_initialYRotation.y) },
                { CalibrationStep.RotateRight, ConvertToPositiveAngle(_initialYRotation.y + _coverageAngle) },
                { CalibrationStep.RotateLeft, ConvertToPositiveAngle(_initialYRotation.y - _coverageAngle) },
                { CalibrationStep.RotateFurtherRight, ConvertToPositiveAngle(_initialYRotation.y + _coverageAngle * _furtherAngleIncrease) },
                { CalibrationStep.RotateFurtherLeft, ConvertToPositiveAngle(_initialYRotation.y - _coverageAngle * _furtherAngleIncrease) },
            };

            StartCoroutine(DetectSensorsReady());
        }

        /// <summary>
        /// Start tracking users steps to guide calibration
        /// </summary>
        /// <param name="initializationType"></param>
        public override void StartTrackCalibrationSteps(SlamInitializationType initializationType)
        {
            _mainCoroutine = StartCoroutine(TrackCalibrationSteps(initializationType));
        }

        private IEnumerator TrackCalibrationSteps(SlamInitializationType initializationType)
        {
            // waiting for the sensors
            while (!_sensorsReady)
            {
                yield return null;
            }

            // shows a message in case the user doesn't move in the beggining
            _currentCalibrationStage = CalibrationStage.WaitingForTracking;
            StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));

            if (initializationType == SlamInitializationType.NewMap)
            {
                _currentCalibrationStage = CalibrationStage.Mapping;
                // start timeout countdown
                StartCoroutine(TimeOutCalibrationFail(SlamFailTimeOut));
            }
            else
            {
                _headOrientation.gameObject.SetActive(false);
                _currentCalibrationStage = CalibrationStage.Remapping;
            }

            // show start message
            StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));

            int currentStep = 0;
            while (true)
            {
                float coverage = _coverageAngle;
                if (_calibrationSteps[currentStep] == CalibrationStep.RotateFurtherLeft ||
                   _calibrationSteps[currentStep] == CalibrationStep.RotateFurtherRight)
                {
                    coverage *= _furtherAngleIncrease;
                }

                float angle;
                // TODO change logic to consider _initialYOrientation after compositor implements different origin
                if (_centerEye.rotation.eulerAngles.y > 180)
                {
                    angle = Mathf.Clamp(_centerEye.rotation.eulerAngles.y - 360, -coverage, coverage);
                }
                else
                {
                    angle = Mathf.Clamp(_centerEye.rotation.eulerAngles.y, -coverage, coverage);
                }
                _headOrientation.Rotation = (coverage - angle) / (coverage * 2);

                // if calibration happens at any point
                if (DetectCalibratedStage())
                {
                    _currentCalibrationStage = CalibrationStage.Completed;
                    yield return StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));
                }
                // if hold still stage is detected at any point
                else if (DetectHoldStillStage() && _currentCalibrationStage != CalibrationStage.HoldStill)
                {
                    _currentCalibrationStage = CalibrationStage.HoldStill;
                    _headOrientation.gameObject.SetActive(false);
                    StartCoroutine(TimeOutCalibrationFail(SlamHoldStillMaxTime));
                    StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));
                }
                // if goal for the current step is achieved
                else if (ApproximatelyWithThreshold(_calibrationStepAngle[_calibrationSteps[currentStep]],
                    ConvertToPositiveAngle(_centerEye.rotation.eulerAngles.y)))
                {
                    if (currentStep + 1 < _calibrationSteps.Length)
                    {
                        currentStep++;
                        StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));
                    }
                    // steps finished
                    else
                    {
                        // if not initialized then fail
                        StartCoroutine(FailState());
                    }
                }
                yield return null;
            }           
        }



        private IEnumerator TimeOutCalibrationFail(float secondsToWait)
        {
            yield return new WaitForSeconds(secondsToWait);
            if (_currentCalibrationStage != CalibrationStage.Completed)
            {
                StartCoroutine(FailState());
            }
        }

        private IEnumerator FailState()
        {
            StopCoroutine(_mainCoroutine);
            _currentCalibrationStage = CalibrationStage.Fail;
            _localizer.onSlamInitializationFailed.Invoke();
            yield return StartCoroutine(_slamUI.ChangeUIStage(_currentCalibrationStage));
            metaContext.Get<MetaLocalization>().ResetLocalization();
        }

        private IEnumerator DetectSensorsReady()
        {
            if (_localizer != null && _localizer.SlamFeedback != null)
            {
                while (!(_localizer.SlamFeedback.CameraReady))
                {
                    yield return null;
                }
                _sensorsReady = true;
            }
        }

        private bool DetectHoldStillStage()
        {
            SlamFeedback feedback = _localizer.SlamFeedback;
            return feedback.TrackingReady && feedback.CameraReady && feedback.scale_quality_percent >= 100;
        }

        private bool DetectCalibratedStage()
        {
            return _localizer.SlamFeedback.FilterReady;
        }

        private bool ApproximatelyWithThreshold(float a, float b)
        {
            return Mathf.Abs(a - b) < _angleDisparityThreshold;
        }

        private float ConvertToPositiveAngle(float angle)
        {
            angle = (angle % 360);
            return (angle < 0) ? 360 + angle : angle;
        }
    }
}