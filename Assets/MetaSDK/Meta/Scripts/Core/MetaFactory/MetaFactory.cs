﻿using UnityEngine;

using Meta.Mouse;
using Meta.Buttons;
using Meta.Internal;
using Meta.Reconstruction;

namespace Meta
{
    internal class MetaFactory
    {
        private DataAcquisitionSystem _dataAcquisitionSystem;
        private GameObject _metaPrefab;
        private float _meterToUnityScale;
        private string _sensorPlaybackPath;
        private Transform _depthOcclusionTransform;
        private string _playbackPath;

        internal MetaFactory(DataAcquisitionSystem dataAcquisitionSystem, GameObject metaPrefab, float effectiveScale, string playbackPath = null, string sensorPlaybackPath = null)
        {
            _dataAcquisitionSystem = dataAcquisitionSystem;
            _metaPrefab = metaPrefab;
            _meterToUnityScale = effectiveScale;
            _playbackPath = playbackPath;
            _sensorPlaybackPath = sensorPlaybackPath;
            _depthOcclusionTransform = metaPrefab.transform.Find("MetaCameras/DepthOcclusion");
        }

        public MetaFactoryPackage ConstructAll()
        {
            var package = new MetaFactoryPackage();

            package.MetaContext.MeterToUnityScale = _meterToUnityScale;
            if (!usingPlayback)
            {
                ConstructSensors(package);
                ConstructButtonEventProvider(package);
                ConstructDepthOcclusion(package);
            }
            ConstructLocalization(package);
            ConstructDefaultInteractionEngine(package);
            ConstructHands(package);

            ConstructGaze(package);
            ConstructLocking(package);
            ConstructEnvironmentServices(package);
            ConstructUserSettings(package);
            ConstructMetaSdkAnalytics(package);
            ConstructCalibrationParameters(package);
            ConstructAlignmentHandler(package);
            ConstructInputWrapper(package);
            return package;
        }

        private void ConstructInputWrapper(MetaFactoryPackage package)
        {
            UnityInputWrapper inputWrapper = new UnityInputWrapper();
            UnityKeyboardWrapper keyboardWrapper = new UnityKeyboardWrapper();
            package.MetaContext.Add<IInputWrapper>(inputWrapper);
            package.MetaContext.Add<IKeyboardWrapper>(keyboardWrapper);
            //_eventReceivers.Add(inputWrapper);
        }

        /// <summary>
        /// Constructs the AlignmentHandler. This will load an 
        /// </summary>
        private void ConstructAlignmentHandler(MetaFactoryPackage package)
        {
            AlignmentHandler alignmentHandler = new AlignmentHandler();
            package.EventReceivers.Add(alignmentHandler);
            package.MetaContext.Add(alignmentHandler);
        }

        /// <summary>
        /// Constructs the calibration parameters object.
        /// No calibration data is guaranteed until the DLL which supplies the data does.
        /// </summary>
        private void ConstructCalibrationParameters(MetaFactoryPackage package)
        {
            CalibrationParameters pars = new CalibrationParameters(new CalibrationParameterLoaderAdditionalMatrices());
            package.MetaContext.Add(pars);
            package.EventReceivers.Add(pars);
        }

        private void ConstructMetaSdkAnalytics(MetaFactoryPackage package)   
        {
            MetaSdkAnalytics handler = new MetaSdkAnalytics();
            package.EventReceivers.Add(handler);
        }

        private void ConstructDepthOcclusion(MetaFactoryPackage package)
        {
            GameObject depthOcclusionGO = _metaPrefab.transform.Find("MetaCameras/DepthOcclusion/ShaderOcclusion").gameObject;
            if (depthOcclusionGO == null)
            {
                UnityEngine.Debug.LogWarning("Meshrenederer missing from depthOcclusion GameObject");
                return;
            }
            else if (depthOcclusionGO.GetComponent<DepthOcclusionManager>() == null)
            {
                UnityEngine.Debug.LogWarning("DepthOcclusionManager missing from depthOcclusion GameObject");
                return;
            }
            else if (depthOcclusionGO.GetComponent<Renderer>() == null)
            {
                UnityEngine.Debug.LogWarning("Renderer missing from depthOcclusion GameObject");
                return;
            }
            else if (depthOcclusionGO.GetComponent<Renderer>().material.shader.name != "Meta/DepthOcclusionShader")
            {
                UnityEngine.Debug.LogWarning("Renderer on depthOcclusion GameObject does not have the right shader set up");
                return;
            }

            var depthOcclusionHandler = new DepthOcclusionHandler(depthOcclusionGO);
            //Hack; becuase Cant run Coroutines outside of MonoBehvaiour. 
            //todo: Maybe should think of using a MetaBehaviour Script to get around this. 
            depthOcclusionGO.GetComponent<DepthOcclusionManager>().depthOcclusionHandler = depthOcclusionHandler;
            package.EventReceivers.Add(depthOcclusionHandler);

            // Add to context
            package.MetaContext.Add(depthOcclusionHandler);
        }

