﻿namespace MyTestVueApp.Server.Interfaces
{
    public interface ILoginService
    {
        public Task<string> GetUserId(string code);
    }
}
