using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ServerAPI.Model.Errors;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using Microsoft.AspNetCore.Mvc.Controllers;
using ServerAPI.Model.StaticModel;

namespace ServerAPI.Utilities
{
    public class GlobalFilterAttribute : ActionFilterAttribute
    {
        private EntityCRUDService entityCRUD;
        public GlobalFilterAttribute(EntityCRUDService entityCRUD)
        {
            this.entityCRUD = entityCRUD; 
        }

        // Tiền xử lý request đến.

        // Sử dụng nhóm quyền lấy từ Header + template + method---> FrontendCode để chặn request. 
        // Nếu muốn bỏ qua cái này, dùng custom Attribute [IgnorFilter....]
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //var controllerMetaData = (ControllerActionDescriptor)context.ActionDescriptor;
            //if (controllerMetaData.MethodInfo.CustomAttributes.
            //    Any(x => x.AttributeType == typeof(IgnoreFilterAtrribute)))
            //{
            //    return;
            //}
            //else
            //{
            //    context.Result = new ForbidResult(); 
            //}

#if (!DEBUG)
            // Nếu là APi Free thì không chặn. 
            var controllerMetaData = (ControllerActionDescriptor)context.ActionDescriptor;
            if (controllerMetaData.MethodInfo.CustomAttributes.
                Any(x => x.AttributeType == typeof(IgnoreFilterAtrribute)))
            {
                return;
            }

            // Triển khai lọc filter.
            // Nếu API có Frontend = 0, thì Forbiden. ( Bảo vệ server ). Lấy method và template của request để xem nhóm quyền frontend
            var roleUrlFound = this.entityCRUD.
                GetAll<Role>(x => x.Template.ToLower() == context.ActionDescriptor.AttributeRouteInfo.Template.ToLower() &&
                x.Method.ToLower() == context.HttpContext.Request.Method).FirstOrDefault();
            if (roleUrlFound.FrontendCode is null || roleUrlFound.FrontendCode == 0)
            {
                context.Result = new ForbidResult();
            }

            // Tìm quyền frontend để lấy Id. 
            var frontEndRoleFound = this.entityCRUD.GetAll<FrontendRole>(x => x.FrontendCode == roleUrlFound.FrontendCode).FirstOrDefault();
            var groupRoleId = 0; // Cái này lấy từ header của request gửi lên. 

            var frontEndRoleId = frontEndRoleFound.Id;

            // Lấy quyền đọc ghi sửa xóa với 1 group Role và mã code. 
            var roleActive = this.entityCRUD.GetAll<RoleActive>(x => x.FrontendRoleId == frontEndRoleId && x.GroupRoleId == groupRoleId).FirstOrDefault();
            switch (context.HttpContext.Request.Method.ToLower())
            {
                case "get":
                    {
                        if (!roleActive.IsView.Value)
                        {
                            context.Result = new ForbidResult();
                        }
                        break;
                    }
                case "post":
                    {
                        if (!roleActive.IsCreate.Value)
                        {
                            context.Result = new ForbidResult();
                        }
                        break;
                    }
                case "put":
                    {
                        if (!roleActive.IsPut.Value)
                        {
                            context.Result = new ForbidResult();
                        }
                        break;
                    }
                case "delete":
                    {
                        if (!roleActive.IsDelete.Value)
                        {
                            context.Result = new ForbidResult();
                        }
                        break;
                    }

            }
#endif
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //var a = (ControllerActionDescriptor)context.ActionDescriptor; 
            //a.MethodInfo.CustomAttributes.Any(x=>x.AttributeType==typeof(IgnoreAtrribute))
        }

    }
}
