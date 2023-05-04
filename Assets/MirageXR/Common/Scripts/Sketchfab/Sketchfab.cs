using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using i5.Toolkit.Core.VerboseLogging;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace MirageXR
{
    public static class Sketchfab
    {

        public struct SketchfabResponse
        {
            public string error;
            public string access_token;
            public string refresh_token;
        }

        private const float MAX_THUMBNAIL_HEIGHT = 256;
        private const float MAX_USER_AVATAR_HEIGHT = 256;
        private const float MAX_COLLECTION_AVATAR_HEIGHT = 256;

        private const string JSON_FILE_NAME = "previewInfo.json";
        private const string FOLDER_NAME = "models";
        private const string MODEL_NAME = "scene.gltf";

        public static string GetSearchUrl(string searchOption, string searchTerm, int cursorPosition, int itemsPrePage, bool downloadable = true)
        {
            var sb = new StringBuilder();
            sb.Append($"search?count={itemsPrePage}");
            sb.Append($"&cursor={cursorPosition}");
            sb.Append($"&type={searchOption.ToLower()}");
            sb.Append($"&q={searchTerm.ToLower()}");
            sb.Append($"&downloadable={downloadable.ToString().ToLower()}");
            return $"https://api.sketchfab.com/v3/{sb}";
        }

        public static async Task<(bool, string)> SearchModelsAsync(string token, string searchOption, string searchTerm, int cursorPosition, int itemsPrePage, bool downloadable = true)
        {
            const int timeout = 60;
            var url = GetSearchUrl(searchOption, searchTerm, cursorPosition, itemsPrePage, downloadable);
            return await DownloadStringAsync(url, token, timeout);
        }

        public static List<ModelPreviewItem> ReadWebResults(string result, string searchOption)
        {
            var previewList = new List<ModelPreviewItem>();

            Debug.Log($"search raw {searchOption} result:\n{result}");

            switch (searchOption.ToLower())
            {
                case "models":
                    {
                        var foundModels = JsonUtility.FromJson<SketchfabModelSearchResult>(result).results;
                        previewList.AddRange(foundModels.Select(CreateModelPreview));
                        break;
                    }
                case "users":
                    {
                        var foundUsers = JsonUtility.FromJson<SketchfabUserSearchResult>(result).results;
                        previewList.AddRange(foundUsers.Select(CreateUserPreview));
                        break;
                    }
                case "collections":
                    {
                        var foundCollections = JsonUtility.FromJson<SketchfabCollectionSearchResult>(result).results;
                        previewList.AddRange(foundCollections.Select(CreateUserPreview));
                        break;
                    }
            }

            return previewList;
        }

        private static ModelPreviewItem CreateModelPreview(SketchfabModel sketchfabModel)
        {
            var image = sketchfabModel.thumbnails.images.FirstOrDefault(t => t.height < MAX_THUMBNAIL_HEIGHT) ??
                        sketchfabModel.thumbnails.images.First();
            return new ModelPreviewItem
            {
                name = sketchfabModel.name,
                description = sketchfabModel.description,
                uid = sketchfabModel.uid,
                resourceUrl = sketchfabModel.uri,
                resourceImage = new ThumbnailImage
                {
                    url = image.url,
                    uid = image.uid,
                    width = image.width,
                    height = image.height
                }
            };
        }

        private static ModelPreviewItem CreateUserPreview(SketchfabUser sketchfabUser)
        {
            var image = sketchfabUser.avatar.images.FirstOrDefault(t => t.height < MAX_USER_AVATAR_HEIGHT) ??
                        sketchfabUser.avatar.images.First();
            return new ModelPreviewItem
            {
                name = sketchfabUser.username,
                description = sketchfabUser.displayName,
                uid = sketchfabUser.uid,
                resourceUrl = sketchfabUser.profileUrl,
                resourceImage = new ThumbnailImage
                {
                    url = image.url,
                    uid = image.uid,
                    width = image.width,
                    height = image.height
                }
            };
        }

        private static ModelPreviewItem CreateUserPreview(SketchfabCollection sketchfabCollection)
        {
            var image = sketchfabCollection.user.avatar.images.FirstOrDefault(t => t.height < MAX_COLLECTION_AVATAR_HEIGHT) ??
                        sketchfabCollection.user.avatar.images.First();
            return new ModelPreviewItem
            {
                name = sketchfabCollection.name,
                description = sketchfabCollection.slug,
                uid = sketchfabCollection.uid,
                resourceUrl = sketchfabCollection.collectionUrl,
                resourceImage = new ThumbnailImage
                {
                    url = image.url,
                    uid = image.uid,
                    width = image.width,
                    height = image.height
                }
            };
        }

        public static async Task<(bool, string)> GetTokenAsync(string username, string password, string clientId, string clientSecret)
        {
            const int timeout = 60 * 2;

            const string url = "https://sketchfab.com/oauth2/token/";
            const string authorizationKey = "Basic";
            const string grantTypeKey = "grant_type";
            const string usernameKey = "username";
            const string passwordKey = "password";
            const string grantTypeValue = "password";

            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent
                {
                    { new StringContent(grantTypeValue), grantTypeKey },
                    { new StringContent(username), usernameKey },
                    { new StringContent(password), passwordKey }
                };

                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    var credentials = Base64Encode($"{clientId}:{clientSecret}");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationKey, credentials);
                    var content = await client.PostAsync(url, form);
                    var response = await content.Content.ReadAsStringAsync();
                    return (true, response);
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }

        public static async Task<(bool, string)> RenewTokenAsync(string renewToken, string clientId, string clientSecret)
        {
            const int timeout = 60 * 2;

            const string url = "https://sketchfab.com/oauth2/token/";
            const string grantTypeKey = "grant_type";
            const string clientIdKey = "client_id";
            const string clientSecretKey = "client_secret";
            const string refreshKey = "refresh_token";
            const string grantTypeValue = "refresh_token";

            if (string.IsNullOrEmpty(renewToken) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return (false, null);
            }

            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent
                {
                    { new StringContent(grantTypeValue), grantTypeKey },
                    { new StringContent(clientId), clientIdKey },
                    { new StringContent(clientSecret), clientSecretKey },
                    { new StringContent(renewToken), refreshKey }
                };

                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    var content = await client.PostAsync(url, form);
                    var response = await content.Content.ReadAsStringAsync();
                    return (true, response);
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }

        public static async Task<(bool, Sprite)> LoadSpriteAsync(string url)
        {
            try
            {
                var (result, texture) = await LoadTextureAsync(url);
                if (!result)
                {
                    return (false, null);
                }

                // scale according to width or height (fit to button), create and store
                var rec = new Rect(0, 0, texture.width, texture.height);
                var ppu = Mathf.Max(texture.width / 9f, texture.height / 6f);
                return (true, Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), ppu));
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return (false, null);
            }
        }

        public static async Task<(bool, Texture2D)> LoadTextureAsync(string url)
        {
            try
            {
                const int timeout = 60 * 2;

                const string httpPrefix = "http";
                const string filePrefix = "file://";

                byte[] bytes;
                var texture = new Texture2D(2, 2);
                if (url.StartsWith(httpPrefix))
                {
                    (_, bytes) = await DownloadBytesAsync(url, timeout);
                }
                else if (url.StartsWith(filePrefix))
                {
                    (_, bytes) = await ReaFileAsync(url.Substring(filePrefix.Length));
                }
                else
                {
                    return (false, null);
                }

                var result = bytes != null && texture.LoadImage(bytes);
                return (result, result ? texture : null);
            }
            catch (Exception e)
            {
                AppLog.LogError(e.ToString());
                return (false, null);
            }
        }

        private static async Task<(bool, byte[])> ReaFileAsync(string file)
        {
            byte[] bytes;
            if (!File.Exists(file)) return (false, null);
            using (var stream = new FileStream(file, FileMode.Open))
            {
                bytes = new byte[stream.Length];
                await stream.ReadAsync(bytes, 0, (int)stream.Length);
            }

            return (true, bytes);
        }

        private static async Task<(bool, byte[])> DownloadBytesAsync(string url, int timeout, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            using (var memory = new MemoryStream())
            {
                var (result, _) = await Network.DownloadToStreamAsync(url, memory, timeout, progress, cancellationToken);
                return (result, result ? memory.ToArray() : null);
            }
        }

        public static async Task<(bool, ModelDownloadInfo)> GetDownloadInfoAsync(string token, ModelPreviewItem modelPreviewItem)
        {
            const int timeout = 2 * 60;
            const string webReqUrlFormat = "https://api.sketchfab.com/v3/models/{0}/download";

            var webReqUrl = string.Format(webReqUrlFormat, modelPreviewItem.uid);
            var (result, content) = await DownloadStringAsync(webReqUrl, token, timeout);
            ModelDownloadInfo downloadInfo = null;
            if (result)
            {
                downloadInfo = JsonUtility.FromJson<ModelDownloadInfo>(content);
            }
            return (result, downloadInfo);
        }

        public static async Task<bool> DownloadModelAndExtractAsync(string url, ModelPreviewItem modelPreview, Action<float> onProgressChanged = null)
        {
            const int timeout = 5 * 60;

            var archiveName = $"{modelPreview.name}.zip";
            var imageName = $"{modelPreview.name}.png";

            modelPreview.name = GetValidFileName(modelPreview.name);

            var modelsFolderPath = Path.Combine(Application.persistentDataPath, FOLDER_NAME);
            if (!Directory.Exists(modelsFolderPath)) Directory.CreateDirectory(modelsFolderPath);

            var modelFolderPath = Path.Combine(modelsFolderPath, modelPreview.name);
            if (!Directory.Exists(modelFolderPath)) Directory.CreateDirectory(modelFolderPath);

            var archivePath = Path.Combine(modelsFolderPath, archiveName);
            var jsonPath = Path.Combine(modelFolderPath, JSON_FILE_NAME);
            var modelPath = Path.Combine(modelFolderPath, MODEL_NAME);
            var imagePath = Path.Combine(modelFolderPath, imageName);

            using (var stream = new FileStream(archivePath, FileMode.OpenOrCreate))
            {
                Progress<float> progress = null;
                if (onProgressChanged != null) progress = new Progress<float>(onProgressChanged.Invoke);
                var (result, _) = await Network.DownloadToStreamAsync(url, stream, timeout, progress);
                if (!result) return false;

                modelPreview.fileSize = stream.Length;
            }

            if (!Directory.Exists(modelFolderPath)) Directory.CreateDirectory(modelFolderPath);

            using (var stream = new FileStream(imagePath, FileMode.OpenOrCreate))
            {
                var (result, _) = await Network.DownloadToStreamAsync(modelPreview.resourceImage.url, stream, timeout);
                if (!result)
                {
                    return false;
                }
                modelPreview.resourceImage.url = $"file://{imagePath}";
            }

            using (var stream = new FileStream(archivePath, FileMode.Open))
            {
                await ZipUtilities.ExtractZipFileAsync(stream, modelFolderPath);
                modelPreview.resourceUrl = $"file://{modelPath}";
            }

            var output = Newtonsoft.Json.JsonConvert.SerializeObject(modelPreview, Newtonsoft.Json.Formatting.Indented);

            using (var fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(fs))
            {
                await writer.WriteAsync(output);
            }

            return true;
        }

        public static async Task LoadModelAsync(ModelPreviewItem modelPreview)
        {
            var modelsFolderPath = Path.Combine(Application.persistentDataPath, FOLDER_NAME);
            var archiveUrl = Path.Combine(modelsFolderPath, $"{modelPreview.name}.zip");
            var modelFolder = Path.Combine(modelsFolderPath, modelPreview.name);
            var targetDirectory = Path.Combine(RootObject.Instance.activityManager.ActivityPath, modelPreview.name);
            if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

            // build zip archive, if required (might be slow for big models)
            if (!File.Exists(archiveUrl))
            {
                Debug.Log("model archive not found, taking remedial action...");
                using (var stream = new FileStream(archiveUrl, FileMode.OpenOrCreate))
                using (var zipStream = new ZipOutputStream(stream))
                {
                    await ZipUtilities.CompressFolderAsync($"{modelFolder}\\", zipStream);
                }
            }

            // unpack to session folder
            using (var stream = new FileStream(archiveUrl, FileMode.Open))
            {
                await ZipUtilities.ExtractZipFileAsync(stream, targetDirectory);
            }
        }

        public static List<ModelPreviewItem> GetLocalModels()
        {
            const string searchPattern = "*";

            var modelsFolderPath = Path.Combine(Application.persistentDataPath, FOLDER_NAME);
            if (!Directory.Exists(modelsFolderPath)) Directory.CreateDirectory(modelsFolderPath);
            var localModelDirs = Directory.GetDirectories(modelsFolderPath, searchPattern, SearchOption.TopDirectoryOnly);
            var items = new List<ModelPreviewItem>();
            foreach (var dir in localModelDirs)
            {
                var jsonPath = Path.Combine(dir, JSON_FILE_NAME);
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    items.Add(JsonUtility.FromJson<ModelPreviewItem>(json));
                }
            }

            return items;
        }

        public static async Task<(bool, string)> DownloadStringAsync(string uri, string token, int timeout)
        {
            const string authorizationKey = "Bearer";
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationKey, token);
                    var content = await client.GetStringAsync(uri);
                    return (true, content);
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }

        private static string GetValidFileName(string source)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            return r.Replace(source, string.Empty);
        }

        public static async Task<(bool, string)> RequestToken(string appId, string appSecret, string code, CancellationToken cancellationToken = default)
        {
            const int timeout = 60;
            const string redirectUri = "https://wekit-ecs.com/sso/callback.php";
            const string uri = "https://sketchfab.com/oauth2/token/";
            const string contentType = "application/x-www-form-urlencoded";

            const string clientIdKey = "client_id";
            const string redirectUriKey = "redirect_uri";
            const string codeKey = "code";
            const string grantTypeKey = "grant_type";
            const string clientSecretKey = "client_secret";
            const string authorizationCodeValue = "authorization_code";

            var form = new MultipartFormDataContent
            {
                { new StringContent(appId), clientIdKey },
                { new StringContent(redirectUri), redirectUriKey },
                { new StringContent(code), codeKey },
                { new StringContent(authorizationCodeValue), grantTypeKey },
                { new StringContent(appSecret), clientSecretKey },
            };

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    using (var response = await client.PostAsync(uri, form, cancellationToken))
                    {
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        var content = await response.Content.ReadAsStringAsync();
                        return (response.IsSuccessStatusCode, content);
                    }
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