        private void ConstructUserSettings(MetaFactoryPackage package)
        {
            //This will be how the username is passed around
            Credentials creds = new Credentials("default", null);
            package.MetaContext.Add(creds);

            var userSettings = new EventReceivingUserSettings(creds);
            package.EventReceivers.Add(userSettings);
            package.MetaContext.Add((IUserSettings)userSettings);
        }

        private void ConstructDefaultInteractionEngine(MetaFactoryPackage package)
        {
            //todo: redundant and conflicting options possible. Needs to be refactored

            HandKernelSettings handSettignsGO = GameObject.FindObjectOfType<HandKernelSettings>();
            InteractionEngine interactionEngine = null;
            string handkernelType = handSettignsGO.handKernelType.ToString();
            if (!usingPlayback)
            {
                InteractionEngineFactory.Construct(out interactionEngine, handkernelType, "Sensors", _depthOcclusionTransform);
            }
            else
            {
                InteractionEngineFactory.Construct(out interactionEngine, handkernelType, "Playback", _depthOcclusionTransform, _playbackPath);
            }
            package.EventReceivers.Add(interactionEngine);

            // Add to context
            package.MetaContext.Add(interactionEngine);
        }


        private void ConstructSensors(MetaFactoryPackage package)
        {
            var deviceInfo = new DeviceInfo();
            deviceInfo.imuModel = IMUModel.MPU9150Serial;
            deviceInfo.cameraModel = CameraModel.DS325;
            deviceInfo.depthFps = 60;
            deviceInfo.depthHeight = 240;
            deviceInfo.depthWidth = 320;
            deviceInfo.colorFps = 30;
            deviceInfo.colorHeight = 720;
            deviceInfo.colorWidth = 1280;

            var metaSensors = new MetaSensors(deviceInfo, _dataAcquisitionSystem, _sensorPlaybackPath);
            package.EventReceivers.Add(metaSensors);

            //Sensor Messages
            MetaSensorFailureMessages messages = new MetaSensorFailureMessages();
            package.EventReceivers.Add(messages);
            package.MetaContext.Add(messages);
        }

        private void ConstructButtonEventProvider(MetaFactoryPackage package)
        {
            var provider = new MetaButtonEventProvider();
            package.EventReceivers.Add(provider);
            package.MetaContext.Add<IMetaButtonEventProvider>(provider);
        }

        private void ConstructLocalization(MetaFactoryPackage package)
        {
            var metaLocalization = new MetaLocalization(_metaPrefab);
            package.EventReceivers.Add(metaLocalization);
            package.MetaContext.Add(metaLocalization);

            // Set localizer if no Localization Settings to set localizer
            var settings = _metaPrefab.GetComponent<MetaLocalizationSettings>();
            if (settings == null)
            {
                metaLocalization.SetLocalizer(typeof(SlamLocalizer));
            }
        }

        private void ConstructGaze(MetaFactoryPackage package)
        {
            var gaze = new Gaze();
            package.EventReceivers.Add(gaze);
            package.MetaContext.Add(gaze);
        }

        private void ConstructLocking(MetaFactoryPackage package)
        {
            var hudLock = new HudLock();
            var orbitalLock = new OrbitalLock();
            package.EventReceivers.Add(hudLock);
            package.EventReceivers.Add(orbitalLock);

            // Add to context
            package.MetaContext.Add(hudLock);
            package.MetaContext.Add(orbitalLock);
        }

        private void ConstructHands(MetaFactoryPackage package)
        {
            var kernelCocoLauncher = new KernelCocoLauncherModule();
            package.EventReceivers.Add(kernelCocoLauncher);
            package.MetaContext.Add(kernelCocoLauncher);


            var handsModule = new HandsModule(_depthOcclusionTransform);
            package.EventReceivers.Add(handsModule);
            package.MetaContext.Add(handsModule);
            HandObjectReferences references = new HandObjectReferences();
            package.MetaContext.Add(references);
            InteractionObjectOutlineFactory outlineFactory = new InteractionObjectOutlineFactory();
            outlineFactory.SubscribeToHandObjectReferences(references);
            package.MetaContext.Add(outlineFactory);
        }

        private void ConstructEnvironmentServices(MetaFactoryPackage package)
        {
            string envPath = string.Format("{0}\\{1}\\", System.Environment.GetEnvironmentVariable("meta_root"), EnvironmentConstants.EnvironmentFolderName);
            IEnvironmentProfileRepository profileRepository = new EnvironmentProfileRepository(new EnvironmentProfileFileIOStream(envPath + "EnvironmentProfiles.json"),
                                                                                               new EnvironmentProfileJsonParser(),
                                                                                               new EnvironmentProfileVerifier(), 
                                                                                               envPath);
            package.MetaContext.Add(profileRepository);          
        }

        private bool usingPlayback
        {
            get { return !string.IsNullOrEmpty(_playbackPath); }
        }
    }
}
