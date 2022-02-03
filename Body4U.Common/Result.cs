namespace Body4U.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class Result
    {
        private readonly List<string> errors;

        public Result
            (bool succeeded,
            List<string> errors,
            HttpStatusCode statusCode)
        {
            this.Succeeded = succeeded;
            this.errors = errors;
            this.StatusCode = statusCode;
        }

        public bool Succeeded { get; }

        public List<string> Errors
            => this.Succeeded
                ? new List<string>()
                : this.errors;

        public HttpStatusCode StatusCode { get; }

        public static Result Success
            => new Result(true, new List<string>(), new HttpStatusCode());

        public static Result Failure(HttpStatusCode statusCode, IEnumerable<string> errors)
            => new Result(false, errors.ToList(), new HttpStatusCode());

        public static Result Failure(HttpStatusCode statusCode, string error)
            => new Result(false, new List<string> { error }, statusCode);

        public static Result Failure(HttpStatusCode statusCode)
            => new Result(false, new List<string>(), statusCode);
    }

    public class Result<TData> : Result
    {
        private readonly TData data;

        private Result(
            bool succeeded,
            TData data,
            List<string> errors,
            HttpStatusCode statusCode)
            : base(succeeded, errors, new HttpStatusCode())
            => this.data = data;

        public TData Data
            => this.data;

        public static Result<TData> SuccessWith(TData data)
            => new Result<TData>(true, data, new List<string>(), new HttpStatusCode());

        public new static Result<TData> Failure(HttpStatusCode statusCode, IEnumerable<string> errors)
            => new Result<TData>(false, default!, errors.ToList(), statusCode);

        public new static Result<TData> Failure(HttpStatusCode statusCode, string error)
            => new Result<TData>(false, default!, new List<string> { error }, statusCode);

        public new static Result<TData> Failure(HttpStatusCode statusCode)
            => new Result<TData>(false, default!, new List<string>(), statusCode);
    }
}
