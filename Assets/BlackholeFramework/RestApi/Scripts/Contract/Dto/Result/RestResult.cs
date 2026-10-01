using System;
using System.Net;

namespace Blackhole.RestApi.Contract
{
    public sealed class RestResult
    {
        private readonly RestError _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public HttpStatusCode? StatusCode { get; }
        public string ResponseMessage { get; }

        public RestError Error
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("A successful result does not contain an error.");
                }

                return _error;
            }
        }

        private RestResult(bool isSuccess, HttpStatusCode? statusCode, RestError error, string responseMessage)
        {
            if (isSuccess && error != null)
            {
                throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
            }

            if (!isSuccess && error == null)
            {
                throw new ArgumentNullException(nameof(error), "A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            StatusCode = statusCode;
            ResponseMessage = responseMessage;
            _error = error;
        }

        public static RestResult Success(HttpStatusCode statusCode = HttpStatusCode.OK, string responseMessage = null)
        {
            return new RestResult(isSuccess: true, statusCode, error: null, responseMessage);
        }

        public static RestResult Failure(RestError error, HttpStatusCode? statusCode = null, string responseMessage = null)
        {
            return new RestResult(isSuccess: false, statusCode, error, responseMessage);
        }
    }

    public sealed class RestResult<T>
    {
        private readonly T _value;
        private readonly RestError _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public HttpStatusCode? StatusCode { get; }
        public string ResponseMessage { get; }

        public T Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("A failed result does not contain a value.");
                }

                return _value;
            }
        }

        public RestError Error
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("A successful result does not contain an error.");
                }

                return _error;
            }
        }

        private RestResult(bool isSuccess, HttpStatusCode? statusCode, T value, RestError error, string responseMessage)
        {
            if (isSuccess && error != null)
            {
                throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
            }

            if (!isSuccess && error == null)
            {
                throw new ArgumentNullException(nameof(error), "A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            StatusCode = statusCode;
            ResponseMessage = responseMessage;
            _value = value;
            _error = error;
        }

        public static RestResult<T> Success(T value, HttpStatusCode statusCode = HttpStatusCode.OK, string responseMessage = null)
        {
            return new RestResult<T>(isSuccess: true, statusCode, value, error: null, responseMessage);
        }

        public static RestResult<T> Failure(RestError error, HttpStatusCode? statusCode = null, string responseMessage = null)
        {
            return new RestResult<T>(isSuccess: false, statusCode, value: default, error, responseMessage);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<RestError, TResult> onFailure)
        {
            if (onSuccess == null)
                throw new ArgumentNullException(nameof(onSuccess));

            if (onFailure == null)
                throw new ArgumentNullException(nameof(onFailure));

            return IsSuccess ? onSuccess(_value) : onFailure(_error);
        }
    }
}
