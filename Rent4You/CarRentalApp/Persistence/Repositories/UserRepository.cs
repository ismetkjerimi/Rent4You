using System;
using CarRentalApp.Core.domain;
using CarRentalApp.Core.Repositories;

namespace CarRentalApp.Persistence.Repositories
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(CarRentalAppContext context) : base(context)
        {
        }

        public CarRentalAppContext CarRentalAppContext => Context as CarRentalAppContext;

    }
}