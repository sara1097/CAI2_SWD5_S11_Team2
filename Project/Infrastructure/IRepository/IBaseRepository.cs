﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IRepository
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetById(int id); // task for asynchronous 
        Task<List<T>> GetAll();
        Task<List<T>> Search(Expression<Func<T, bool>> predicate);
        Task Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<List<T>> GetAll(Expression<Func<T, bool>> criteria = null, Expression<Func<T, object>>[] includes = null);
        Task<T> GetFirstOrDefault(Expression<Func<T, bool>> filter);

    }
}
