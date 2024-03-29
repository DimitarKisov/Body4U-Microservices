﻿namespace Body4U.Article.Controllers
{
    using Body4U.Article.Models.Requests.Service;
    using Body4U.Article.Services.Service;
    using Body4U.Common.Controllers;
    using Body4U.Common.Infrastructure;
    using Body4U.Common.Models.Service;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [AuthorizeTrainer]
    public class ServiceController : ApiController
    {
        private readonly IServiceService serviceService;

        public ServiceController(IServiceService serviceService)
            => this.serviceService = serviceService;

        [HttpPost]
        [Route(nameof(Create))]
        public async Task<ActionResult> Create(CreateServiceRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.serviceService.Create(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.CreatedAtAction(nameof(Create), new {id = result.Data }, result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(nameof(All) + PathSeparator + TrainerId)]
        public async Task<ActionResult> All(int trainerId)
        {
            var result = await this.serviceService.All(trainerId);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(Id)]
        public async Task<ActionResult> Get(int id)
        {
            var result = await this.serviceService.Get(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.Ok(result.Data);
        }

        [HttpPut]
        [Route(nameof(Edit))]
        public async Task<ActionResult> Edit(EditServiceRequestModel request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.serviceService.Edit(request);
            if (!result.Succeeded)
            {
                this.ModelState.Clear();
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<ActionResult> Delete([FromBody] int id)
        {
            var result = await this.serviceService.Delete(id);
            if (!result.Succeeded)
            {
                return this.ProcessErrors(result);
            }

            return this.NoContent();
        }
    }
}
