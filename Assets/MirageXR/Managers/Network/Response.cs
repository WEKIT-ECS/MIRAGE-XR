using System.Collections.Generic;

namespace MirageXR.NewDataModel
{
    public class Response
    {
        public List<Error>? Errors { get; set; }
        public ResponseStatusCode StatusCode { get; set; } = ResponseStatusCode.OK;

        public static Response<T> Success<T>(T data)
        {
            return new Response<T>(data);
        }

        public static Response Failure(ErrorCodes errorCode, string description, ResponseStatusCode statusCode)
        {
            var error = new Error {Code = errorCode, ErrorMessage = description};
            return new Response {Errors = new List<Error> {error}, StatusCode = statusCode};
        }

        public static Response<T> Failure<T>(T data, ErrorCodes errorCode, string description, ResponseStatusCode statusCode)
        {
            var error = new Error {Code = errorCode, ErrorMessage = description};
            return new Response<T> {Data = data, Errors = new List<Error> {error}, StatusCode = statusCode};
        }

        public static Response Success()
        {
            return new Response();
        }
    }

    public class Response<T> : Response
    {
        public T Data { get; set; }

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
        public string ErrorMessage { get; set; }
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
            if (response.Errors is { Count: > 0 })
            {
                Error = response.Errors[0];  // The 1st element of the List in Response
            }
        }
    }
}