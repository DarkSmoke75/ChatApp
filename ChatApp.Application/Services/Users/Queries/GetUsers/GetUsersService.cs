using ChatApp.Application.Interfaces.Contexts;
using ChatApp.Common.Dto;

namespace ChatApp.Application.Services.Users.Queries.GetUsers
{
    public class GetUsersService : IGetUsersService
    {
        private readonly IDatabaseContext _context;
        private readonly IUserContext _userContext;
        public GetUsersService(IDatabaseContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public ResultDto<List<GetUsersResultDto>> Execute()
        {
            var currentUserId = _userContext.UserId;
            var users = _context.Users.Where(p=>p.Id != currentUserId).Select(p => new GetUsersResultDto
            {
                UserId = p.Id,
                Username = p.Username,
                DisplayName = p.DisplayName
            }).ToList();
            return new ResultDto<List<GetUsersResultDto>>
            {
                Data = users,
                IsSuccess = true,
                Message= "Users retrieved successfully"
            };
        }
    }
}
