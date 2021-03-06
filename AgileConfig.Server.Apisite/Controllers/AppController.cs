﻿using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class AppController : Controller
    {
        private readonly IAppService _appService;
        private readonly ISysLogService _sysLogService;

        public AppController(IAppService appService, ISysLogService sysLogService)
        {
            _appService = appService;
            _sysLogService = sysLogService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]AppVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var oldApp = await _appService.GetAsync(model.Id);
            if (oldApp != null)
            {

                return Json(new
                {
                    success = false,
                    message = "应用Id已存在，请重新输入。"
                });
            }

            var app = new App();
            app.Id = model.Id;
            app.Name = model.Name;
            app.Secret = model.Secret;
            app.Enabled = model.Enabled;
            app.CreateTime = DateTime.Now;
            app.UpdateTime = null;

            var result = await _appService.AddAsync(app);
            if (result)
            {
                await _sysLogService.AddSysLogSync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"新增应用【AppId：{app.Id}】【AppName：{app.Name}】"
                });
            }

            return Json(new
            {
                success = result,
                message = !result ? "新建应用失败，请查看错误日志" : ""
            });
        }


        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]AppVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var app = await _appService.GetAsync(model.Id);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的应用程序。"
                });
            }

            app.Name = model.Name;
            app.Secret = model.Secret;
            app.Enabled = model.Enabled;
            app.UpdateTime = DateTime.Now;

            var result = await _appService.UpdateAsync(app);
            if (result)
            {
                await _sysLogService.AddSysLogSync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"修改应用【AppId：{app.Id}】【AppName：{app.Name}】"
                });
            }
            return Json(new
            {
                success = result,
                message = !result ? "修改应用失败，请查看错误日志" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var apps = await _appService.GetAllAppsAsync();

            return Json(new
            {
                success = true,
                data = apps
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var app = await _appService.GetAsync(id);

            return Json(new
            {
                success = app != null,
                data = app,
                message = app == null ? "未找到对应的应用程序。" : ""
            });
        }

        /// <summary>
        /// 在启动跟禁用之间进行切换
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DisableOrEanble(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var app = await _appService.GetAsync(id);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的应用程序。"
                });
            }

            app.Enabled = !app.Enabled;

            var result = await _appService.UpdateAsync(app);

            if (result)
            {
                await _sysLogService.AddSysLogSync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"{(app.Enabled?"启用":"禁用")}应用【AppId】:{app.Id}"
                });
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改应用失败，请查看错误日志" : ""
            });
        }
    }
}
