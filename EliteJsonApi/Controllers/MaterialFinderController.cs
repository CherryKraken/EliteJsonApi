using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EliteJsonApi.Controllers
{
    [Route("MaterialFinder")]
    public class MaterialFinderController : Controller
    {
        private readonly ApiController _api;
        private readonly Data.EliteDbContext _ctx;

        public MaterialFinderController(Data.EliteDbContext context)
        {
            _ctx = context;
            _api = new ApiController(_ctx);
        }

        [Route("Default")]
        public IActionResult Default()
        {
            return View();
        }

        [HttpGet("LoadPartial/{*matName}")]
        public IActionResult LoadPartial(string matName)
        {
            var mat = _ctx.Material.First(m => m.Name == matName);

            if (mat.Type == "Raw")
                return RedirectToAction("testviewmodel", "api/v1");
                    //return RawMaterialPartial(matName);

            if (mat.Type == "Data")
                return EncodedMaterialPartial(matName);

            if (mat.Type == "Manufactured")
                return ManufacturedMaterialPartial(matName);

            return new PartialViewResult();
        }

        [HttpGet("RawMaterialPartial/{*matName}")]
        public PartialViewResult RawMaterialPartial(string matName)
        {
            //_api.Request = Request;
            //_api.Request.Query = Request.Query;
            //return PartialView(_api.GetMaterials(matName));
            //return PartialView(RedirectToAction("testviewmodel", "api/v1", Request.Query));
            return PartialView(_api.GetTestViewModel().);
        }

        [HttpGet]
        public PartialViewResult EncodedMaterialPartial(string matName)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public PartialViewResult ManufacturedMaterialPartial(string matName)
        {
            throw new NotImplementedException();
        }
    }
}
