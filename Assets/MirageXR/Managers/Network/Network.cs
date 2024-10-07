using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace MirageXR.NewDataModel
{
    public static class Network
    {
        private const int Timeout = 5;//20;
        private const string AuthorizationMethod = "Bearer";

        public enum RequestType
        {
            Get = 0,
            Patch = 1,
            Post = 2,
            Put = 3,
            Delete = 4
        }

        public static async UniTask<Response<T>> RequestAsync<T>(string url, HttpContent content = null, string token = null, Dictionary<string, string> header = null, RequestType type = RequestType.Get, float timeout = Timeout, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await GetResponseAsync(url, content, token, header, type, timeout, cancellationToken);
                var responseStr = await response.Content.ReadAsStringAsync();
                if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created)
                {
                    Debug.Log($"{type.ToString()}: {url}\nresponse: '{responseStr}'");
                }
                else if (responseStr is { Length: > 0 }) // Failures returned by server
                {
                    Debug.LogError($"{type.ToString()}: {url}\nresponse: '{responseStr}'");
                }
                else // Failures from HTTP layer
                {
                    var message = $"{type.ToString()}: {url} => StatusCode = {response.StatusCode.ToString()}"; // response.HttpResponseHeaders may contain useful info
                    var error = new Error { Code = ErrorCodes.Failure, Message = message };
                    Debug.LogError($"{type.ToString()}: {url}\nresponse: '{error}'");
                    return new Response<T>
                    {
                        StatusCode = StatusCode.ToResponseStatusCode(response.StatusCode),
                        Error = error
                    };
                }

                var data = JsonConvert.DeserializeObject<Response<T>>(responseStr);
                if (data == null)
                {
                    var message = $"{type.ToString()}: {url}\nCan't deserialize object to type {typeof(T).FullName}\nresponse: '{responseStr}'";
                    Debug.LogError(message);
                    var error = new Error { Code = ErrorCodes.Failure, Message = message };
                    return new Response<T>
                    {
                        StatusCode = ResponseStatusCode.SerializationError,
                        Error = error
                    };
                }

                return data;
            }
            catch (TaskCanceledException e)
            {
                Debug.Log(e);
                var error = new Error { Code = ErrorCodes.Failure, Message = e.Message };
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                    error.Message += $"\nInnerException: {e.InnerException.Message}";
                }

                return new Response<T> { StatusCode = ResponseStatusCode.Canceled, Error = error };
            }
            catch (TimeoutException e)
            {
                Debug.LogError(e);
                var error = new Error { Code = ErrorCodes.Failure, Message = e.Message };
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                    error.Message += $"\nInnerException: {e.InnerException.Message}";
                }

                return new Response<T> { StatusCode = ResponseStatusCode.Timeout, Error = error };
            }
            catch (HttpRequestException e)
            {
                var error = new Error { Code = ErrorCodes.Failure, Message = e.Message };
                // As HttpRequestException.Message is usually not informative,
                // try to fetch e.InnerException, which is expected to be System.Net.WebException
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                    if (e.InnerException is WebException webException)
                    {
                        var failError = HandleWebException(webException);
                        return new Response<T> { StatusCode = failError.StatusCode,  Error = failError.Error };
                    }

                    error.Message = $"\nInnerException: {e.InnerException.Message}";
                }
                Debug.LogError(e);
                return new Response<T> { StatusCode = ResponseStatusCode.NoConnection, Error = error };
            }
            catch (WebException e)
            {
                Debug.LogError(e);
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                }

                var failError = HandleWebException(e);
                return new Response<T>
                    { StatusCode = failError.StatusCode, Error = failError.Error };
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e);
                var error = new Error { Code = ErrorCodes.NotFound, Message = e.Message };
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                    error.Message += $"\nInnerException: {e.InnerException.Message}";
                }
                return new Response<T> { StatusCode = ResponseStatusCode.NotFound, Error = error };
            }
            catch (Exception e) when (e is InvalidOperationException or UriFormatException)
            {
                Debug.LogError(e);
                var error = new Error { Code = ErrorCodes.NotFound, Message = e.Message }; // bad URI
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);
                    error.Message += $"\nInnerException: {e.InnerException.Message}";
                }
                return new Response<T> { StatusCode = ResponseStatusCode.NotFound, Error = error };
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                var error = new Error { Code = ErrorCodes.Failure, Message = e.Message };
                if (e.InnerException != null)
                {
                    Debug.LogError(e.InnerException);   
                    error.Message += $"\nInnerException:{e.InnerException.Message}";
                }
                return new Response<T> { StatusCode = ResponseStatusCode.UnhandledException, Error = error };
            }
            finally
            {
                response?.Dispose();
            }
        }

        private static async Task<HttpResponseMessage> PatchCallFixAsync(HttpMessageInvoker client, string url, HttpContent content, CancellationToken cancellationToken)
        {
            const string patch = "PATCH";

            var request = new HttpRequestMessage(new HttpMethod(patch), url)
            {
                Content = content
            };

            return await client.SendAsync(request, cancellationToken);
        }

        private static async UniTask<HttpResponseMessage> GetResponseAsync(string url, HttpContent content, string token, Dictionary<string, string> header, RequestType type, float timeout = Timeout, CancellationToken cancellationToken = default)
        {
            var handler = new HttpClientHandler();

            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(timeout);

            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorizationMethod, token);
            }

            if (header != null)
            {
                foreach (var keyValuePair in header)
                {
                    client.DefaultRequestHeaders.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var response = type switch
            {
                RequestType.Get => await client.GetAsync(url, cancellationToken),
                RequestType.Post => await client.PostAsync(url, content, cancellationToken),
                RequestType.Delete => await client.DeleteAsync(url, cancellationToken),
                RequestType.Patch => await PatchCallFixAsync(client, url, content, cancellationToken),
                RequestType.Put => await client.PutAsync(url, content, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return response;
        }

        private static FailedResponseData HandleWebException(in WebException e)
        {
            var error = new Error { Code = ErrorCodes.Failure };
            var responseString= string.Empty;
            var stream = e.Response?.GetResponseStream();
            if (stream is { Length: > 0 })
            {
                responseString = $"\nResponse=({stream}).";
            }
            switch (e.Status)
            {
                case WebExceptionStatus.Success: // 0
                    error.Message = $"No error was encountered.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.OK, error);
                case WebExceptionStatus.NameResolutionFailure: // 1
                    error.Message = $"The name resolver service could not resolve the host name.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.ConnectFailure: // 2
                    error.Message = $"The remote service could not be contacted at the transport level.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.ReceiveFailure: // 3
                    error.Message = $"A complete response was not received from the remote server.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.SendFailure: // 4
                    error.Message = $"A complete request could not be sent to the remote server.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.PipelineFailure: // 5
                    error.Message = $"The connection was closed before the response for a pipelined request was received.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.RequestCanceled: // 6
                    error.Message = $"The request was canceled / aborted.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.RequestTimeout, error);
                case WebExceptionStatus.ProtocolError: // 7
                    error.Message = $"The response received from the server indicated a protocol-level error.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.ProtocolError, error);
                case WebExceptionStatus.ConnectionClosed: // 8
                    error.Message = $"The connection was prematurely closed.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.TrustFailure: // 9
                    error.Message = $"A server certificate could not be validated.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.SecureChannelFailure: // 10
                    error.Message = $"An error occurred while establishing a connection using SSL.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.ServerProtocolViolation: // 11
                    error.Message = $"The server response was not a valid HTTP response.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.KeepAliveFailure: // 12
                    error.Message = $"The connection for a request that specifies the Keep-alive header was closed unexpectedly.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.Pending: // 13
                    error.Message = $"An internal asynchronous request is pending.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.ProtocolError, error);
                case WebExceptionStatus.Timeout: // 14
                    error.Message = $"No response was received during the time-out period for a request.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.Timeout, error);
                case WebExceptionStatus.ProxyNameResolutionFailure: //15
                    error.Message = $"The name resolver service could not resolve the proxy host name.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NoConnection, error);
                case WebExceptionStatus.UnknownError: // 16
                    error.Message = $"An exception of unknown type has occurred.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.UnknownWebError, error);
                case WebExceptionStatus.MessageLengthLimitExceeded: // 17
                    error.Message = $"A message was received that exceeded the specified length limit.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.RequestEntityTooLarge, error);
                case WebExceptionStatus.CacheEntryNotFound: // 18
                    error.Message = $"The specified cache entry was not found.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.NotFound, error);
                case WebExceptionStatus.RequestProhibitedByCachePolicy: // 19
                    error.Message = $"The request was not permitted by the cache policy.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.ProtocolError, error);
                case WebExceptionStatus.RequestProhibitedByProxy: // 20
                    error.Message = $"This request was not permitted by the proxy.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.ProtocolError, error);
                default:
                    error.Message = $"Unknown WebExceptionStatus={e.Status}.{responseString}";
                    return new FailedResponseData(ResponseStatusCode.UnhandledWebFailure, error);
            }
        }
    }
}