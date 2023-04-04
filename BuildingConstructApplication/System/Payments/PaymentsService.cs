﻿using Data.DataContext;
using Data.Entities;
using Data.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Pagination;
using ViewModels.Payment;
using ViewModels.Response;

namespace Application.System.Payments
{
    public class PaymentsService : IPaymentsService
    {
        private readonly BuildingConstructDbContext _context;
        private IHttpContextAccessor _accessor;
        public PaymentsService(BuildingConstructDbContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task<BaseResponse<RefundDTO>> CheckRefundPayment()
        {
            BaseResponse<RefundDTO> response = new();
            var userId = _accessor.HttpContext?.User?.FindFirst("UserID")?.Value.ToString();
            var query = await _context.Payments.Where(x => x.UserId.ToString().Equals(userId)).OrderByDescending(x => x.ExpireationDate).ToListAsync();
            var viewCheck = await _context.ContractorPosts.Where(x => x.CreateBy.ToString().Equals(userId)).ToListAsync();
            var n = await _context.Users.Where(x => x.BuilderId != null).CountAsync();
            var number = n * 0.2M;
            var flag = false;
            foreach (var item in viewCheck)
            {
                if (item.ViewCount < number)
                {
                    flag = true;
                }
            }
            if (flag == true)
            {
                var check = await _context.ContractorPosts.Include(x => x.AppliedPosts).Where(x => x.CreateBy.ToString().Equals(userId)).ToListAsync();
                foreach (var i in check)
                {
                    if (i.AppliedPosts.Count() < 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag == false)
            {
                if (query.Count == 0)
                {
                    response.Code = BaseCode.SUCCESS;
                    response.Message = BaseCode.SUCCESS_MESSAGE;
                    response.Data = null;
                }
                else
                {
                    if (query.First().ExpireationDate >= DateTime.Now)
                    {
                        response.Code = BaseCode.SUCCESS;
                        response.Message = BaseCode.SUCCESS_MESSAGE;
                        response.Data = new();
                        response.Data.IsOver = false;
                        response.Data.EndDate = query.First().ExpireationDate.ToString();
                    }
                    else if (query.First().ExpireationDate.AddMonths(1).Month != DateTime.Now.Month)
                    {
                        response.Code = BaseCode.SUCCESS;
                        response.Message = BaseCode.SUCCESS_MESSAGE;
                        response.Data = null;
                    }
                    else
                    {
                        query.First().ExtendDate = DateTime.Now.AddMonths(1);
                        _context.Payments.Update(query.First());
                        await _context.SaveChangesAsync();
                        response.Code = BaseCode.SUCCESS;
                        response.Message = BaseCode.SUCCESS_MESSAGE;
                        response.Data = new();
                        response.Data.IsOver = true;
                        response.Data.EndDate = query.First().ExpireationDate.ToString();
                    }
                }
            }
            return response;
        }

        public async Task<BasePagination<List<PaymentDTO>>> RefundList()
        {
            var response = new BasePagination<List<PaymentDTO>>();
            response.Data = new();
            var userId = _accessor.HttpContext?.User.FindFirst("UserID")?.Value.ToString();

            var query = await _context.Payments.Include(x=>x.Users).Where(x => x.UserId.ToString().Equals(userId) && x.IsRefund == true).ToListAsync();
            if (query.Any())
            {
                foreach(var item in query)
                {
                    var dto = new PaymentDTO();
                    dto.FullName = item.Users.LastName +" "+ item.Users.FirstName;
                    dto.Phonenumber = item.Users.PhoneNumber;
                    dto.Price = item.Price;
                    dto.PaymentDate = item.PaymentDate;
                    dto.ExpireationDate = item.ExpireationDate;
                    dto.IsRefund = item.IsRefund;
                    dto.TransactionId = item.TransactionId;
                    dto.VnPayResponseCode = item.VnPayResponseCode;
                    dto.PaymentId = item.PaymentId;
                    dto.ExtendDate = item.ExtendDate;
                    response.Data.Add(dto);
                }
                response.Message = BaseCode.SUCCESS_MESSAGE;
                response.Code = BaseCode.SUCCESS;
            }
            else
            {
                response.Data = null;
                response.Message = BaseCode.SUCCESS_MESSAGE;
                response.Code = BaseCode.SUCCESS;
            }
            return response;
        }

        //public async Task<BaseResponse<bool>> UpdateIsRefund()
        //{
        //    var userId = _accessor.HttpContext?.User.FindFirst("UserID")?.Value.ToString();
        //    var query = await _context.Payments.Where(x => x.UserId.ToString().Equals(userId)).FirstOrDefaultAsync();
        //}
    }
}
