using System;
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

        private const string DEVELOP_MODE_KEY = "DevelopMode";
        private const bool DEVELOP_MODE_DEFAULT = false;

        private const string REMEMBER_SKETCHFAB_USER_KEY = "RememberSketchfabUser";
        private const bool REMEMBER_SKETCHFAB_USER_DEFAULT = false;

        private const string SHOW_BIG_CARDS_KEY = "ShowBigCards";
        public const bool SHOW_BIG_CARDS_DEFAULT = false;

        private const string DONT_SHOW_CALIBRATION_GUIDE_KEY = "DontShowCalibrationGuide";
        private const bool DONT_SHOW_CALIBRATION_GUIDE_DEFAULT = false;

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


        private const int SKETCHFAB_RENEW_DYAS_COOLDOWN = 5;

        public static string plugin = "arete";

        public static string username;
        public static string userid;
        public static string usermail;
        public static string token;

        public enum LearningRecordStores {WEKIT, ARETE};

        private static LearningRecordStores CurrentLearningRecordStore;

        public enum ShowBy {ALL, MYASSIGNMENTS, MYACTIVITIES};

        private static ShowBy CurrentShowby = ShowBy.ALL;

        public enum SortBy {DATE, RELEVEANCE};

        private static SortBy CurrentSortby = SortBy.DATE;

        private static readonly PrefsBoolValue _publicUploadPrivacy = new PrefsBoolValue(UPLOAD_PRIVACY_KEY, PUBLIC_UPLOAD_PRIVACY_DEFAULT);
        private static readonly PrefsBoolValue _rememberUser = new PrefsBoolValue(REMEMBER_USER_KEY, REMEMBER_USER_DEFAULT);
        private static readonly PrefsBoolValue _developMode = new PrefsBoolValue(DEVELOP_MODE_KEY, DEVELOP_MODE_DEFAULT);
        private static readonly PrefsBoolValue _rememberSketchfabUser = new PrefsBoolValue(REMEMBER_SKETCHFAB_USER_KEY, REMEMBER_SKETCHFAB_USER_DEFAULT);
        private static readonly PrefsBoolValue _dontShowCalibrationGuide = new PrefsBoolValue(DONT_SHOW_CALIBRATION_GUIDE_KEY, DONT_SHOW_CALIBRATION_GUIDE_DEFAULT);
        private static readonly PrefsStringValue _domain = new PrefsStringValue(MOODLE_URL_KEY, WEKIT_URL);
        private static readonly PrefsStringValue _privacyPolicyDomain = new PrefsStringValue(PRIVACY_POLICY_URL_KEY, WEKIT_PRIVACY_POLICY_URL);
        private static readonly PrefsStringValue _sketchfabTokenRenewDate = new PrefsStringValue(SKETCHFAB_TOKEN_RENEW_KEY, SKETCHFAB_TOKEN_RENEW_DEFAULT);
        private static readonly PrefsBoolValue _showBigCards = new PrefsBoolValue(SHOW_BIG_CARDS_KEY, SHOW_BIG_CARDS_DEFAULT);

        private static readonly PrefsBoolValue _localSave = new PrefsBoolValue(LOCAL_SAVE, LOCAL_SAVE_DEFAULT);
        private static readonly PrefsBoolValue _cloudSave = new PrefsBoolValue(CLOUD_SAVE, CLOUD_SAVE_DEFAULT);
        private static readonly PrefsBoolValue _showPublicUploadWarning = new PrefsBoolValue(SHOW_PUBLIC_UPLOAD_WARNING, SHOW_PUBLIC_UPLOAD_WARNING_DEFAULT);

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