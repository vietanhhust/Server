using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryItemController : ControllerBase
    {
        private EntityCRUDService entityCRUD;
        private ClientManagerContext context;
        public CategoryItemController(EntityCRUDService entityCRUD, ClientManagerContext context)
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
        }

        //Lấy tât cả item về
        [HttpGet]
        public IActionResult getAll()
        {
            return Ok(this.entityCRUD.GetAll<CategoryItem>().Select(item=> new { 
                item.Id,
                item.CategoryId, 
                item.CategoryItemName, 
                item.UnitPrice, 
                item.ImageUrl
            }).ToList());
        }

        // Lấy chi tiết một danh mục
        [HttpGet("{id}")]
        public IActionResult getDetail(int id)
        {
            CategoryItem item = this.entityCRUD.GetAll<CategoryItem>().
                FirstOrDefault(item => item.Id == id);
            if(item is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy sản phẩm",
                    Status = 400
                });
            }
            else
            {
                return Ok(item);
            }
        }

        // Thêm mới một sản phẩm
        [HttpPost]
        public ObjectResult addCategoryItem([FromBody] CategoryItem item)
        {
            item.Id = null;
            if(this.entityCRUD.Add<CategoryItem>(item).Result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Lỗi nhập liệu, vui lòng nhập đầy đủ các trường",
                    Status = 400
                });
            }
        }

        // Update một sản phẩm
        [HttpPut("{id}")]
        public ObjectResult updateCategoryItem(int id, [FromBody] CategoryItem item)
        {
            item.Id = id;
            CategoryItem itemFound = this.context.CategoryItems.FirstOrDefault(x => x.Id == id);
            if(itemFound is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Không tìm thấy sản phẩm",
                    Status = 400
                });
            }
            else
            {
                if(this.entityCRUD.Update<CategoryItem, CategoryItem>(item, itemFound).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Messege = "Vui lòng nhập đủ các trường",
                        Status = 400
                    });
                }
            }

            // Xóa một sản phẩm
            
            
        }
        [HttpDelete("{id}")]
        public IActionResult deleteItem(int id)
        {
            CategoryItem item = this.context.CategoryItems.FirstOrDefault(x => x.Id == id); 
            if(item is null)
            {
                return BadRequest(new ErrorModel
                {
                    Messege = "Sản phẩm không tồn tại", 
                    Status = 400
                }); 

            }else
            {
                if (this.entityCRUD.Delete<CategoryItem>(item).Result)
                {
                    return Ok(true);
                }
                else
                {
                    return BadRequest(new ErrorModel
                    {
                        Status = 400,
                        Messege = "Thao tác không thành công"
                    });
                }
            }
        }
    }
}
