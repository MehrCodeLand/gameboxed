using Api.Leyer.DTOs;
using Api.Leyer.Strcuts;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Leyer.Interfaces
{
    public interface IUserRepository
    {
        Task<MyResponse<bool>> RegisterASync(UserRegisterDto dto);
    }
}
