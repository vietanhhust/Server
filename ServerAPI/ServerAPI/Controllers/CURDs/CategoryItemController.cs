using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ServerAPI.Model.Database;
using ServerAPI.Model.Errors;
using ServerAPI.Model.Hubs;

namespace ServerAPI.Controllers.CURDs
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryItemController : ControllerBase
    {
        private EntityCRUDService entityCRUD;
        private ClientManagerContext context;
        private IHubContext<ClientHub> hubContext;
        public CategoryItemController(EntityCRUDService entityCRUD, ClientManagerContext context, IHubContext<ClientHub> hubContext)
        {
            this.context = context;
            this.entityCRUD = entityCRUD;
            this.hubContext = hubContext;
        }

        //Lấy tât cả item về
        [HttpGet]
        public IActionResult getAll([FromQuery] string keyword)
        {
            if(keyword is null || keyword =="")
            {
                keyword = "";
            }
            return Ok(this.entityCRUD.GetAll<CategoryItem>(item=>item.CategoryItemName.ToLower().Contains(keyword.ToLower())).Select(item=> new { 
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
                this.hubContext.Clients.All.SendAsync("categoryChange");
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
                    this.hubContext.Clients.All.SendAsync("categoryChange");
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
                    this.hubContext.Clients.All.SendAsync("categoryChange");
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

        // Dùng để upload ảnh
        [HttpPost]
        [Route("image")]
        public IActionResult uploadImage()
        {
            var file = Request.Form.Files[0];
            Console.WriteLine(file.FileName); //abc.png
            var name = file.FileName;
            var buffer = "";
            if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\CategoryItemImage", name))){
                var nameArray = name.Split(".");
                for(int i = 0; i < nameArray.Length-1; i++)
                {
                    buffer += nameArray[i];
                }
                buffer += "copy.";
                buffer += nameArray[nameArray.Length - 1];
            }
            else
            {
                buffer = name;
            }
            var fileName = Path.GetFileName(buffer); ;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\CategoryItemImage", fileName);
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                    return Ok(new
                    {
                        fileName = buffer
                    });
                }
            }
            else
            {
                return BadRequest(new ErrorModel()
                {
                    Messege = "Lỗi gửi file"
                });
            }
            
        }
    }
}
