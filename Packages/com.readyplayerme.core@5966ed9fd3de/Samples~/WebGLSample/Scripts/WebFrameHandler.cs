using System;
using Newtonsoft.Json;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Core.WebView;
using UnityEngine;

namespace ReadyPlayerMe.Samples.WebGLSample
{
    public enum AutoInitialize
    {
        OnStart,
        OnAwake,
        None
    }

    public class WebFrameHandler : MonoBehaviour
    {
        private string TAG = nameof(WebFrameHandler);
        public Action<string> OnAvatarExport;
        public Action<string> OnUserSet;
        public Action<string> OnUserUpdated;
        public Action<string> OnUserAuthorized;
        public Action onUserLogOut;
        public Action<AssetRecord> OnAssetUnlock;

        [SerializeField] private AutoInitialize autoInitialize = AutoInitialize.OnStart;
        [SerializeField] private UrlConfig urlConfig;

        private void Awake()
        {
            if (autoInitialize == AutoInitialize.OnAwake)
            {
                Setup();
            }
        }

        private void Start()
        {
            if (autoInitialize == AutoInitialize.OnStart)
            {
                Setup();
            }
        }

        public void Setup(string loginToken = "")
        {
            WebInterface.SetupRpmFrame(urlConfig.BuildUrl(loginToken), name);
        }

        /// <summary>
        /// This message is received from the RPM iFrame Javascript in the RPM WebGL Template.
        /// </summary>
        /// <param name="message">The message will contain data pass from the iFrame as JSON</param>
        // ReSharper disable once UnusedMember.Global
        public void FrameMessageReceived(string message)
        {
            var webMessage = JsonConvert.DeserializeObject<WebMessage>(message);
            switch (webMessage.eventName)
            {
                case WebViewEvents.AVATAR_EXPORT:
                    SDKLogger.Log(TAG, webMessage.eventName);
                    OnAvatarExport?.Invoke(webMessage.GetAvatarUrl());
                    WebInterface.SetIFrameVisibility(false);
                    break;
                case WebViewEvents.USER_SET:
                    SDKLogger.Log(TAG, webMessage.eventName);
                    OnUserSet?.Invoke(webMessage.GetUserId());
                    break;
                case WebViewEvents.USER_AUTHORIZED:
                    SDKLogger.Log(TAG, webMessage.eventName);
                    OnUserAuthorized?.Invoke(webMessage.GetUserId());
                    break;
                case WebViewEvents.USER_UPDATED:
                    SDKLogger.Log(TAG, webMessage.eventName);
                    OnUserUpdated?.Invoke(webMessage.GetUserId());
                    break;
                case WebViewEvents.USER_LOGOUT:
                    SDKLogger.Log(TAG, webMessage.eventName);
                    onUserLogOut?.Invoke();
                    break;
                case WebViewEvents.ASSET_UNLOCK:
                    OnAssetUnlock?.Invoke(webMessage.GetAssetRecord());
                    break;
            }
        }
    }
}
