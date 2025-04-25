using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data;
using Infrastructure.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet; //get the table for this model(T) from the context

        // Constructor to initialize the DbContext and DbSet
        public BaseRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Implementing the methods of IBaseRepository
        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }


        public async Task<List<T>> GetAll() 
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<T>> Search(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task Add(T entity, Action<string> LogAction)
        {
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
            LogAction?.Invoke($"Entity of type {typeof(T).Name} added Successfully");
        }
        public async Task Update(T entity, Action<string> LogAction)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            LogAction?.Invoke($"Entity of type {typeof(T).Name} updated Successfully");
        }
        public async Task Delete(int id, Action<string> LogAction)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                LogAction?.Invoke($"Entity of type {typeof(T).Name} deleted Successfully");
            }
             
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

    }
}
