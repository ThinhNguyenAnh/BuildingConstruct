﻿using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Response;
using ViewModels.SavePost;

namespace Application.System.SavePost
{
    public interface ISaveService
    {
        public Task<BaseResponse<string>> SavePost(SavePostRequest request);
        public Task<BaseResponse<List<SavePostDetailDTO>>> GetSavePostByUsID();
        public Task<bool> DeleteSave(DeleteSaveRequest request);
    }
}