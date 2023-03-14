﻿using Application.Library;
using Data.DataContext;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Payment;

namespace Application.System.VnPay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly BuildingConstructDbContext _context;
        public VnPayService(IConfiguration configuration, BuildingConstructDbContext context, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _context = context;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name},{model.UserId}, {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            return response;

        }
        public async Task<string> StoreDB(PaymentRequestDTO response)
        {
            var payment = new Payment();
            payment.UserId = Guid.Parse(response.UserId);
            payment.PaymentDate = DateTime.Now;
            payment.ExpireationDate = DateTime.Now.AddMonths(1);
            payment.Price = response.Amount;
            payment.TransactionId = response.TransactionId;
            payment.PaymentId = response.PaymentId;
            payment.IsRefund = false;
            payment.VnPayResponseCode = response.VnPayResponseCode;
            _context.Payments.Add(payment);
            var rs=_context.SaveChanges();
            if (rs > 0)
            {
                return payment.VnPayResponseCode;
            }
            else
            {
                return "Failed";
            }
        }
    }
}
