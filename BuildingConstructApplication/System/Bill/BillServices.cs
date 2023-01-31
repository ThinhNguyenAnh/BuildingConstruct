﻿using Data.DataContext;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ViewModels.BillModels;

namespace Application.System.Bill
{
    public class BillServices : IBillServices
    {
        private readonly BuildingConstructDbContext _context;
        private IHttpContextAccessor _accessor;
        public BillServices(BuildingConstructDbContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task<bool> CreateBill(Guid userID, BillDTO requests)
        {
            Claim identifierClaim = _accessor.HttpContext.User.FindFirst("UserID");
            var usID = identifierClaim.Value;
            var storeID = _context.Users.Where(x => x.Id.ToString().Equals(userID)).FirstOrDefault().MaterialStoreID;
            var contracID = _context.Users.Where(x => x.Id.ToString().Equals(userID)).FirstOrDefault().ContractorId;
            var bill = new Data.Entities.Bill()
            {
                Note = requests.Notes,
                Status = requests.Status,
                StartDate = requests.StartDate,
                EndDate = requests.EndDate,
                TotalPrice = requests.TotalPrice,
                ContractorId = (int)(contracID == null ? null : contracID),
                StoreID = (int)(storeID == null ? null : storeID),
            };
            await _context.AddAsync(bill);
            var rs = await _context.SaveChangesAsync();
            if (rs > 0)
            {
            
                foreach (var item in requests.ProductBill)
                {
                    var billDetail = new BillDetail();
                    billDetail.BillId = bill.Id;
                    billDetail.ProductID = item.ProductId;
                    billDetail.Quantity = item.Quantity;
                    billDetail.Price = item.Price;
                    _context.BillDetails.Add(billDetail);
                    _context.SaveChanges();
                }
                if (requests.SmallBill != null && requests.BillType!=0)
                {
                    var smallBill = new Data.Entities.SmallBill()
                    {
                        
                    };
                }
                return true;
            }
            return false;
        }
    }
}
