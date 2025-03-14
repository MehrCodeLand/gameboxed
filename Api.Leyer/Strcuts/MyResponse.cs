using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Api.Leyer.Strcuts;

public struct MyResponse<T>
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("isError")]
    public bool IsError { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }

    // Success response
    public static MyResponse<T> Success(string message , T data ) =>
        new MyResponse<T> { Message = message, IsError = false , Data = data};

    // Error response
    public static MyResponse<T> Error(string message) =>
        new MyResponse<T> { Message = message, IsError = true, Data = default };
}
