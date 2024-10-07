using System.Collections.Generic;
using Newtonsoft.Json;

namespace MirageXR.NewDataModel
{
    public class Response
    {
        [JsonProperty] public Error? Error { get; set; }
        [JsonProperty] public ResponseStatusCode StatusCode { get; set; } = ResponseStatusCode.OK;
        public bool IsSuccess => StatusCode is ResponseStatusCode.OK or ResponseStatusCode.Created;

        public static Response<T> Success<T>(T data)
        {
            return new Response<T>(data);
        }

        public static Response Failure(ErrorCodes errorCode, string description, ResponseStatusCode statusCode)
        {
            var error = new Error {Code = errorCode, Message = description};
            return new Response {Error = error, StatusCode = statusCode};
        }

        public static Response<T> Failure<T>(T data, ErrorCodes errorCode, string description, ResponseStatusCode statusCode)
        {
            var error = new Error {Code = errorCode, Message = description};
            return new Response<T> {Data = data, Error = error, StatusCode = statusCode};
        }

        public static Response Success()
        {
            return new Response();
        }
    }

    public class Response<T> : Response
    {
        [JsonProperty] public T Data { get; set; }

        public Response()
        {
        }

        public Response(T value)
        {
            Data = value;
        }

        public static implicit operator T(Response<T> response)
        {
            return response.Data;
        }

        public static implicit operator Response<T>(T data)
        {
            return Success(data);
        }
    }

    public class Error
    {
        public string Message { get; set; }
        public ErrorCodes Code { get; set; }
    }

    public class FailedResponseData
    {
        public ResponseStatusCode StatusCode { get; set; }
        public Error Error { get; set; }

        public FailedResponseData(ResponseStatusCode statusCode, Error error)
        {
            StatusCode = statusCode;
            Error = error;
        }

        public FailedResponseData(Response response)
        {
            StatusCode = response.StatusCode;
            Error = response.Error;
        }
    }
}