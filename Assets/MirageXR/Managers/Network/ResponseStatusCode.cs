using System.Net;

namespace MirageXR.NewDataModel
{
    public enum ResponseStatusCode
    {
        // Standard codes copied from Unity's System.Net.HttpStatusCode metadata, also see
        // https://httpwg.org/specs/rfc9110.html#status.codes and https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode?view=net-8.0
        
        // 0 - 99 : not used

        //1xx (Informational): The request was received, continuing process
        Continue                        = HttpStatusCode.Continue,                      // = 100,   The initial part of a request has been received, the server intends to send a final response after the request has been fully received and acted upon.   
        SwitchingProtocols              = HttpStatusCode.SwitchingProtocols,            // = 101,   The server understands and is willing to proceed, via the Upgrade header field, for a change in the application protocol being used.
        Processing                      = HttpStatusCode.Processing,                    // = 102,   The server has accepted the complete request but hasn't completed it yet.
        EarlyHints                      = HttpStatusCode.EarlyHints,                    // = 103,   The server is likely to send a final response with the header fields included in the informational response (RFC 8297).

        // 2xx (Successful): The request was successfully received, understood, and accepted
        OK                              = HttpStatusCode.OK,                            // = 200,   Success
        Created                         = HttpStatusCode.Created,                       // = 201,   Success, new resource{s} created. The primary resource created is identified by either a Location header field in the response or, if no Location header field is received, by the target URI.
        Accepted                        = HttpStatusCode.Accepted,                      // = 202,   Success, but the processing has not been completed.
        NonAuthoritativeInformation     = HttpStatusCode.NonAuthoritativeInformation,   // = 203,   Success, but the enclosed content has been modified by a transforming proxy.
        NoContent                       = HttpStatusCode.NoContent,                     // = 204,   Success, and there is no additional content to send in the response content.
        ResetContent                    = HttpStatusCode.ResetContent,                  // = 205,   Success, and server requests the client to reset the "document view", to its original state.
        PartialContent                  = HttpStatusCode.PartialContent,                // = 206,   Successfully fulfilling a range request .
        MultiStatus                     = HttpStatusCode.MultiStatus,                   // = 207,   Success, returning multiple elements containing 200, 300, 400, and 500 series status codes (WebDAV only)
        AlreadyReported                 = HttpStatusCode.AlreadyReported,               // = 208,   Success, used in the above response elements (WebDAV only)
        IMUsed                          = HttpStatusCode.IMUsed,                        // = 226,   Success, returning "difference" to sent data.

        // 3xx(Redirection) : Further action needs to be taken in order to complete the request
        MultipleChoices                 = HttpStatusCode.MultipleChoices,               // = 300,   The requested information has multiple representations, select a variant in the Location header and resend. C# alias: HttpStatusCode.Ambiguous
        MovedPermanently                = HttpStatusCode.MovedPermanently,              // = 301,   The target resource has been moved permanently to the URI specified in the Location header, resend GET with this URI. C#/.NET alias: HttpStatusCode.Moved
        Found                           = HttpStatusCode.Found,                         // = 302,   The target resource has been moved temporarily to the URI specified in the Location header, resend GET with this URI. C#/.NET alias: HttpStatusCode.Redirect
        SeeOther                        = HttpStatusCode.SeeOther,                      // = 303,   Server redirects the client to the URI specified in the Location header as the result of a POST, resend GET with this URI. C#/.NET alias: HttpStatusCode.RedirectMethod
        NotModified                     = HttpStatusCode.NotModified,                   // = 304,   Server reports to the client that cached copy is up to date. The contents of the resource are not transferred.
        //UseProxy                      = HttpStatusCode.UseProxy,                      // = 305,   [deprecated] The request should use the proxy server at the URI specified in the Location header.
        //Unused                        = HttpStatusCode.Unused,                        // = 306,   [deprecated][reserved]
        TemporaryRedirect               = HttpStatusCode.TemporaryRedirect,             // = 307,   The target resource has been moved temporarily to the URI specified in the Location header, resend same method with this URI. C#/.NET alias: HttpStatusCode.RedirectKeepVerb
        PermanentRedirect               = HttpStatusCode.PermanentRedirect,             // = 308,   The target resource has been moved permanently to the URI specified in the Location header, resend same method with this URI.

