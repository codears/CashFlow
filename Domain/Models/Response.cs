namespace CashReportApi.Domain
{
    public record class Response
    {
        public static readonly Response Successful = new();
        public Error? Error { get; init; }
        public static implicit operator Response(Error error)
        {
            return new Response() { Error = error };
        }
    }

    public record class Response<T> : Response
    {
        public T Value { get; init; }

        public static implicit operator Response<T>(T value)
        {
            return new Response<T>() { Value = value };
        }

        public static implicit operator Response<T>(Error error)
        {
            return new Response<T>() { Error = error };
        }
    }

}
