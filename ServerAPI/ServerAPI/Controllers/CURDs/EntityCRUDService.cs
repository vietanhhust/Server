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

        // Add List Entity
        public async Task<bool> AddRange<T>(List<T> list) where T: class
        {
            var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await transaction.CreateSavepointAsync("Before_Add_List");
                this.context.Set<T>().AddRange(list);
                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Add_List");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync("Before_Add_List");
                await transaction.ReleaseSavepointAsync("Before_Add_List");
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

        // Put List Entity 
        public async Task<bool> UpdateRange<T>(List<T> list) where T: class{
            var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await transaction.CreateSavepointAsync("Before_Update_List");
                this.context.Set<T>().UpdateRange(list);
                await this.context.SaveChangesAsync(); 
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Update_List");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync("Before_Update_List");
                await transaction.ReleaseSavepointAsync("Before_Update_List");
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

        // Delete List Entity 
        public async Task<bool> DeleteRange<T>(List<T> list) where T : class
        {
            var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                await transaction.CreateSavepointAsync("Before_Delete_List");
                this.context.Set<T>().RemoveRange(list);
                await this.context.SaveChangesAsync();
                await transaction.CommitAsync();
                await transaction.ReleaseSavepointAsync("Before_Delete_List");
                return await Task<bool>.Factory.StartNew(() => true);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync("Before_Delete_List");
                await transaction.ReleaseSavepointAsync("Before_Delete_List");
                return await Task<bool>.Factory.StartNew(() => false);
            }
        }

        //Excute Command
        public bool ExcuteCommand(string command, object param= null)
        {
            if(param is null)
            {
                try
                {
                    this.context.Database.ExecuteSqlRaw(command);
                    this.context.SaveChanges();
                    return true;
                }catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    this.context.Database.ExecuteSqlRawAsync(command, param);
                    this.context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        //Excute CommandAsync
        public Task<bool> ExcuteCommandAsync(string command, object param = null)
        {
            if (param is null)
            {
                try
                {
                    this.context.Database.ExecuteSqlRawAsync(command);
                    this.context.SaveChanges();
                    return Task<bool>.Factory.StartNew(() => true);
                }
                catch
                {
                    return Task<bool>.Factory.StartNew(() => false);
                }
            }
            else
            {
                try
                {
                    this.context.Database.ExecuteSqlRawAsync(command, param);
                    this.context.SaveChanges();
                    return Task<bool>.Factory.StartNew(() => true);
                }
                catch
                {
                    return Task<bool>.Factory.StartNew(() => false);
                }
            }
        }

        // UntrackDbContet 
        public void UnTrackContext()
        {
            this.context.ChangeTracker.Clear(); 
        }
        
    }
}
