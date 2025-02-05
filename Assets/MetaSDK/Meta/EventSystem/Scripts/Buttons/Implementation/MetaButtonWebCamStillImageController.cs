﻿using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Meta.Buttons
{
    /// <summary>
    /// Class in charge of capturing still images from the Virtual Webcamm
    /// </summary>
    public class MetaButtonWebCamStillImageController : MonoBehaviour, IOnMetaButtonEvent
    {
        private const string WEBCAM_NAME = "Meta 2 Webcam";

        [SerializeField]
        [Tooltip("(Optional) Target where to render the Webcam feed")]
        private Renderer _targetRenderer;
        private WebCamTexture _webcamTexture;
        private Texture2D _texture2d;
        private bool _isProcessing;
        private object _stateObject = new object();
        private FileStream _writer;
        private bool _initializing;

        /// <summary>
        /// Initialize the WebCam
        /// </summary>
        private IEnumerator Start()
        {
            _initializing = true;
            if (!CreateWebCamTexture())
                yield break;

            _webcamTexture.Play();
            yield return new WaitForSeconds(3);
            _webcamTexture.Pause();
            _initializing = false;
        }

        /// <summary>
        /// Process the Meta Button Event
        /// </summary>
        /// <param name="button">Button Message</param>
        public void OnMetaButtonEvent(IMetaButton button)
        {
            if (button.Type != ButtonType.ButtonCamera)
                return;
            if (button.State != ButtonState.ButtonShortPress)
                return;
            if (!this.enabled)
            {
                Debug.LogWarning("Script is not enabled");
                return;
            }
            if (!CreateWebCamTexture())
            {
                Debug.LogWarning("Could not create Webcam Texture");
                return;
            }
            if (_isProcessing)
            {
                Debug.LogWarning("Still processing data");
                return;
            }

            StartCoroutine(TakeStill());
        }

        /// <summary>
        /// Create the Webcam texture and the Texture2D for encoding
        /// </summary>
        /// <returns>True if the objects were created, false otherwise</returns>
        private bool CreateWebCamTexture()
        {
            if (_webcamTexture != null)
            {
                return true;
            }

            var devices = WebCamTexture.devices;
            bool webcamFound = false;
            for (int i = 0; i < devices.Length; ++i)
            {
                if (devices[i].name.Contains(WEBCAM_NAME))
                {
                    webcamFound = true;
                    break;
                }
            }

            if (!webcamFound)
            {
                Debug.LogWarning("Meta2 Webcam not found. Please make sure you have the Meta2 Virtual Webcam Drivers installed");
                return false;
            }

            var targetWidth = 1280;
            var targetHeight = 720;
            _webcamTexture = new WebCamTexture(WEBCAM_NAME, targetWidth, targetHeight, 30);
            if (_targetRenderer != null)
            {
                _targetRenderer.material.mainTexture = _webcamTexture;
            }
            _texture2d = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
            return true;
        }

        /// <summary>
        /// Take the screenshot
        /// </summary>
        private IEnumerator TakeStill()
        {
            // Wait for initialization
            while (_initializing)
                yield return null;

            _isProcessing = true;
            //*******************
            // Get a frame
            // Hit play, if it's the first time we capture, we need to wait a bit to get the full picture
            // Once the texture is ready we pause the webcam
            _webcamTexture.Play();
            yield return new WaitForSeconds(0.5f);
            _webcamTexture.Pause();

            // Transfer data to the texture 2D for encoding
            var webcamPixels = _webcamTexture.GetPixels();
            yield return null;
            _texture2d.SetPixels(webcamPixels);
            yield return null;
            _texture2d.Apply();
            yield return null;

            // Encode: This part is heavy
            var bytes = _texture2d.EncodeToPNG();
            yield return null;

            // Save to file
            _writer = File.Create(GetTargetFileName());
            _writer.BeginWrite(bytes, 0, bytes.Length, OnFileWriteFinished, _stateObject);
        }

        /// <summary>
        /// Called when we finished writing the image to a file.
        /// </summary>
        private void OnFileWriteFinished(IAsyncResult result)
        {
            _writer.EndWrite(result);
            _writer.Dispose();
            _writer = null;
            _isProcessing = false;
        }

        /// <summary>
        /// Make sure we stop the Webcam when the script is disabled
        /// </summary>
        private void OnDisable()
        {
            if (_webcamTexture == null)
            {
                return;
            }
            _webcamTexture.Stop();
        }

        private string GetTargetFileName()
        {
            // Get root folder
            var root = Environment.GetEnvironmentVariable("META_ROOT", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(root))
            {
                root = "";
            }

            // Get Subfolder
            var path = Path.Combine(root, "Screenshots");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Get filename
            var fileName = string.Format("Meta Screen Shot {0:yyyy-MM-dd} at {0:HH.mm.ss} PM.png", DateTime.Now);
            return Path.Combine(path, fileName);
        }
    }
}
