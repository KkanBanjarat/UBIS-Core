using Microsoft.EntityFrameworkCore;
using UBIS.Access.Application.Dtos;
using UBIS.Access.Application.Interfaces;
using UBIS.Access.Domain.Entities;
using UBIS.Access.Infrastructure.Data;

namespace UBIS.Access.Infrastructure.Services;

public class RoleMemberService : IRoleMemberService
{
    private readonly AccessDbContext _context;

    public RoleMemberService(AccessDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleMemberDto>> GetByRoleIdAsync(Guid roleId)
    {
        return await (
            from ur in _context.Set<TbUserRole>().AsNoTracking()
            join u in _context.Set<TbUser>().AsNoTracking() on ur.UserId equals u.Id
            where ur.RoleId == roleId && !ur.IsDelete
            orderby u.DisplayName
            select new RoleMemberDto
            {
                UserRoleId = ur.Id,
                UserId = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                EmployeeCode = u.EmployeeCode,
                IsActive = u.IsActive,
                Scope = ur.Scope
            }).ToListAsync();
    }
}