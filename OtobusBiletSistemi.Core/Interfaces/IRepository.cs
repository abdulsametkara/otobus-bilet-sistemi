using OtobusBiletSistemi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);

        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task DeleteAsync(T entity);
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FindSingleAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
        Task<bool> ExistsAsync(int id);
    }
}
