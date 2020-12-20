using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerAPI.Controllers.CURDs;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Utilities;

namespace ServerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private ClientManagerContext context;
        private IMapper mapper;
        private EntityCRUDService entityCRUD;
        public CategoryController(ClientManagerContext context, IMapper mapper, EntityCRUDService entityCRUD)
        {
            this.context = context;
            this.mapper = mapper;
            this.entityCRUD = entityCRUD;
        }

        // Lấy về danh mụcs
        [HttpGet]
        public IActionResult getAll()
        {
            return Ok(this.entityCRUD.GetAll<Category>().
                Select(cate => new
                {
                    cate.Id,
                    cate.CategoryName, 
                    cate.Description
                }).ToList());
        }

        // Lấy về 1 danh mục bằng Id
        [HttpGet("{id}")]
        public IActionResult getDetail(int? id)
        {
            var cate = this.entityCRUD.GetAll<Category>().
                Where(x => x.Id == id).Select(item=> new { 
                    item.Id, 
                    item.CategoryName
                }).FirstOrDefault();
            if (cate is null)
            {
                return BadRequest(new ErrorModel
                {
                    Status = 400,
                    Messege = "Không tìm thấy danh mục"
                });
            }
            return Ok(cate);
        }

        // Thêm mới danh mục
        [HttpPost]
        public ObjectResult AddCategory([FromBody] Category category)
        {
            category.Id = null;
            
            if(this.entityCRUD.Add<Category>(category).Result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Vui lòng nhập các trường", 
                    Status = 400
                });
            }
        }

        // Sửa danh mục
        [HttpPut("{id}")]
        public ObjectResult UpdateCategory(int? id, [FromBody] Category category)
        {
            if(id is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tham số không hợp lệ",
                    Status = 400
                });
            }
            Category cate = this.context.Categories.FirstOrDefault(item => item.Id == id);
            if(cate is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy danh mục",
                    Status = 400
                });
            }
            else
            {
                category.Id = id;
                if(this.entityCRUD.Update<Category, Category>(category, cate).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel { 
                        Messege="Vui lòng nhập các trường bằng dữ liệu hợp lệ", 
                        Status=400
                    });
                }
            }
        }

        // Xóa danh mục
        [HttpDelete("{id}")]
        public async Task<ObjectResult> Delete(int? id)
        {
            if (id is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Tham số không hợp lệ",
                    Status = 400
                });
            }
            Category cate = this.context.Categories.FirstOrDefault(item => item.Id == id);
            if (cate is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy danh mục",
                    Status = 400
                });
            }
            else
            {
                await this.entityCRUD.Delete<Category>(cate);
                return Ok(true);
            }
        }
    }
}
