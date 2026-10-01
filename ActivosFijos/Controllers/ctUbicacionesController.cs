using ActivosFijos.Models.Auxiliar;
using ActivosFijos.Models.EntidadesBD;
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
        private readonly repoCentrosDeCosto centroCostoRepositorio;
        private readonly repoValidaciones validacionesRepositorio;
        public ctUbicacionesController(repoUbicacionesAFT ubicacionesRepositorio, repoLocales localRepositorio, repoPuestosTrabajo puestoTrabajoRepositorio, repoCentrosDeCosto centroCostoRepositorio, repoValidaciones validacionesRepositorio)
        {
            this.ubicacionesRepositorio = ubicacionesRepositorio;
            this.localRepositorio = localRepositorio;
            this.puestoTrabajoRepositorio = puestoTrabajoRepositorio;
            this.centroCostoRepositorio = centroCostoRepositorio;
            this.validacionesRepositorio = validacionesRepositorio;
        }
        public IActionResult Gestion()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Gestión de ubicaciones");
            return View("vGestionUbicaciones");
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerCentroCostos()
        {
            var centrosCosto = await ubicacionesRepositorio.ObtenerCentroCostosArbol().ConfigureAwait(false);
            var contadores = await ubicacionesRepositorio.ObtenerContadoresGeneral().ConfigureAwait(false);
            var sinon = contadores.FirstOrDefault(c => c.Tipo == "sinUbicacion");
            return Json(new
            {
                centrosCosto,
                contadores,
                sinUbicacion = sinon != null ? sinon.Contador : 0
            });
        }
        [HttpPost]
        public async Task<JsonResult> ObtenerLocales(Guid idCentroCosto)
        {
            var locales = await ubicacionesRepositorio.ObtenerLocalesDeCentroCosto(idCentroCosto).ConfigureAwait(false);
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
            if (tipo == eTipoNivel.CentroCosto)
            {
                lista = await ubicacionesRepositorio.ObtenerAFTsPorCentroCosto(id).ConfigureAwait(false);
                var centroCosto = await centroCostoRepositorio.EncontrarCentroCostoPorID(id).ConfigureAwait(false);
                titulo = centroCosto?.Nombre ?? "";
            }
            else if (tipo == eTipoNivel.Local)
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
        public async Task<JsonResult> GuardarLocal(string nombre, string? descripcion, Guid idCentroCosto)
        {
            try
            {
                var errores = validacionesRepositorio.EspaciosEnBlanco(nombre, "Nombre");
                if (errores.Count > 0)
                    return Json(new { siNo = false, objMensaje = string.Join(" ", errores.Values) });

                if (localRepositorio.SiExisteNombreDelLocal(nombre))
                    return Json(new { siNo = false, objMensaje = "El Nombre del local a insertar ya existe." });

                if (!centroCostoRepositorio.SiExisteElCentroCostoConElID(idCentroCosto))
                    return Json(new { siNo = false, objMensaje = "El Centro de Costo seleccionado no existe." });

                var local = new Locale
                {
                    IdLocal = Guid.NewGuid(),
                    Nombre = nombre,
                    Descripcion = descripcion,
                    CentroCosto = idCentroCosto
                };

                var resultado = await localRepositorio.InsertarLocal(local).ConfigureAwait(false);
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> ModificarLocal(Guid idLocal, string nombre, string? descripcion)
        {
            try
            {
                var local = await localRepositorio.SeleccionarLocalesPorID(idLocal).ConfigureAwait(false);
                if (local == null)
                    return Json(new { siNo = false, objMensaje = "El Local a modificar no existe." });

                var errores = validacionesRepositorio.EspaciosEnBlanco(nombre, "Nombre");
                if (errores.Count > 0)
                    return Json(new { siNo = false, objMensaje = string.Join(" ", errores.Values) });

                if (localRepositorio.SiExisteNombreDelLocalAModificar(nombre, idLocal))
                    return Json(new { siNo = false, objMensaje = "El Nombre del local a modificar ya existe." });

                local.Nombre = nombre;
                local.Descripcion = descripcion;

                var resultado = await localRepositorio.ModificarLocal(local).ConfigureAwait(false);
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> EliminarLocal(Guid idLocal)
        {
            try
            {
                var local = await localRepositorio.SeleccionarLocalesPorID(idLocal).ConfigureAwait(false);
                if (local == null)
                    return Json(new { siNo = false, objMensaje = "El Local a eliminar no existe." });

                if (await localRepositorio.SiTieneDependencias(idLocal).ConfigureAwait(false))
                    return Json(new { siNo = false, objMensaje = "No se puede eliminar el Local porque tiene puestos, activos fijos o historial asociado. Desasigne primero los elementos dependientes." });

                var resultado = await localRepositorio.EliminarLocal(local).ConfigureAwait(false);
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> GuardarPuestoTrabajo(string nombre, string? descripcion, Guid idLocal)
        {
            try
            {
                var errores = validacionesRepositorio.EspaciosEnBlanco(nombre, "Nombre");
                if (errores.Count > 0)
                    return Json(new { siNo = false, objMensaje = string.Join(" ", errores.Values) });

                if (puestoTrabajoRepositorio.SiExisteNombreDelPuesto(nombre, idLocal))
                    return Json(new { siNo = false, objMensaje = "El Nombre del puesto de trabajo a insertar ya existe en ese local." });

                if (!localRepositorio.LocalExists(idLocal))
                    return Json(new { siNo = false, objMensaje = "El Local seleccionado para el puesto de trabajo no existe." });

                var puesto = new PuestoTrabajo
                {
                    IdPuesto = Guid.NewGuid(),
                    Nombre = nombre,
                    Descripcion = descripcion,
                    IdLocal = idLocal
                };

                var resultado = await puestoTrabajoRepositorio.InsertarPuestoTrabajo(puesto).ConfigureAwait(false);
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> ModificarPuestoTrabajo(Guid idPuesto, string nombre, string? descripcion)
        {
            try
            {
                var puesto = await puestoTrabajoRepositorio.SeleccionarPuestoPorID(idPuesto).ConfigureAwait(false);
                if (puesto == null)
                    return Json(new { siNo = false, objMensaje = "El Puesto de Trabajo a modificar no existe." });

                var errores = validacionesRepositorio.EspaciosEnBlanco(nombre, "Nombre");
                if (errores.Count > 0)
                    return Json(new { siNo = false, objMensaje = string.Join(" ", errores.Values) });

                if (puestoTrabajoRepositorio.SiExisteNombreDelPuestoAModificar(nombre, puesto.IdLocal, idPuesto))
                    return Json(new { siNo = false, objMensaje = "El Nombre del puesto de trabajo a modificar ya existe en ese local." });

                puesto.Nombre = nombre;
                puesto.Descripcion = descripcion;

                var resultado = await puestoTrabajoRepositorio.ModificarPuestoTrabajo(puesto).ConfigureAwait(false);
                return Json(new { siNo = resultado.SiSatisfactorio, objMensaje = resultado.Mensaje });
            }
            catch (Exception ex)
            {
                return Json(new { siNo = false, objMensaje = ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> EliminarPuestoTrabajo(Guid idPuesto)
        {
            try
            {
                var puesto = await puestoTrabajoRepositorio.SeleccionarPuestoPorID(idPuesto).ConfigureAwait(false);
                if (puesto == null)
                    return Json(new { siNo = false, objMensaje = "El Puesto de Trabajo a eliminar no existe." });

                if (await puestoTrabajoRepositorio.SiTieneActivosFijosAsignados(idPuesto).ConfigureAwait(false))
                    return Json(new { siNo = false, objMensaje = "No se puede eliminar el Puesto de Trabajo porque tiene activos fijos asignados. Desasigne primero los activos." });

                var resultado = await puestoTrabajoRepositorio.EliminarPuestoTrabajo(puesto).ConfigureAwait(false);
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
        CentroCosto,
        Local,
        Puesto,
        SinUbicacion
    }
}