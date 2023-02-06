using UnityEngine;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
#if NETFX_CORE
using Windows.System;
#endif

namespace MirageXR
{
    /// <summary>
    /// Class that bundles the network access
    /// </summary>
    public static class Network
    {

        #region obsolete code

        private const string DEFAULT_DOWNLOAD_URL = "https://wekitproject.appspot.com/storage/sessions?category=ARLEM";
        private const string DEFAULT_UPLOAD_URL = "http://wekitproject.appspot.com/storage/requestupload";

        private static readonly HttpClient client = new HttpClient();

        public static async void Upload(string path, string recordingId, Action<HttpStatusCode> callback)
        {
            var result = await UploadAsync(path, recordingId);
            callback(result);
        }

        public static async Task<HttpStatusCode> UploadAsync(string path, string recordingId)
        {
            var bytes = await CompressRecord(path, recordingId);
            if (bytes == null) return HttpStatusCode.SeeOther;
            var (url, status) = await GetUploadUrlAndTokenAsync();
            if (status != HttpStatusCode.OK) return status;
            return await UploadAsync(url, bytes, recordingId);
        }

        private static async Task<byte[]> CompressRecord(string path, string recordingId)
        {
            byte[] bytes = null;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipStream = new ZipOutputStream(stream))
                    {
                        await ZipUtilities.CompressFolderAsync(path, zipStream);
                        await ZipUtilities.AddFileToZipStreamAsync(
                            zipStream,
                            $"{path}-activity.json",
                            $"{recordingId}-activity.json");
                        await ZipUtilities.AddFileToZipStreamAsync(
                            zipStream,
                            $"{path}-workplace.json",
                            $"{recordingId}-workplace.json");
                    }
                    bytes = stream.ToArray();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"compression error: {e}");
            }

            return bytes;
        }

        private static async Task<(string, HttpStatusCode)> GetUploadUrlAndTokenAsync()
        {
            var response = await client.PostAsync(DEFAULT_UPLOAD_URL, null);
            return (await response.Content.ReadAsStringAsync(), response.StatusCode);
        }

        private static async Task<HttpStatusCode> UploadAsync(string url, byte[] content, string recordingId)
        {
            Debug.Log($"secondPost: uploading to {url}");
            var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(content);

            var header = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"myFile\"",
                FileName = $"\"{recordingId}\""
            };
            fileContent.Headers.ContentDisposition = header;
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-zip-compressed");
            form.Add(fileContent, "myFile", recordingId);

            // fixed parameters
            // string macAddress = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            var userName = await GetUserNameAsync();
            form.Add(new StringContent("Hololens"), "\"device\"");
            form.Add(new StringContent(userName), "\"author\"");
            // ServiceManager.GetService<ActivityService>().UpdateSessionName(myTitle.text);
            var arlemName = string.Empty;


            try { arlemName = RootObject.Instance.activityManager.Activity.name; }
            catch (Exception) { /*ignore*/ }



            if (string.IsNullOrEmpty(arlemName))
                arlemName = $"WEKIT ARLEM recorder session of {DateTime.Now.ToString(CultureInfo.InvariantCulture)}";

            form.Add(new StringContent(arlemName), "\"description\"");
            // form.Add(new StringContent("WEKIT ARLEM recorder session of " + DateTime.Now.ToString()), "\"description\"");
            form.Add(new StringContent("ARLEM"), "\"category\"");

            var response = await client.PostAsync(url, form);
            var responseString = await response.Content.ReadAsStringAsync();

            Debug.Log("secondPost: PostAsync ");
            Debug.Log($"secondPost: read response: {response}");
            Debug.Log($"secondPost: done: {responseString}");

            return response.StatusCode;
        }

        private static async Task<string> GetUserNameAsync()
        {
            var displayName = "unknown user";
#if NETFX_CORE
            // Populate the list of users.
            IReadOnlyList<User> users = await User.FindAllAsync();
            int userNumber = 1;
            foreach (User user in users)
            {
                displayName = (string)await user.GetPropertyAsync(KnownUserProperties.DisplayName);
                // Choose a generic name if we do not have access to the actual name.
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = $"User #{userNumber}";
                    userNumber++;
                }
            }
