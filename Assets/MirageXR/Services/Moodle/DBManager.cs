﻿using System;
using System.Globalization;
using UnityEngine;

namespace MirageXR
{
    public class PrefsFloatValue
    {
        private readonly float _defaultValue;
        private readonly string _prefsKey;
        private float? _value;

        public float Value
        {
            get => _value ?? PlayerPrefs.GetFloat(_prefsKey, _defaultValue);
            set
            {
                _value = value;
                PlayerPrefs.SetFloat(_prefsKey, value);
                PlayerPrefs.Save();
            }
        }

        public PrefsFloatValue(string prefsKey, float defaultValue)
        {
            _prefsKey = prefsKey;
            _defaultValue = defaultValue;
        }
    }

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

        private const string DEVELOP_MODE_KEY = "DevelopMode";
        private const bool DEVELOP_MODE_DEFAULT = false;

        private const string REMEMBER_SKETCHFAB_USER_KEY = "RememberSketchfabUser";
        private const bool REMEMBER_SKETCHFAB_USER_DEFAULT = false;

        private const string SHOW_BIG_CARDS_KEY = "ShowBigCards";
        public const bool SHOW_BIG_CARDS_DEFAULT = false;

        private const string DONT_SHOW_CALIBRATION_GUIDE_KEY = "DontShowCalibrationGuide";
        private const bool DONT_SHOW_CALIBRATION_GUIDE_DEFAULT = false;

        private const string DONT_SHOW_NEW_AUGMENTATION_HINT_KEY = "DontShowNewAugmentationHint";
        private const bool DONT_SHOW_NEW_AUGMENTATION_HINT_DEFAULT = false;

        private const string MOODLE_URL_KEY = "MoodleURL";
        //public const string MOODLE_URL_DEFAULT = "https://learn.wekit-ecs.com";
        public const string WEKIT_URL = "https://learn.wekit-ecs.com";
        public const string ARETE_URL = "https://arete.ucd.ie";

        private const string PRIVACY_POLICY_URL_KEY = "PrivacyPolicyURL";
        public const string WEKIT_PRIVACY_POLICY_URL = "https://wekit-ecs.com/privacy-policy";
        public const string ARETE_PRIVACY_POLICY_URL = "https://www.areteproject.eu/gdprpolicy/";

        private const string SKETCHFAB_TOKEN_RENEW_KEY = "Sketchfab_token_last_renew_date";
        private const string SKETCHFAB_TOKEN_RENEW_DEFAULT = "";

        private const string LOCAL_SAVE = "LocalSave";
        private const bool LOCAL_SAVE_DEFAULT = true;

        private const string CLOUD_SAVE = "CloudSave";
        private const bool CLOUD_SAVE_DEFAULT = false;

        private const string SHOW_PUBLIC_UPLOAD_WARNING = "DontShowPublicUploadWarning";
        private const bool SHOW_PUBLIC_UPLOAD_WARNING_DEFAULT = true;

        private const string SHOW_GRID_KEY = "show_grid";
        private const bool SHOW_GRID_DEFAULT = true;

        private const string SNAP_TO_GRID_KEY = "snap_to_grid";
        private const bool SNAP_TO_GRID_DEFAULT = true;

        private const string GRID_CELL_WIDTH_KEY = "grid_cell_width";
        private const float GRID_CELL_WIDTH_DEFAULT = 10f;

        private const string GRID_ANGLE_STEP_KEY = "grid_angle_step";
        private const float GRID_ANGLE_STEP_DEFAULT = 10f;

        private const string GRID_SCALE_STEP_KEY = "grid_scale_step";
        private const float GRID_SCALE_STEP_DEFAULT = 10f;

        private const string GRID_SHOW_ORIGINAL_OBJECT_KEY = "grid_show_original_object";
        private const bool GRID_SHOW_ORIGINAL_OBJECT_DEFAULT = false;

        private const string GRID_USE_OBJECT_CENTER_KEY = "grid_use_object_center_key";
        private const bool GRID_USE_OBJECT_CENTER_DEFAULT = false;

        private const int SKETCHFAB_RENEW_DYAS_COOLDOWN = 5;

        public static string plugin = "arete";

        public static string username;
        public static string userid;
        public static string usermail;
        public static string token;

        public enum LearningRecordStores {WEKIT};

        private static LearningRecordStores CurrentLearningRecordStore;

        public enum ShowBy {ALL, MYASSIGNMENTS, MYACTIVITIES};

        private static ShowBy CurrentShowby = ShowBy.ALL;

        public enum SortBy {DATE, RELEVEANCE};

        private static SortBy CurrentSortby = SortBy.DATE;

