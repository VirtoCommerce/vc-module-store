using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Web.Controllers.Api;

[Route("api/store-authentication-schemes/{storeId}")]
public class StoreAuthenticationSchemeController : Controller
{
    private readonly IStoreAuthenticationService _service;

    public StoreAuthenticationSchemeController(IStoreAuthenticationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<IList<StoreAuthenticationScheme>>> Get([FromRoute] string storeId)
    {
        var result = await _service.GetStoreSchemesAsync(storeId, clone: false);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(ModuleConstants.Security.Permissions.Update)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update([FromRoute] string storeId, [FromBody] IList<StoreAuthenticationScheme> models)
    {
        await _service.SaveStoreSchemesAsync(storeId, models);
        return Ok();
    }
}
