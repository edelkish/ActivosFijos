using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Repositorios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctUbicacionesController : Controller
    {
        private readonly repoUbicacionesAFT ubicacionesRepositorio;
        private readonly repoLocales localRepositorio;
        private readonly repoPuestosTrabajo puestoTrabajoRepositorio;
        public ctUbicacionesController(repoUbicacionesAFT ubicacionesRepositorio, repoLocales localRepositorio, repoPuestosTrabajo puestoTrabajoRepositorio)
        {
            this.ubicacionesRepositorio = ubicacionesRepositorio;
            this.localRepositorio = localRepositorio;
            this.puestoTrabajoRepositorio = puestoTrabajoRepositorio;
        }
        public IActionResult Gestion()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Gestión de ubicaciones");
            return View("vGestionUbicaciones");
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerCubiculos()
        {
            var cubos = await ubicacionesRepositorio.ObtenerCubiculosArbol().ConfigureAwait(false);
            var contadores = await ubicacionesRepositorio.ObtenerContadoresGeneral().ConfigureAwait(false);
            var sinon = contadores.FirstOrDefault(c => c.Tipo == "sinUbicacion");
            return Json(new
            {
                cubos,
                contadores,
                sinUbicacion = sinon != null ? sinon.Contador : 0
            });
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerLocales(Guid idCubiculo)
        {
            var locales = await ubicacionesRepositorio.ObtenerLocalesDeCubiculo(idCubiculo).ConfigureAwait(false);
            var contadores = await ubicacionesRepositorio.ObtenerContadoresGeneral().ConfigureAwait(false);
            return Json(new { locales, contadores });
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerPuestos(Guid idLocal)
        {
            var puestos = await ubicacionesRepositorio.ObtenerPuestosDeLocal(idLocal).ConfigureAwait(false);
            var contadores = await ubicacionesRepositorio.ObtenerContadoresGeneral().ConfigureAwait(false);
            return Json(new { puestos, contadores });
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerAFTs(eTipoNivel tipo, Guid id)
        {
            List<mdAFTUbicacion> lista;
            string titulo = "";
            if (tipo == eTipoNivel.Local)
            {
                lista = await ubicacionesRepositorio.ObtenerAFTsPorLocal(id).ConfigureAwait(false);
                var local = await localRepositorio.SeleccionarLocalesPorID(id).ConfigureAwait(false);
                titulo = local != null ? local.Nombre : "";
            }
            else if (tipo == eTipoNivel.Puesto)
            {
                lista = await ubicacionesRepositorio.ObtenerAFTsPorPuesto(id).ConfigureAwait(false);
                var puesto = await puestoTrabajoRepositorio.SeleccionarPuestoPorID(id).ConfigureAwait(false);
                titulo = puesto != null ? puesto.Nombre : "";
            }
            else
            {
                lista = await ubicacionesRepositorio.ObtenerAFTsSinUbicacion().ConfigureAwait(false);
                titulo = "Sin ubicación";
            }

            return Json(new { rows = lista, titulo });
        }
        [HttpPost]
        public async Task<JsonResult> Asignar(string noInventarios, Guid? local, Guid? puesto)
        {
            try
            {
                var usuario = User.Identity != null ? User.Identity.Name : string.Empty;
                var resultado = new ActivosFijos.Clases.clResponse<bool> { SiSatisfactorio = true, Mensaje = string.Empty };
                if (string.IsNullOrWhiteSpace(noInventarios))
                    return Json(new { siNo = false, objMensaje = "Debe seleccionar al menos un AFT." });

                foreach (var item in noInventarios.Split('|'))
                {
                    if (long.TryParse(item, out long noInventario))
                    {
                        resultado = await ubicacionesRepositorio.AsignarUbicacion(noInventario, local, puesto, usuario ?? "").ConfigureAwait(false);
                        if (!resultado.SiSatisfactorio)
                            break;
                    }
                }
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> QuitarUbicacion(string noInventarios)
        {
            try
            {
                var usuario = User.Identity != null ? User.Identity.Name : string.Empty;
                var resultado = new ActivosFijos.Clases.clResponse<bool> { SiSatisfactorio = true, Mensaje = string.Empty };
                if (string.IsNullOrWhiteSpace(noInventarios))
                    return Json(new { siNo = false, objMensaje = "Debe seleccionar al menos un AFT." });

                foreach (var item in noInventarios.Split('|'))
                {
                    if (long.TryParse(item, out long noInventario))
                    {
                        resultado = await ubicacionesRepositorio.QuitarUbicacion(noInventario, usuario ?? "").ConfigureAwait(false);
                        if (!resultado.SiSatisfactorio)
                            break;
                    }
                }
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> Historial(long noInventario)
        {
            var historial = await ubicacionesRepositorio.ObtenerHistorial(noInventario).ConfigureAwait(false);
            return Json(new { rows = historial });
        }
        private List<Clases.clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<Clases.clBreadCumb> prueba = new List<Clases.clBreadCumb>();
            Clases.clBreadCumb objBreadsCumbs = new Clases.clBreadCumb()
            {
                nombre = "Ubicaciones",
                ruta = new List<string>() { "Ubicaciones", "ctUbicaciones" }
            };
            prueba.Add(objBreadsCumbs);
            objBreadsCumbs = new Clases.clBreadCumb()
            {
                nombre = titulo,
                ruta = new List<string>()
            };
            prueba.Add(objBreadsCumbs);
            return prueba;
        }
    }

    public enum eTipoNivel
    {
        Local,
        Puesto,
        SinUbicacion
    }
}