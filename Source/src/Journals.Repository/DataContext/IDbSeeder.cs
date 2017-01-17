using System;
using System.Threading.Tasks;

namespace Journals.Repository.DataContext
{
    public interface IDbSeeder
    {
        Task Seed();

    }
}