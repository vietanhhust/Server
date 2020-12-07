using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.Model.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AutoMapper;

namespace ServerAPI.Controllers.CURDs
{
    public class EntityCRUDService
    {
        private ClientManagerContext context;
        private IMapper mapper; 
        public EntityCRUDService(ClientManagerContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        // Get all
        public List<T> GetAll<T>(Expression<Func<T, bool>> predicate = null) where T: class{
            if(predicate is null)
            {
                return this.context.Set<T>().ToList(); 
            }
            return this.context.Set<T>().Where(predicate).ToList();
        }

        // Add entity
        public async Task<bool> Add<T>(T t) where T: class
        {
            var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await transaction.CreateSavepointAsync("Before_Add");
                this.context.Set<T>().Add(t);
                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Add");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync("Before_Add");
                await transaction.ReleaseSavepointAsync("Before_Add");
                return await Task<bool>.Factory.StartNew(() => false);
            }
        }

        // Put entity
        public async Task<bool> Update<T, F>(T source, F destination) 
            where T: class 
            where F: class
        {
            this.mapper.Map<T, F>(source, destination);
            var transaction = this.context.Database.BeginTransaction();
            try
            {
                await transaction.CreateSavepointAsync("Before_Update");
                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Update");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                Console.WriteLine("co loi");
                await transaction.RollbackToSavepointAsync("Before_Update");
                await transaction.ReleaseSavepointAsync("Before_Update");
                return await Task<bool>.Factory.StartNew(() => false);
            }
        }

        //Delete entity
        public async Task<bool> Delete<T>(T t) where T: class
        {
            var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await transaction.CreateSavepointAsync("Before_Delete");
                this.context.Set<T>().Remove(t);
                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Delete");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync("Before_Delete");
                await transaction.ReleaseSavepointAsync("Before_Delete");
                return await Task<bool>.Factory.StartNew(() => false);
            }
        }
        
    }
}
