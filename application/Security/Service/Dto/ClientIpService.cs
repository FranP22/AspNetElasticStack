using Microsoft.AspNetCore.Http;
using Security.Service.Interface;

namespace Security.Service.Dto
{
    public class ClientIpService : IClientIpService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public ClientIpService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string? GetIp()
        {
            return _contextAccessor.HttpContext?
                .Items["ClientIp"]?
                .ToString();
        }
    }
}
