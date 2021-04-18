using System;
using CarRentalApp.Core.domain;

namespace CarRentalApp.Core.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {

    }
}