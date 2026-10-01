using System;

namespace Blackhole.SharedKernel.Contract
{
    /// <summary>
    /// Represents an operation that either succeeded or failed without returning
    /// a value or exposing failure details.
    /// </summary>
    public sealed class Result
    {
        private static readonly Result SuccessfulResult = new Result(isSuccess: true);
        private static readonly Result FailedResult = new Result(isSuccess: false);

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public static Result Success()
        {
            return SuccessfulResult;
        }

        public static Result Failure()
        {
            return FailedResult;
        }
    }

    /// <summary>
    /// Represents an operation that either succeeded without returning a value
    /// or failed with a module-specific error.
    /// </summary>
    /// <typeparam name="TError">The module-specific failure type.</typeparam>
    public sealed class Result<TError> 
    {
        private static readonly Result<TError> SuccessfulResult =
            new Result<TError>(isSuccess: true, error: default);

        private readonly TError _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public TError Error
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

        private Result(bool isSuccess, TError error)
        {
            if (!isSuccess && ReferenceEquals(error, null))
            {
                throw new ArgumentNullException(nameof(error), "A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            _error = error;
        }

        public static Result<TError> Success()
        {
            return SuccessfulResult;
        }

        public static Result<TError> Failure(TError error)
        {
            return new Result<TError>(isSuccess: false, error);
        }
    }

    /// <summary>
    /// Represents an operation that either succeeded with a value or failed
    /// with a module-specific error.
    /// </summary>
    /// <typeparam name="TValue">The value produced by a successful operation.</typeparam>
    /// <typeparam name="TError">The module-specific failure type.</typeparam>
    public sealed class Result<TValue, TError>
    {
        private readonly TValue _value;
        private readonly TError _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException("A failed result does not contain a value.");
                }

                return _value;
            }
        }

        public TError Error
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

        private Result(bool isSuccess, TValue value, TError error)
        {
            if (!isSuccess && ReferenceEquals(error, null))
            {
                throw new ArgumentNullException(nameof(error), "A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            _value = value;
            _error = error;
        }

        public static Result<TValue, TError> Success(TValue value)
        {
            return new Result<TValue, TError>(isSuccess: true, value, error: default);
        }

        public static Result<TValue, TError> Failure(TError error)
        {
            return new Result<TValue, TError>(isSuccess: false, value: default, error);
        }
    }
}