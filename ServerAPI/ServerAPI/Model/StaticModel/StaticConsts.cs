﻿using ServerAPI.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ServerAPI.Model.StaticModel
{
    public static class StaticConsts
    {

        // Nhóm quyền không thể xóa: Default và Admin
        public static List<int> UndeletableGroupRole = new List<int>()
        {
            1, 2
        };

        // Nhóm máy không thể xóa: Default. 
        public static List<int> UndeletableGroupClient = new List<int>()
        {
            1
        };

        public static Dictionary<string, string> Method => new Dictionary<string, string>()
        {
            {"GET", "1" },
            {"POST", "2" },
            {"PUT", "3" },
            {"DELETE", "4" },
        };

        // List này để duy trì các connection, để gửi và nhận thông tin
        static public List<ClientConnect> ConnectedClient = new List<ClientConnect>();
        static public List<AdminPageConnect> AdminConnected = new List<AdminPageConnect>();

        static public WebSocket Socket; 
    }

    //Enum Method
    public enum MethodEnum
    {
        GET = 1,
        POST = 2,
        PUT = 3,
        DELETE = 4
    }

    public class APIDescriptionModel
    {
        public string Template { get; set; }
        public string Method { get; set; }
        public int? FrontendCode { get; set; }
    }

    // Đánh nhãn Attribute này vào API để làm public những API cần. 
    public class IgnoreFilterAtrribute: System.Attribute
    {

    }

    public class LoginModel
    {
        public string UserName { get; set; }
        
        // Password dùng toLower(); 
        public string Password { get; set; }
    }

    // Đại diện cho một máy đang connect
    public class ClientConnect
    {
        public string ConnectionId { get; set; }
        public int ClientId { get; set; }
        public Account Account { get; set; }
        public long? TimeLogin { get; set; }
        public int? ElapsedTime { get; set; }
    }

    // Đại diện cho 1 admin page đang connect
    public class AdminPageConnect
    {
        public string ConnectionId { get; set; }
    }

    public class BalanceModel
    {
        public int Money { get; set;}
        public int AccountId { get; set; }
    }

    public class KickStart
    {
        public KickStart(ClientManagerContext context)
        {
          
        }
    }

   

    
}

// Dùng để xử lý list category item. 
public class CategoryOrder
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int Quantity { get; set; }
    public int UnitPrice { get; set; }
}

// Dùng để xử lý Order từ người dùng. 
public class OrderCreating
{
    public int? AccountId { get; set; }
    public int AdminId { get; set; }
    public long CreatedTime { get; set; }
    public int? ClientId { get; set; }
    public List<CategoryOrder> ListCategory { get; set; }
}

// Dùng để lấy ra lịch sử nạp, trừ tiền.
public class HistoryBalanceQueryModel
{
    public long? fromDate { get; set; }
    public long? toDate { get; set; }
    public int? accountId { get; set; }
    public bool typeChange { get; set; }
}


// Dùng để query ra lịch sử gọi đồ. 
public class HistoryOrderQueryModel
{
    public long? fromDate { get; set; }
    public long? toDate { get; set; }
    public int? accountId { get; set; }
    public int? adminId { get; set;  }
    public int? clientId { get; set; }
}

// Dùng để gửi dữ liệu order và order details 

