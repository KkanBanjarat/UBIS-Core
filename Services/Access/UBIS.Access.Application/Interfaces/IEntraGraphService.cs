using UBIS.Access.Application.Dtos;

namespace UBIS.Access.Application.Interfaces;

public interface IEntraGraphService
{
    Task<List<UserDto>> GetUsersAsync();
}