#endif
            return displayName;
        }

        // downloads the sensor recording from Learning Hub's backend storage to hololens, unzips it to the ARLEM directory
        public static async Task<bool> DownloadAndDecompressAsync(string downloadUrl, string filename, string targetDir)
        {
            Debug.Log($"DownloadAndDecompress: {filename}, {downloadUrl}, {targetDir}");
            var success = false;
            var response = await client.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode)
            {
                var unzipDir = Path.Combine(targetDir, filename.Substring(0, filename.Length - 4));
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    await ZipUtilities.ExtractZipFileAsync(contentStream, unzipDir);
                }
                success = true;
            }
            else
            {
                Debug.Log($"failed to download {filename}: {response.StatusCode}");
            }
            return success;
        }

        // downloads the sensor recording from Learning Hub's backend storage to hololens, unzips it to the ARLEM directory
        public static async Task<bool> DownloadAndDecompressPlayerAsync(string downloadUrl, string filename, string targetDir)
        {
            Debug.Log($"DownloadAndDecompress: {filename}, {downloadUrl}, {targetDir}");
            var success = false;

            var response = await client.GetAsync(downloadUrl);
            if (response.IsSuccessStatusCode)
            {
                var unzipDir = targetDir;
                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    await ZipUtilities.ExtractZipFileAsync(contentStream, unzipDir);
                }
                success = true;
            }
            else
            {
                Debug.Log($"failed to download {filename}: {response.StatusCode}");
            }

            return success;
        }

        // downloads the list of downloadable ARLEM sessions from server
        public static async Task<SimpleJSON.JSONNode> GetDownloadableSessionsAsync(string sessionUrl = DEFAULT_DOWNLOAD_URL)
        {
            Debug.Log($"GetDownloadableSessions: {sessionUrl}");
            var response = await client.GetAsync(sessionUrl);
            if (response.IsSuccessStatusCode)
            {
                var sessionJson = await response.Content.ReadAsStringAsync();
                var node = SimpleJSON.JSON.Parse(sessionJson);
                // Debug.Log("parsed sessions: " + node.Count);
                //  each entry in the JSONNode has the following structure:
                /*
                 *  {
                 *       "id":5746569068412928,
                 *       "author":"User #1",
                 *       "uploadingDevice":"Hololens",
                 *       "description":"WEKIT ARLEM recorder session of 10/19/2018 6:05:05 AM",
                 *       "category":"ARLEM",
                 *       "size":56983,
                 *       "filename":"session-2018-10-19_06-02-46.zip",
                 *       "filetype":"application/x-zip-compressed",
                 *       "key":"/gs/wekitproject.appspot.com/L2FwcGhvc3RpbmdfZ2xvYmFsL2Jsb2JzL0FFbkIyVXB5eDJqd1hiMXVOU3BISndsS1VoblZhMHdRdEFaeGZWVHFjUERqbkNXZTZjSXhFU0tmaVpuWEVkYUZoSzFxX0xRWFV6SHhpeTFJVXJxblFvUVotT2NoOHg3ZjVBLmlOdDdQYmZsTDRUd3JvVm8",
                 *       "uploadingDate":"Oct 19, 2018 1:05:07 PM"
                 *  }
                 *
                 *  prepend the key attribute with "https://wekitproject.appspot.com/storage/serve" for download of that session
                 */
                // Debug.Log("node as array: " + node.AsArray.Count);
                return node;
            }

            Debug.Log($"failed to load sessions from: {sessionUrl}: {response.StatusCode}");
            return null;
        }
        #endregion

        // timeout after which a ongoing request fails
        private const int REQUEST_TIMEOUT_IN_SECONDS = 30; // 0.5 minute

        // timeout after which an ongoing upload fails
        private const int UPLOAD_TIMEOUT_IN_SECONDS = 600; // 10 minutes

        /// <summary>
        /// Tries to log in the user on the Moodle backend.
        /// </summary>
        /// <param name="userName">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="domain">The domain of the Moodle instance</param>
        /// <returns>Returns a tuple with the response (true = success, false = fail) and the resulting string</returns>
        public static async Task<(bool, string)> LoginRequestAsync(string userName, string password, string domain)
        {
            const string uriFormat = "{0}/mod/{1}/classes/webservice/authunity.php";
            const string userNameKey = "username";
            const string passwordKey = "password";
            const string domainKey = "domain";

            var form = new MultipartFormDataContent
            {
                { new StringContent(userName), userNameKey },
                { new StringContent(password), passwordKey },
                { new StringContent(domain), domainKey }
            };

            var uri = string.Format(uriFormat, domain, DBManager.plugin);
            return await PostRequestAsync(uri, form, REQUEST_TIMEOUT_IN_SECONDS);
        }

        public static async Task<(bool, string)> GetCustomDataFromAPIRequestAsync(string token, string domain, string requestValue, string function, string parametersValueFormat)
        {
            const string uriFormat = "{0}/mod/{1}/classes/webservice/getdata_from_moodle.php";
            const string tokenKey = "token";
            const string functionKey = "function";
            const string parametersKey = "parameters";
            const string requestKey = "request";

            var form = new MultipartFormDataContent
            {
                { new StringContent(token), tokenKey },
                { new StringContent(function), functionKey },
                { new StringContent(parametersValueFormat), parametersKey },
                { new StringContent(requestValue), requestKey }
            };

            var uri = string.Format(uriFormat, domain, DBManager.plugin);
            return await PostRequestAsync(uri, form, REQUEST_TIMEOUT_IN_SECONDS);
        }

        public static async Task<(bool, string)> GetCustomDataFromDBRequestAsync(string userid, string domain, string requestValue, string tokenValue, string itemid = null, string sessionid = null)
        {
            const string uriFormat = "{0}/mod/{1}/classes/webservice/getdata_from_db.php";
            const string requestKey = "request";
            const string useridKey = "userid";
            const string tokenKey = "token";
            const string itemidKey = "itemid";
            const string sessionIdKey = "sessionid";

            var form = new MultipartFormDataContent { { new StringContent(requestValue), requestKey } };
            if (userid != null) form.Add(new StringContent(userid), useridKey);
            if (tokenValue != null) form.Add(new StringContent(tokenValue), tokenKey);

            // if you need to identify an arlem in moodle send its itemid to moodle and get back any info of that file
            // you can only use either sessionid or itemid to identify a file, however recommend use both.
            if (itemid != null) form.Add(new StringContent(itemid), itemidKey);
            if (sessionid != null) form.Add(new StringContent(sessionid), sessionIdKey);

            var uri = string.Format(uriFormat, domain, DBManager.plugin);
            return await PostRequestAsync(uri, form, REQUEST_TIMEOUT_IN_SECONDS);
        }

        public static async Task<(bool, string)> UploadRequestAsync(
            string token,
            string userId,
            string sessionId,
            bool isPublic,
            string fileName,
            string title,
            byte[] zipContent,
            string thumbnailFileName,
            byte[] thumbnailContent,
            string domain,
            string activityJson,
            string workplaceJson,
            int updateMode)
        {
            const string uriFormat = "{0}/mod/{1}/classes/webservice/getfile_from_unity.php";
            const string tokenKey = "token";
            const string userIdKey = "userid";
            const string sessionIdKey = "sessionid";
            const string publicKey = "public";
            const string fileKey = "myfile";
            const string titleKey = "title";
            const string thumbnailKey = "thumbnail";
            const string zipMediaType = "application/x-zip-compressed";
            const string imageMediaType = "image/jpg";
            const string descriptionType = "form-data";
            const string activityJsonKey = "activity";
            const string workplaceJsonKey = "workplace";
            const string updateFileKey = "updatefile";

            var zipBinaryContent = new ByteArrayContent(zipContent);
            zipBinaryContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(descriptionType)
            {
                Name = fileKey,
                FileName = fileName
            };
            zipBinaryContent.Headers.ContentType = MediaTypeHeaderValue.Parse(zipMediaType);

            ByteArrayContent thumbnailBinaryContent = null;
            if (thumbnailContent != null && thumbnailFileName != null)
            {
                thumbnailBinaryContent = new ByteArrayContent(thumbnailContent);
                thumbnailBinaryContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(descriptionType)
                {
                    Name = thumbnailKey,
                    FileName = thumbnailFileName
                };
                thumbnailBinaryContent.Headers.ContentType = MediaTypeHeaderValue.Parse(imageMediaType);
            }

            var form = new MultipartFormDataContent
            {
                { new StringContent(token), tokenKey },
                { new StringContent(userId), userIdKey },
                { new StringContent(updateMode.ToString()), updateFileKey },
                { new StringContent(sessionId), sessionIdKey },
                { new StringContent(title), titleKey },
                { new StringContent(activityJson), activityJsonKey },
                { new StringContent(workplaceJson), workplaceJsonKey },
                { new StringContent((isPublic ? 1 : 0).ToString()), publicKey },
                { zipBinaryContent, fileKey, fileName },
            };

            if (thumbnailBinaryContent != null)
            {
                form.Add(thumbnailBinaryContent, thumbnailKey, thumbnailFileName);
            }

            var uri = string.Format(uriFormat, domain, DBManager.plugin);
            return await PostRequestAsync(uri, form, UPLOAD_TIMEOUT_IN_SECONDS);
        }

        public static async Task<(bool, string)> DownloadRequestToStreamAsync(
            Stream stream,
            string contextId,
            string component,
            string fileArea,
            string itemId,
            string fileName,
            string domain,
            CancellationToken cancellationToken = default)
        {
            const string uriFormat = "{0}/pluginfile.php/{1}/{2}/{3}/{4}/{5}";

            var uri = string.Format(uriFormat, domain, contextId, component, fileArea, itemId, fileName);
            return await GetRequestToStreamAsync(uri, stream, UPLOAD_TIMEOUT_IN_SECONDS, cancellationToken);
        }

        private static async Task<(bool, string)> PostRequestAsync(string uri, HttpContent httpContent, int timeout, CancellationToken cancellationToken = default)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    using (var response = await client.PostAsync(uri, httpContent, cancellationToken))
                    {
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

        private static async Task<(bool, string)> GetRequestToStreamAsync(string uri, Stream stream, int timeout, CancellationToken cancellationToken = default)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    using (var response = await client.GetAsync(uri, cancellationToken))
                    {
                        await response.Content.CopyToAsync(stream);
                        return (true, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }

        public static async Task<(bool, string)> DownloadToStreamAsync(string uri, Stream stream, int timeout, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(timeout);
                try
                {
                    await client.DownloadAsync(uri, stream, progress, cancellationToken);
                    return (true, string.Empty);
                }
                catch (Exception e)
                {
                    return (false, e.ToString());
                }
            }
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            const int bufferSize = 81920;
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {

                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return;
                    }

                    var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                    await download.CopyToAsync(destination, bufferSize, relativeProgress, cancellationToken);
                    progress.Report(1f);
                }
            }
        }
    }

    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!source.CanRead)
            {
                throw new ArgumentException("Has to be readable", nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException("Has to be writable", nameof(destination));
            }

            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}