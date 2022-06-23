﻿using System;
using System.Globalization;
using UnityEngine;

namespace MirageXR
{
    public class PrefsStringValue
    {
        private readonly string _defaultValue;
        private readonly string _prefsKey;
        private string _value;
        
        public string Value
        {
            get => _value ?? PlayerPrefs.GetString(_prefsKey, _defaultValue);
            set
            {
                _value = value;
                PlayerPrefs.SetString(_prefsKey, value);
                PlayerPrefs.Save();
            }
        }

        public PrefsStringValue(string prefsKey, string defaultValue)
        {
            _prefsKey = prefsKey;
            _defaultValue = defaultValue;
        } 
    }
    
    public class PrefsBoolValue
    {
        private readonly bool _defaultValue;
        private readonly string _prefsKey;
        private bool? _value;
        
        public bool Value
        {
            get => _value ?? PlayerPrefs.GetInt(_prefsKey, _defaultValue ? 1 : 0) == 1;
            set
            {
                _value = value;
                PlayerPrefs.SetInt(_prefsKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public PrefsBoolValue(string prefsKey, bool defaultValue)
        {
            _prefsKey = prefsKey;
            _defaultValue = defaultValue;
        } 
    }
    
    public static class DBManager // TODO: rename to Settings
    {
        private const string UPLOAD_PRIVACY_KEY = "UploadPrivacy";
        public const bool PUBLIC_UPLOAD_PRIVACY_DEFAULT = false;
        
        private const string REMEMBER_USER_KEY = "RememberUser";
        private const bool REMEMBER_USER_DEFAULT = false;
        
        private const string REMEMBER_SKETCHFAB_USER_KEY = "RememberSketchfabUser";
        private const bool REMEMBER_SKETCHFAB_USER_DEFAULT = false;
        
        private const string DONT_SHOW_CALIBRATION_GUIDE_KEY = "DontShowCalibrationGuide";
        private const bool DONT_SHOW_CALIBRATION_GUIDE_DEFAULT = false;
        
        private const string MOODLE_URL_KEY = "MoodleURL";
        public const string MOODLE_URL_DEFAULT = "https://learn.wekit-ecs.com";
        
        private const string SKETCHFAB_TOKEN_RENEW_KEY = "Sketchfab_token_last_renew_date";
        private const string SKETCHFAB_TOKEN_RENEW_DEFAULT = "";

        private const int SKETCHFAB_RENEW_DYAS_COOLDOWN = 5;
        
        public static string plugin = "arete";

        public static string username;
        public static string userid;
        public static string usermail;
        public static string token;

        private static int CurrentLearningRecordStore;

        private static readonly PrefsBoolValue _publicUploadPrivacy = new PrefsBoolValue(UPLOAD_PRIVACY_KEY, PUBLIC_UPLOAD_PRIVACY_DEFAULT);
        private static readonly PrefsBoolValue _rememberUser = new PrefsBoolValue(REMEMBER_USER_KEY, REMEMBER_USER_DEFAULT);
        private static readonly PrefsBoolValue _rememberSketchfabUser = new PrefsBoolValue(REMEMBER_SKETCHFAB_USER_KEY, REMEMBER_SKETCHFAB_USER_DEFAULT);
        private static readonly PrefsBoolValue _dontShowCalibrationGuide = new PrefsBoolValue(DONT_SHOW_CALIBRATION_GUIDE_KEY, DONT_SHOW_CALIBRATION_GUIDE_DEFAULT);
        private static readonly PrefsStringValue _domain = new PrefsStringValue(MOODLE_URL_KEY, MOODLE_URL_DEFAULT);
        private static readonly PrefsStringValue _sketchfabTokenRenewDate = new PrefsStringValue(SKETCHFAB_TOKEN_RENEW_KEY, SKETCHFAB_TOKEN_RENEW_DEFAULT);

        public static bool isNeedToRenewSketchfabToken => sketchfabLastTokenRenewDate <= DateTime.Now.AddDays(-SKETCHFAB_RENEW_DYAS_COOLDOWN);
        
        public static DateTime sketchfabLastTokenRenewDate
        {
            get
            {
                var result = DateTime.TryParseExact(_sketchfabTokenRenewDate.Value, "d", new CultureInfo("en-US"), DateTimeStyles.None, out var dateTime);
                return result ? dateTime : default;
            }
            set => _sketchfabTokenRenewDate.Value = value.ToString("d", new CultureInfo("en-US"));
        }

        public static bool publicUploadPrivacy
        {
            get => _publicUploadPrivacy.Value;
            set => _publicUploadPrivacy.Value = value;
        }

        public static int publicCurrentLearningRecordStore
        {
            get => CurrentLearningRecordStore;
            set => CurrentLearningRecordStore = value;
        }

        public static bool rememberUser
        {
            get => _rememberUser.Value;
            set => _rememberUser.Value = value;
        }
        
        public static bool rememberSketchfabUser
        {
            get => _rememberSketchfabUser.Value;
            set => _rememberSketchfabUser.Value = value;
        }
        
        public static bool dontShowCalibrationGuide
        {
            get => _dontShowCalibrationGuide.Value;
            set => _dontShowCalibrationGuide.Value = value;
        }
        
        public static string domain
        {
            get => _domain.Value;
            set => _domain.Value = value;
        }
        
        public static string registerPage => $"{domain}/login/signup.php";

        public static string deleteAccount => $"{domain}/admin/tool/dataprivacy/createdatarequest.php?type=2";

        // if user is logged into Moodle
        public static bool LoggedIn => username != null;

        public static void LogOut()
        {
            username = null;
            token = null;
            userid = null;
            usermail = null;
        }

    }
}