        // 4xx(Client Error) : The request contains bad syntax or cannot be fulfilled
        BadRequest                      = HttpStatusCode.BadRequest,                    // = 400,   The request could not be understood by the server. (Sent when no other error is applicable, or if the exact error is unknown or does not have its own error code)
        Unauthorized                    = HttpStatusCode.Unauthorized,                  // = 401,   The requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication.
        //PaymentRequired               = HttpStatusCode.PaymentRequired,               // = 402,   [reserved]
        Forbidden                       = HttpStatusCode.Forbidden,                     // = 403,   The server understood the request but refuses to fulfill it. Do not resend with same credentials.
        NotFound                        = HttpStatusCode.NotFound,                      // = 404,   The requested resource does not exist (at the moment).
        MethodNotAllowed                = HttpStatusCode.MethodNotAllowed,              // = 405,   The request method is not supported by the target resource. The Allow header in response contains a list of supported methods.
        NotAcceptable                   = HttpStatusCode.NotAcceptable,                 // = 406,   The target resource does not have a representation that would be acceptable to the client, indicated with Accept headers of request. Response contains a list of available representations + resource identifiers.
        ProxyAuthenticationRequired     = HttpStatusCode.ProxyAuthenticationRequired,   // = 407,   The requested proxy requires authentication. The Proxy-authenticate header contains the details of how to perform the authentication.
        RequestTimeout                  = HttpStatusCode.RequestTimeout,                // = 408,   The client did not send a request within the server time limit, request may be resent.
        Conflict                        = HttpStatusCode.Conflict,                      // = 409,   A conflict with the state of the target resource detected, response contains info on the source of the conflict so that it can be resolved in a resent request.
        Gone                            = HttpStatusCode.Gone,                          // = 410,   The requested resource was removed.
        LengthRequired                  = HttpStatusCode.LengthRequired,                // = 411,   The required Content-length header is missing in request, - add header and resend.
        PreconditionFailed              = HttpStatusCode.PreconditionFailed,            // = 412,   One or more conditions given in the request headers evaluated to false.
        RequestEntityTooLarge           = HttpStatusCode.RequestEntityTooLarge,         // = 413,   The request content is too large. A Retry-After header in response, if present, indicates that it is temporary.
        RequestUriTooLong               = HttpStatusCode.RequestUriTooLong,             // = 414,   The URI is too long. This can occur when a client sent a GET request with long query information instead of a POST request.
        UnsupportedMediaType            = HttpStatusCode.UnsupportedMediaType,          // = 415,   The request content is in a format not supported by used method. See an Accept-Encoding or an Accept header in response.
        RequestedRangeNotSatisfiable    = HttpStatusCode.RequestedRangeNotSatisfiable,  // = 416,   The set of ranges in the request's Range header is invalid. See a Content-Range header in response for valid ranges.
        ExpectationFailed               = HttpStatusCode.ExpectationFailed,             // = 417,   The request's Expect header field (Section 10.1.1) could not be met by the server.
        MisdirectedRequest              = HttpStatusCode.MisdirectedRequest,            // = 421,   The request was directed at a server that is not able to produce a response.
        UnprocessableEntity             = HttpStatusCode.UnprocessableEntity,           // = 422,   The request was well-formed but had semantic errors. C#/.NET alias: HttpStatusCode.UnprocessableContent
        Locked                          = HttpStatusCode.Locked,                        // = 423,   The source or destination resource is locked.
        FailedDependency                = HttpStatusCode.FailedDependency,              // = 424,   The used method failed because the requested action depended on another action and that action failed.
        UpgradeRequired                 = HttpStatusCode.UpgradeRequired,               // = 426,   The client should switch to a different protocol (e.g. TLS/1.0). See Upgrade header in the response indicating the required protocol(s).
        PreconditionRequired            = HttpStatusCode.PreconditionRequired,          // = 428,   The server requires the request to be conditional (RFC 6585).
        TooManyRequests                 = HttpStatusCode.TooManyRequests,               // = 429, ! The user has sent too many requests in a given amount of time ("rate limiting") (RFC 6585). The response SHOULD include details explaining the condition.
        RequestHeaderFieldsTooLarge     = HttpStatusCode.RequestHeaderFieldsTooLarge,   // = 431,   The server is unwilling to process the request because its header fields are too large (RFC 6585).
        UnavailableForLegalReasons      = HttpStatusCode.UnavailableForLegalReasons,    // = 451,   The server is denying access to the resource as a consequence of a legal demand (RFC 7725).