        private static readonly PrefsBoolValue _publicUploadPrivacy = new (UPLOAD_PRIVACY_KEY, PUBLIC_UPLOAD_PRIVACY_DEFAULT);
        private static readonly PrefsBoolValue _rememberUser = new (REMEMBER_USER_KEY, REMEMBER_USER_DEFAULT);
        private static readonly PrefsBoolValue _developMode = new (DEVELOP_MODE_KEY, DEVELOP_MODE_DEFAULT);
        private static readonly PrefsBoolValue _rememberSketchfabUser = new (REMEMBER_SKETCHFAB_USER_KEY, REMEMBER_SKETCHFAB_USER_DEFAULT);
        private static readonly PrefsBoolValue _dontShowCalibrationGuide = new (DONT_SHOW_CALIBRATION_GUIDE_KEY, DONT_SHOW_CALIBRATION_GUIDE_DEFAULT);
        private static readonly PrefsBoolValue _dontShowNewAugmentationHint = new (DONT_SHOW_NEW_AUGMENTATION_HINT_KEY, DONT_SHOW_NEW_AUGMENTATION_HINT_DEFAULT);
        private static readonly PrefsStringValue _domain = new (MOODLE_URL_KEY, WEKIT_URL);
        private static readonly PrefsStringValue _privacyPolicyDomain = new (PRIVACY_POLICY_URL_KEY, WEKIT_PRIVACY_POLICY_URL);
        private static readonly PrefsStringValue _sketchfabTokenRenewDate = new (SKETCHFAB_TOKEN_RENEW_KEY, SKETCHFAB_TOKEN_RENEW_DEFAULT);
        private static readonly PrefsBoolValue _showBigCards = new (SHOW_BIG_CARDS_KEY, SHOW_BIG_CARDS_DEFAULT);
        private static readonly PrefsBoolValue _showGrid = new (SHOW_GRID_KEY, SHOW_GRID_DEFAULT);
        private static readonly PrefsBoolValue _snapToGrid = new (SNAP_TO_GRID_KEY, SNAP_TO_GRID_DEFAULT);
        private static readonly PrefsFloatValue _gridCellWidth = new (GRID_CELL_WIDTH_KEY, GRID_CELL_WIDTH_DEFAULT);
        private static readonly PrefsFloatValue _gridAngleStep = new (GRID_ANGLE_STEP_KEY, GRID_ANGLE_STEP_DEFAULT);
        private static readonly PrefsFloatValue _gridScaleStep = new (GRID_SCALE_STEP_KEY, GRID_SCALE_STEP_DEFAULT);
        private static readonly PrefsBoolValue _gridShowOriginalObject = new (GRID_SHOW_ORIGINAL_OBJECT_KEY, GRID_SHOW_ORIGINAL_OBJECT_DEFAULT);
        private static readonly PrefsBoolValue _gridUseObjectCenter = new (GRID_USE_OBJECT_CENTER_KEY, GRID_USE_OBJECT_CENTER_DEFAULT);

        private static readonly PrefsBoolValue _localSave = new (LOCAL_SAVE, LOCAL_SAVE_DEFAULT);
        private static readonly PrefsBoolValue _cloudSave = new (CLOUD_SAVE, CLOUD_SAVE_DEFAULT);
        private static readonly PrefsBoolValue _showPublicUploadWarning = new (SHOW_PUBLIC_UPLOAD_WARNING, SHOW_PUBLIC_UPLOAD_WARNING_DEFAULT);

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

        public static float gridCellWidth
        {
            get => _gridCellWidth.Value;
            set => _gridCellWidth.Value = value;
        }

        public static float gridAngleStep
        {
            get => _gridAngleStep.Value;
            set => _gridAngleStep.Value = value;
        }

        public static float gridScaleStep
        {
            get => _gridScaleStep.Value;
            set => _gridScaleStep.Value = value;
        }

        public static bool gridShowOriginalObject
        {
            get => _gridShowOriginalObject.Value;
            set => _gridShowOriginalObject.Value = value;
        }

        public static bool gridUseObjectCenter
        {
            get => _gridUseObjectCenter.Value;
            set => _gridUseObjectCenter.Value = value;
        }

        public static bool publicUploadPrivacy
        {
            get => _publicUploadPrivacy.Value;
            set => _publicUploadPrivacy.Value = value;
        }

        public static bool publicLocalSave
        {
            get => _localSave.Value;
            set => _localSave.Value = value;
        }

        public static bool publicCloudSave
        {
            get => _cloudSave.Value;
            set => _cloudSave.Value = value;
        }

        public static bool publicShowPublicUploadWarning
        {
            get => _showPublicUploadWarning.Value;
            set => _showPublicUploadWarning.Value = value;
        }

        public static bool showBigCards
        {
            get => _showBigCards.Value;
            set => _showBigCards.Value = value;
        }

        public static bool rememberUser
        {
            get => _rememberUser.Value;
            set => _rememberUser.Value = value;
        }

        public static bool developMode
        {
            get => _developMode.Value;
            set => _developMode.Value = value;
        }

        public static bool rememberSketchfabUser
        {
            get => _rememberSketchfabUser.Value;
            set => _rememberSketchfabUser.Value = value;
        }

        public static bool showGrid
        {
            get => _showGrid.Value;
            set => _showGrid.Value = value;
        }

        public static bool snapToGrid
        {
            get => _snapToGrid.Value;
            set => _snapToGrid.Value = value;
        }

        public static bool dontShowCalibrationGuide
        {
            get => _dontShowCalibrationGuide.Value;
            set => _dontShowCalibrationGuide.Value = value;
        }

        public static bool dontShowNewAugmentationHint
        {
            get => _dontShowNewAugmentationHint.Value;
            set => _dontShowNewAugmentationHint.Value = value;
        }

        public static string domain
        {
            get => _domain.Value;
            set => _domain.Value = value;
        }

        public static string privacyPolicyDomain
        {
            get => _privacyPolicyDomain.Value;
            set => _privacyPolicyDomain.Value = value;
        }

        public static LearningRecordStores publicCurrentLearningRecordStore
        {
            get => CurrentLearningRecordStore;
            set => CurrentLearningRecordStore = value;
        }

        public static ShowBy currentShowby
        {
            get => CurrentShowby;
            set => CurrentShowby = value;
        }

        public static SortBy currentSortby
        {
            get => CurrentSortby;
            set => CurrentSortby = value;
        }

        public static string registerPage => $"{domain}/login/signup.php";


        // if user is logged into Moodle
        public static bool LoggedIn => username != null;

        public static void LogOut()
        {
            username = null;
            token = null;
            userid = null;
            usermail = null;
            rememberUser = false;
            LocalFiles.RemoveUsernameAndPassword();
        }

    }
}