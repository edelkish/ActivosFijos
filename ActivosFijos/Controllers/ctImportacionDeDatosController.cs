using ActivosFijos.Clases;
using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.Vistas;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, import_sub_aft")]    
    public class ctImportacionDeDatosController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly repoImportarDatos _repoImportarDatos;

        public ctImportacionDeDatosController(Microsoft.AspNetCore.Hosting.IHostingEnvironment env, repoImportarDatos repoImportarDatos)
        {
            _env = env;
            _repoImportarDatos = repoImportarDatos;
        }

        #region Importar Activos Fijos
        public IActionResult ImportarSubmayorActivosFijos()
        {
            ViewData["respuesta"] = new clResponse<mdVExcellDataImport>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vImportarSubmayorActivosFijos", new mdVExcellDataImport());
        }

        [HttpPost]
        public IActionResult CargarDatosImportados(mdVExcellDataImport model) //IFormFile MyExcelFile, string? MSExcelTable
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var mensaje = string.Empty;
                var resultadoImportarDatos = _repoImportarDatos.GetDataTableFromSpreadsheet(model.MyExcelFile.OpenReadStream(), false);

                if (resultadoImportarDatos.SiSatisfactorio)
                {

                    ViewData["respuesta"] = resultadoImportarDatos;

                    List<SelectListItem> lista = new List<SelectListItem>();
                    lista = _repoImportarDatos.GetFromDataTableAreasDeResponsabilidad(resultadoImportarDatos.Resultado, string.Empty);

                    ViewData["Areas"] = lista;

                    lista = new List<SelectListItem>();
                    lista = _repoImportarDatos.GetFromDataTableResponsables(resultadoImportarDatos.Resultado, null);

                    ViewData["Responsables"] = lista;

                    clGlobal.vDatos = resultadoImportarDatos.Resultado;

                    return PartialView("_vpTablaSubmayor", resultadoImportarDatos.Resultado);
                }
                else
                {
                    clGlobal.siNo = resultadoImportarDatos.SiSatisfactorio;
                    clGlobal.objMensaje = resultadoImportarDatos.Mensaje;

                    return RedirectToAction(nameof(ImportarSubmayorActivosFijos));
                }
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpTablaSubmayor", new DataTable());
            }
        }

        [HttpPost]
        public IActionResult CargarDatosAdicionales()
        {
            try
            {
                var mensaje = string.Empty;
                var resultadoDatos = _repoImportarDatos.GetDatosAdicionalesFromDataTable(clGlobal.vDatos);

                ViewData["respuesta"] = resultadoDatos;

                return PartialView("_vpDatosAdicionales", resultadoDatos.Resultado);
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<mdDatosAdicionales>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpDatosAdicionales", new mdDatosAdicionales());
            }
        }

        [HttpPost]
        public IActionResult CargarDatosCentroCostosPorAreaResponsabilidad(string? area) //IFormFile MyExcelFile, string? MSExcelTable
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            if (area == "Todos")
                area = string.Empty;

            lista = _repoImportarDatos.GetFromDataTableCentrosDeCosto(clGlobal.vDatos, area);

            return PartialView("_vpCentroCosto", lista);
        }

        [HttpPost]
        public IActionResult CargarDatosResponsablesPorAreaDeResponsabilidad(string? area)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            if (area == "Todos")
                area = string.Empty;

            lista = _repoImportarDatos.GetFromDataTableResponsables(clGlobal.vDatos, area);

            return PartialView("_vpResponsables", lista);
        }

        [HttpPost]
        public IActionResult CargarDatosPorResponsablesYAreaDeResponsabilidad(string? responsable, string? area) //IFormFile MyExcelFile, string? MSExcelTable
        {
            try
            {
                responsable = responsable.Trim() == "Todos" ? string.Empty : responsable.Trim();
                area = area.Trim() == "Todos" ? string.Empty : area.Trim();

                var mensaje = string.Empty;
                var resultadoCargarDatos = _repoImportarDatos.GetDataTablePorResponsableYAreasDeResponsabilidad(clGlobal.vDatos, responsable, area);

                ViewData["respuesta"] = resultadoCargarDatos;

                return PartialView("_vpTablaSubmayorConFiltros", resultadoCargarDatos.Resultado);
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpTablaSubmayorConFiltros", new DataTable());
            }
        }

        [HttpPost]
        public IActionResult GuardarDatosImportados(mdDatosAdicionales model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;

                    clResponse<bool> respuesta = _repoImportarDatos.InsertIntoDataBaseFromDataTableDatosImportados(model.fGeneracion, clGlobal.vDatos);
                    clGlobal.siNo = respuesta.SiSatisfactorio;
                    clGlobal.objMensaje = respuesta.Mensaje;

                    return RedirectToAction(nameof(ImportarSubmayorActivosFijos));
                }

                clGlobal.siNo = false;
                clGlobal.objMensaje = "Persisten errores en el modelo a guardar.";

                return RedirectToAction(nameof(ImportarSubmayorActivosFijos));
            }
            catch (Exception ex)
            {
                clGlobal.siNo = false;
                clGlobal.objMensaje = "Error: " + ex.Message;

                return RedirectToAction(nameof(ImportarSubmayorActivosFijos));
            }
        }

        [HttpPost]
        public IActionResult EnviarPathExcellConColumnasFilas()
        {
            var result = new List<string>();
            var keys = HttpContext.Request.Form;
            string basePath = string.Empty;
            string path = "J:\\" + keys["MyExcelFile"];
            string pathCombine = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                basePath = keys["MyExcelFile"];
            }
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            pathCombine = Path.Combine(basePath, path);

            //create a list to hold all the values
            List<string> excelData = new List<string>();
            //read the Excel file as byte array
            byte[] bin = System.IO.File.ReadAllBytes(pathCombine);
            //create a new Excel package in a memorystream
            using (MemoryStream stream = new MemoryStream(bin))
            using (ExcelPackage excelPackage = new ExcelPackage(stream))
            {
                //loop all worksheets
                foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
                {
                    //loop all rows
                    for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                    {
                        //loop all columns in a row
                        for (int j = worksheet.Dimension.Start.Column; j <=
                       worksheet.Dimension.End.Column; j++)
                        {
                            //add the cell data to the List
                            if (worksheet.Cells[4, 8].Value != null)
                            {
                                excelData.Add(worksheet.Cells[i, j].Value.ToString());
                            }
                        }
                    }
                }
            }

            return View(result);
        }
        #endregion Importar Activos Fijos

        #region Importar Utiles y Herramientas
        public IActionResult ImportarUtilesHerramientas()
        {
            ViewData["respuesta"] = new clResponse<mdVExcellDataImport>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vImportarUtilesHerramientas", new mdVExcellDataImport());
        }

        [HttpPost]
        public IActionResult CargarDatosUtilesHerramientasImportados(mdVExcellDataImport model) //IFormFile MyExcelFile, string? MSExcelTable
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var mensaje = string.Empty;
                var resultadoImportarDatos = _repoImportarDatos.GetDataTableFromSpreadsheetUTH(model.MyExcelFile.OpenReadStream(), false);

                if (resultadoImportarDatos.SiSatisfactorio)
                {

                    ViewData["respuesta"] = resultadoImportarDatos;

                    List<SelectListItem> lista = new List<SelectListItem>();
                    lista = _repoImportarDatos.GetFromDataTableAreasDeResponsabilidad(resultadoImportarDatos.Resultado, string.Empty);

                    ViewData["Areas"] = lista;

                    lista = new List<SelectListItem>();
                    lista = _repoImportarDatos.GetFromDataTableResponsables(resultadoImportarDatos.Resultado, null);

                    ViewData["Responsables"] = lista;

                    clGlobal.vDatos = resultadoImportarDatos.Resultado;

                    return PartialView("_vpTablaUtilesHerramientas", resultadoImportarDatos.Resultado);
                }
                else
                {
                    clGlobal.siNo = resultadoImportarDatos.SiSatisfactorio;
                    clGlobal.objMensaje = resultadoImportarDatos.Mensaje;

                    return RedirectToAction(nameof(ImportarUtilesHerramientas));
                }
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpTablaUtilesHerramientas", new DataTable());
            }
        }

        [HttpPost]
        public IActionResult CargarDatosPorResponsablesYAreaDeResponsabilidadUTH(string? responsable, string? area) //IFormFile MyExcelFile, string? MSExcelTable
        {
            try
            {
                responsable = responsable.Trim() == "Todos" ? string.Empty : responsable.Trim();
                area = area.Trim() == "Todos" ? string.Empty : area.Trim();

                var mensaje = string.Empty;
                var resultadoCargarDatos = _repoImportarDatos.GetDataTablePorResponsableYAreasDeResponsabilidad(clGlobal.vDatos, responsable, area);

                ViewData["respuesta"] = resultadoCargarDatos;

                return PartialView("_vpTablaUtilesHerramientasConFiltros", resultadoCargarDatos.Resultado);
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<DataTable>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpTablaUtilesHerramientasConFiltros", new DataTable());
            }
        }

        [HttpPost]
        public IActionResult CargarDatosAdicionalesUTH()
        {
            try
            {
                var mensaje = string.Empty;
                var resultadoDatos = _repoImportarDatos.GetDatosAdicionalesFromDataTableUTH(clGlobal.vDatos);

                ViewData["respuesta"] = resultadoDatos;

                return PartialView("_vpDatosAdicionalesUTH", resultadoDatos.Resultado);
            }
            catch (Exception ex)
            {
                ViewData["respuesta"] = new clResponse<mdDatosAdicionales>() { SiSatisfactorio = false, Mensaje = ex.Message };
                return PartialView("_vpDatosAdicionalesUTH", new mdDatosAdicionales());
            }
        }

        [HttpPost]
        public IActionResult GuardarDatosImportadosUTH(mdDatosAdicionalesUTH model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;

                    clResponse<bool> respuesta = _repoImportarDatos.InsertIntoDataBaseFromDataTableDatosImportadosUTH(model.fGeneracion, clGlobal.vDatos);
                    clGlobal.siNo = respuesta.SiSatisfactorio;
                    clGlobal.objMensaje = respuesta.Mensaje;

                    return RedirectToAction(nameof(ImportarUtilesHerramientas));
                }

                clGlobal.siNo = false;
                clGlobal.objMensaje = "Persisten errores en el modelo a guardar.";

                return RedirectToAction(nameof(ImportarUtilesHerramientas));
            }
            catch (Exception ex)
            {
                clGlobal.siNo = false;
                clGlobal.objMensaje = "Error: " + ex.Message;

                return RedirectToAction(nameof(ImportarUtilesHerramientas));
            }
        }
        #endregion Importar Utiles y Herramientas
    }
}