        // 5xx (Server Error): The server failed to fulfill an apparently valid request
        InternalServerError             = HttpStatusCode.InternalServerError,           // = 500,   A generic error has occurred on the server.
        NotImplemented                  = HttpStatusCode.NotImplemented,                // = 501,   The server does not recognize the request method and is not capable of supporting it for any resource.
        BadGateway                      = HttpStatusCode.BadGateway,                    // = 502,   An intermediate proxy server received a bad response from another proxy or the origin server.
        ServiceUnavailable              = HttpStatusCode.ServiceUnavailable,            // = 503,   The server is temporarily unavailable, usually due to high load or maintenance. A Retry-After header, if present, suggests a time delay to wait before retrying the request.
        GatewayTimeout                  = HttpStatusCode.GatewayTimeout,                // = 504,   A response from an upstream server timed out.
        HttpVersionNotSupported         = HttpStatusCode.HttpVersionNotSupported,       // = 505,   The server does not support the major version of HTTP that was used in the request message, the response contains information on what protocols are supported.
        VariantAlsoNegotiates           = HttpStatusCode.VariantAlsoNegotiates,         // = 506,   Not proper endpoint in the negotiation process..
        InsufficientStorage             = HttpStatusCode.InsufficientStorage,           // = 507,
        LoopDetected                    = HttpStatusCode.LoopDetected,                  // = 508,   The server encountered an infinite loop while processing a WebDAV request. 
        NotExtended                     = HttpStatusCode.NotExtended,                   // = 510,   Further extensions to the request are required for the server to fulfill it.
        NetworkAuthenticationRequired   = HttpStatusCode.NetworkAuthenticationRequired, // = 511    The client needs to authenticate to gain network access (RFC 7725). The response SHOULD contain a link to a resource that allows the user to submit credentials.

        // Extended codes for internal Client failures, raised by Unity Web or CommonServices exceptions

        UnhandledException              = 666,  //      No specific handler for an Exception (no specific catch implemented), or filtered (grouped) with the below 2 error codes - value assigned Historically ;)
        UnhandledWebFailure             = 667,  //      No specific handler for a caught WebException
        UnhandledServiceFailure         = 668,  //      No specific handler for a caught Unity.Services.Core.RequestFailedException
        UnknownErrorCode                = 669,  //      Given HttpStatusCode not listed in this Enum, or filtered (grouped) with the below 2 error codes.
        UnknownWebError                 = 670,  //      Translated from WebExceptionStatus.UnknownError 
        UnknownServiceError             = 671,  //      Translated from Unity.Services.Core.CommonErrorCodes.Unknown

        ProtocolError                   = 996,  //      WebExceptionStatus.ProtocolError or filtered (grouped) HTTP (REST) or HTTP error codes
        Timeout                         = 997,  // !    A Server (proxy) or a Client Service timed out => retry with increased timeout = if (failure = abort)
        SerializationError              = 998,  //      Bug in Server serializer or Client deserializer

        NoConnection                    = 999   // !    The client failed to connect => abort.
    }
    
    public static class StatusCode
    {
        public static bool Succeeded(in int value)
        {
            return value >= (int)ResponseStatusCode.OK && value < (int)ResponseStatusCode.MultipleChoices;
        }

        public static bool NeedsFurtherActions(in int value)
        {
            return value >= (int)ResponseStatusCode.MultipleChoices && value < (int)ResponseStatusCode.BadRequest;
        }

        public static bool Failed(in int value)
        {
            return value >= (int)ResponseStatusCode.BadRequest;
        }

        public static bool IsRequestFailure(in int value)
        {
            return value >= (int)ResponseStatusCode.MultipleChoices && value < (int)ResponseStatusCode.InternalServerError;
        }

        public static bool IsServerFailure(in int value)
        {
            return value >= (int)ResponseStatusCode.InternalServerError && value < (int)ResponseStatusCode.UnhandledException;
        }

        public static bool IsClientFailure(in int value)
        {
            return value >= (int)ResponseStatusCode.UnhandledException;
        }

        public static bool IsHttpStatusCode(in int value)
        {
            return (System.Enum.IsDefined(typeof(HttpStatusCode), value));
        }

        public static HttpStatusCode? ToHttpStatusCode(in int value)
        {
            if (IsHttpStatusCode(value))
                return (HttpStatusCode)(object)value;
            return null;
        }

        public static ResponseStatusCode ToResponseStatusCode(in HttpStatusCode value)
        {
            return (ResponseStatusCode)(object)value;
        }
    }
}