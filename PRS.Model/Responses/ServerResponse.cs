namespace PRS.Model.Responses
{
    public class ServerResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public static ServerResponse Ok() => new() { Success = true };
        public static ServerResponse Fail(string error) => new() { Success = false, Message = error };
    }

    public class ServerResponse<T> : ServerResponse
    {
        public T? Data { get; set; }

        public static ServerResponse<T> Ok(T data) => new()
        {
            Success = true,
            Data = data
        };

        public new static ServerResponse<T> Fail(string error) => new()
        {
            Success = false,
            Message = error
        };
    }
}
