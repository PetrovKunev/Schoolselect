﻿using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomePageDataAsync();
    }
}