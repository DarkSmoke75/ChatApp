namespace Endpoint.Site.Models.Dtos.Common
{
    public class ApiResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
    public class ApiResultDto<T> : ApiResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
