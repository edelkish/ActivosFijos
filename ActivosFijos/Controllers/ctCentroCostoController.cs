#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Repositorios;
using ActivosFijos.Clases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ActivosFijos.Controllers
{
    [Authorize(AuthenticationSchemes = "Activos Fijos-" + CookieAuthenticationDefaults.AuthenticationScheme, Roles = "admin_web, gest_nom")]
    public class ctCentroCostoController : Controller
    {
        private readonly aftContext _context;
        private readonly repoCentrosDeCosto _repoCentroC;
        private readonly repoValidaciones _repoValidaciones;

        public ctCentroCostoController(aftContext context, repoCentrosDeCosto repoCentroC, repoValidaciones repoValidaciones)
        {
            _context = context;
            _repoCentroC = repoCentroC;
            _repoValidaciones = repoValidaciones;
        }

        // GET: ctCentroCosto
        public async Task<IActionResult> CentrosDeCosto()
        {

            return View("vCentrosDeCosto");
        }

        [HttpPost]
        public async Task<IActionResult> ListadoCentrosDeCosto()
        {
            List<CentroCosto> centroCList = await _repoCentroC.ListadoCentroDeCosto().ConfigureAwait(false);

            return PartialView("_vpTablaCentrosDeCosto", centroCList);
        }

        // GET: ctCentroCosto/Create
        public IActionResult CrearCentroDeCosto()
        {
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Centro de Costo");
            ViewData["respuesta"] = new clResponse<CentroCosto>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };

            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;

            return View("vCrearCentroCosto", new CentroCosto());
        }

        // POST: ctCentroCosto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCentroDeCosto([Bind("Numero,Nombre,IdCentroCosto")] CentroCosto centroCosto)
        {
            if (centroCosto != null)
            {
                var iDictionaryCentrosC = _repoValidaciones.EspaciosEnBlanco(centroCosto.Nombre, "Nombre");
                if (iDictionaryCentrosC.Count > 0)
                {
                    foreach (var item in iDictionaryCentrosC)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoCentroC.SiExisteNombreDelCentroCosto(centroCosto.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El Nombre del Centro Costo a insertar ya existe.");
                }

                iDictionaryCentrosC = _repoValidaciones.EspaciosEnBlanco(centroCosto.Numero == null ? string.Empty : centroCosto.Numero.ToString(), "Numero");
                if (iDictionaryCentrosC.Count > 0)
                {
                    foreach (var item in iDictionaryCentrosC)
                        ModelState.AddModelError(item.Key, item.Value);
                }
                else if (_repoCentroC.SiExisteNumeroDelCentroCosto(centroCosto.Numero))
                {
                    ModelState.AddModelError("Numero", "El Número del Centro Costo a insertar ya existe.");
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        centroCosto.IdCentroCosto = Guid.NewGuid();
                        var resultadoCentroC = await _repoCentroC.GuardarCentroCosto(centroCosto);
                        if (resultadoCentroC.SiSatisfactorio)
                        {
                            mensaje = $"{clMensaje.SuccessMsg}|{clMensaje.Satisfactorio}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            clGlobal.siNo = true;
                            clGlobal.objMensaje = resultadoCentroC.Mensaje;
                            return RedirectToAction(nameof(CrearCentroDeCosto));
                        }
                        else
                        {
                            mensaje = $"{clMensaje.ErrorMsg}|{(!ModelState.IsValid ? clMensaje.ModeloInvalido : resultadoCentroC.Mensaje)}";
                            if (!String.IsNullOrEmpty(mensaje))
                                this.TempData["Mensaje"] = mensaje;
                            ViewData["respuesta"] = resultadoCentroC;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Centro de Costo");
                            return View("vCrearCentroCosto", centroCosto);
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = ex.Message };
                        ViewData["breadsC"] = LlenarBreadsCumbs("Crear Área");
                        return View("vCrearCentroCosto", centroCosto);
                    }
                }
            }

            ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Centro Costo a insertar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Crear Centro de Costo");
            return View("vCrearCentroCosto", centroCosto);
        }

        // GET: ctCentroCosto/Edit/5
        public async Task<IActionResult> EditarCentroCosto(Guid? id)
        {
            if ((id == null) || (id == Guid.Empty))
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
                ViewData["respuesta"] = new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El objeto Centro Costo que se quiere editar es incorrecto o no existe." };
                return View("vEditarCentroCosto", new CentroCosto() { IdCentroCosto = Guid.Empty });
            }
            var centroC = await _repoCentroC.EncontrarCentroCostoPorID(id);
            if (centroC == null)
            {
                ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
                ViewData["respuesta"] = new clResponse<CentroCosto>() { SiSatisfactorio = false, Mensaje = "El objeto Centro Costo que se quiere editar no existe." };
                return View("vEditarCentroCosto", new CentroCosto() { IdCentroCosto = Guid.Empty });
            }
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
            ViewData["respuesta"] = new clResponse<CentroCosto>() { SiSatisfactorio = clGlobal.siNo, Mensaje = clGlobal.objMensaje };
            clGlobal.siNo = false;
            clGlobal.objMensaje = string.Empty;
            return View("vEditarCentroCosto", centroC);
        }

        // POST: ctCentroCosto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModificarCentroCosto(Guid IdCentroCosto, [Bind("Numero,Nombre,IdCentroCosto")] CentroCosto centroCosto)
        {
            if (centroCosto != null)
            {
                if (IdCentroCosto != centroCosto.IdCentroCosto)
                {
                    ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Centro Costo a editar." };
                    ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro Costo");

                    return View("vEditarCentroCosto", centroCosto);
                }
                else
                {
                    var iDictionaryCentrosC = _repoValidaciones.EspaciosEnBlanco(centroCosto.Nombre, "Nombre");
                    if (iDictionaryCentrosC.Count > 0)
                    {
                        foreach (var item in iDictionaryCentrosC)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                    else if (_repoCentroC.SiExisteNombreDelCentroCostoAEditar(centroCosto.Nombre, IdCentroCosto))
                    {
                        ModelState.AddModelError("Nombre", "El Nombre del Centro de Costo a insertar como modificación ya existe.");
                    }

                    iDictionaryCentrosC = _repoValidaciones.EspaciosEnBlanco(centroCosto.Numero == null ? string.Empty : centroCosto.Numero.ToString(), "Numero");
                    if (iDictionaryCentrosC.Count > 0)
                    {
                        foreach (var item in iDictionaryCentrosC)
                            ModelState.AddModelError(item.Key, item.Value);
                    }
                    else if (_repoCentroC.SiExisteNumeroDelCentroCostoAEditar(centroCosto.Numero, IdCentroCosto))
                    {
                        ModelState.AddModelError("Numero", "El Número del Centro Costo a insertar como modificación ya existe.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var mensaje = string.Empty;
                    try
                    {
                        var resultadoCentroC = await _repoCentroC.ModificarCentroCosto(centroCosto);
                        if (resultadoCentroC.SiSatisfactorio)
                        {
                            clGlobal.siNo = resultadoCentroC.SiSatisfactorio;
                            clGlobal.objMensaje = resultadoCentroC.Mensaje;
                            return RedirectToAction(nameof(EditarCentroCosto), new { id = centroCosto.IdCentroCosto });
                        }
                        else
                        {
                            ViewData["respuesta"] = resultadoCentroC;
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
                            return View("vEditarCentroCosto", centroCosto);
                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!_repoCentroC.SiExisteElCentroCostoConElID(centroCosto.IdCentroCosto))
                        {
                            ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = "El Centro de Costo a editar no existe." };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
                            return View("vEditarCentroCosto", centroCosto);
                        }
                        else
                        {
                            ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = ex.Message };
                            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
                            return View("vEditarCentroCosto", centroCosto);
                        }
                    }
                }
            }
            ViewData["respuesta"] = new clResponse<CentroCosto> { SiSatisfactorio = false, Mensaje = "Persisten errores en el objeto Centro Costo a editar." };
            ViewData["breadsC"] = LlenarBreadsCumbs("Editar Centro de Costo");
            return View("vEditarCentroCosto", centroCosto);
        }

        // POST: ctCentroCosto/Delete/5
        [HttpPost, ActionName("EliminarCentroCosto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarCentroCosto(Guid id)
        {
            try
            {
                var centroC = await _repoCentroC.EncontrarCentroCostoPorID(id);
                clResponse<CentroCosto> resultado = new clResponse<CentroCosto>();
                resultado = await _repoCentroC.EliminarCentroCosto(centroC);
                var result = new
                {
                    siNo = resultado.SiSatisfactorio,
                    objMensaje = resultado.Mensaje
                };
                return Json(result);
            }
            catch (Exception ex)
            {
                var result = new
                {
                    siNo = false,
                    objMensaje = ex.Message
                };
                return Json(result);
            }
        }

        private List<clBreadCumb> LlenarBreadsCumbs(string titulo)
        {
            List<clBreadCumb> prueba = new List<clBreadCumb>();
            clBreadCumb objBreadsCumbs = new clBreadCumb()
            {
                nombre = "Centros de Costo",
                ruta = new List<string>() { "CentrosDeCosto", "ctCentroCosto" }
            };
            prueba.Add(objBreadsCumbs);
            objBreadsCumbs = new clBreadCumb()
            {
                nombre = titulo,
                ruta = new List<string>()
            };
            prueba.Add(objBreadsCumbs);
            return prueba;
        }
    }
}
