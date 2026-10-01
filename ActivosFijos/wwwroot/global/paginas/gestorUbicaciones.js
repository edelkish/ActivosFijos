var gridUbicacionInicializado = false;
var filasGrid = [];

$(function () {
    CargarArbolRaiz();
    InicializarModalAsignar();
    InicializarPanelHistorial();
    InicializarArrastreAFT();
    InicializarModalNodo();

    $('#jqxGridAFTUbicacion').on('rowselect rowunselect', function () {
        ActualizarBotones();
    });
});

/* ==================== Árbol ==================== */

function CargarArbolRaiz() {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerCentroCostos',
        method: 'POST',
        data: {},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            var $arbol = $('#arbolUbicaciones').empty();
            var $ulRaiz = $('<ul></ul>').appendTo($arbol);

            var $liSin = $('<li class="gu-nodo gu-sinibli"></li>').appendTo($ulRaiz);
            CargarNodoSinUbicacion($liSin, data.sinUbicacion || 0);

            var contadores = {};
            if (data.contadores) {
                $.each(data.contadores, function (i, c) {
                    contadores[c.id] = c.contador;
                });
            }

            if (data.centrosCosto && data.centrosCosto.length) {
                $.each(data.centrosCosto, function (i, centroCosto) {
                    var $li = $('<li class="gu-nodo" data-id="' + centroCosto.id + '" data-tipo="centroCosto"></li>').appendTo($ulRaiz);
                    $li.data('nodo', centroCosto);
                    var $div = $('<div class="gu-nodo-linea"></div>').appendTo($li)
                        .on('click', function (e) {
                            e.stopPropagation();
                            ToggleNodo($li, centroCosto);
                        });

                    $('<span class="gu-toggle"><i class="fa fa-chevron-right"></i></span>').appendTo($div);
                    $('<span class="gu-icono gu-icono-centrocosto"><i class="fa fa-building"></i></span>').appendTo($div);
                    $('<span class="gu-nombre"></span>').text(centroCosto.nombre).attr('title', centroCosto.nombre).appendTo($div);
                    CrearAccionesNodo($div, centroCosto, $li);
                    var total = contadores[centroCosto.id] != null ? contadores[centroCosto.id] : 0;
                    $('<span class="gu-badge"></span>').text(total).appendTo($div);
                });
            } else {
                $('<span class="gu-carga">Sin centros de costo registrados.</span>').appendTo($arbol);
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function ToggleNodo($li) {
    var $linea = $li.children('.gu-nodo-linea').first();
    var $hijos = $li.children('ul').first();

    if ($hijos.length === 0) {
        $linea.find('.gu-toggle').addClass('gu-abierta');
        $li.append('<li class="gu-carga">Cargando...</li>');
        CargarHijos($li);
    } else if ($hijos.is(':visible')) {
        $hijos.slideUp(120);
        $linea.find('.gu-toggle').removeClass('gu-abierta');
    } else {
        $hijos.slideDown(120);
        $linea.find('.gu-toggle').addClass('gu-abierta');
    }
}

function CargarHijos($li, alTerminar) {
    var nodo = $li.data('nodo');
    var url = '';
    var data = {};

    if (nodo.tipo === 'centroCosto') {
        url = baseUrl + 'ctUbicaciones/ObtenerLocales';
        data = { idCentroCosto: nodo.id };
    } else if (nodo.tipo === 'local') {
        url = baseUrl + 'ctUbicaciones/ObtenerPuestos';
        data = { idLocal: nodo.id };
    } else {
        return;
    }

    $.ajax({
        url: url,
        method: 'POST',
        data: data,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $li.find('.gu-carga').remove();

            var hijos = nodo.tipo === 'centroCosto' ? (data.locales || []) : (data.puestos || []);
            var contadores = {};
            if (data.contadores) {
                $.each(data.contadores, function (i, c) {
                    contadores[c.id] = c.contador;
                });
            }

            var $ul = $('<ul></ul>').appendTo($li);

            if (!hijos.length) {
                $('<li class="gu-carga">Sin elementos.</li>').appendTo($ul);
                $li.find('.gu-toggle').addClass('gu-abierta');
                return;
            }

            $.each(hijos, function (i, item) {
                var $liHijo = $('<li class="gu-nodo" data-id="' + item.id + '" data-tipo="' + item.tipo + '"></li>').appendTo($ul);
                $liHijo.data('nodo', item);

                var $div = $('<div class="gu-nodo-linea"></div>').appendTo($liHijo);

                var permitirToggle = item.tipo === 'local' && item.tieneHijos;
                $('<span class="gu-toggle"' + (permitirToggle ? '' : ' style="visibility:hidden;"') + '><i class="fa fa-chevron-right"></i></span>')
                    .appendTo($div)
                    .on('click', function (e) {
                        e.stopPropagation();
                        ToggleNodo($liHijo);
                    });

                if (item.tipo === 'local') {
                    $('<span class="gu-icono gu-icono-local"><i class="fa fa-map-marker"></i></span>').appendTo($div);
                } else {
                    $('<span class="gu-icono gu-icono-puesto"><i class="fa fa-desktop"></i></span>').appendTo($div);
                }

                $('<span class="gu-nombre"></span>').text(item.nombre).attr('title', item.nombre).appendTo($div);
                CrearAccionesNodo($div, item, $liHijo);

                var contador = contadores[item.id] != null ? contadores[item.id] : item.contador;
                $('<span class="gu-badge"></span>').text(contador || 0).appendTo($div);

                $div.on('click', function (e) {
                    e.stopPropagation();
                    MarcarActivo($div);
                    SeleccionarNodo(item);
                });
            });

            SincronizarToggle($li);

            if (alTerminar) {
                alTerminar();
            }
        },
        error: function (error) {
            $li.find('.gu-carga').remove();
            console.log(error);
        }
    });
}

function MarcarActivo($linea) {
    $('.gu-nodo-linea').removeClass('gu-activo');
    $linea.addClass('gu-activo');
}

function CargarNodoSinUbicacion($li, contador) {
    var $div = $('<div class="gu-nodo-linea"></div>').appendTo($li)
        .on('click', function (e) {
            e.stopPropagation();
            MarcarActivo($div);
            SeleccionarNodo({ tipo: 'sinUbicacion', id: null, nombre: 'Sin ubicación' });
        });

    $('<span class="gu-toggle" style="visibility:hidden;"></span>').appendTo($div);
    $('<span class="gu-icono gu-icono-sinibli"><i class="fa fa-question-circle"></i></span>').appendTo($div);
    $('<span class="gu-nombre">Sin ubicaci&oacute;n</span>').appendTo($div);
    $('<span class="gu-badge"></span>').text(contador || 0).appendTo($div);
}

/* ==================== Gestión de Locales y Puestos ==================== */

function CrearAccionesNodo($linea, nodo, $li) {
    if (!nodo || (nodo.tipo !== 'centroCosto' && nodo.tipo !== 'local' && nodo.tipo !== 'puesto')) {
        return;
    }

    var $acciones = $('<span class="gu-acciones"></span>');

    var CrearBoton = function (icono, titulo, peligro, alHacerClic) {
        $('<button type="button" class="gu-accion' + (peligro ? ' gu-accion-peligro' : '') + '"></button>')
            .attr('title', titulo)
            .html('<i class="fa ' + icono + '"></i>')
            .on('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (new Date().getTime() - instanteFinArrastre < 250) {
                    return;
                }
                alHacerClic();
            })
            .appendTo($acciones);
    };

    if (nodo.tipo === 'centroCosto') {
        CrearBoton('fa-plus', 'Crear Local en este Centro de Costo', false, function () {
            AbrirModalNodo('local', null, $li);
        });
    } else if (nodo.tipo === 'local') {
        CrearBoton('fa-plus', 'Crear Puesto de Trabajo en este Local', false, function () {
            AbrirModalNodo('puesto', null, $li);
        });
        CrearBoton('fa-pencil', 'Editar Local', false, function () {
            AbrirModalNodo('local', nodo, $li);
        });
        CrearBoton('fa-trash', 'Eliminar Local', true, function () {
            ConfirmarEliminarNodo('local', nodo, $li);
        });
    } else {
        CrearBoton('fa-pencil', 'Editar Puesto de Trabajo', false, function () {
            AbrirModalNodo('puesto', nodo, $li);
        });
        CrearBoton('fa-trash', 'Eliminar Puesto de Trabajo', true, function () {
            ConfirmarEliminarNodo('puesto', nodo, $li);
        });
    }

    $acciones.appendTo($linea);

    var $badge = $linea.children('.gu-badge');
    if ($badge.length) {
        $badge.appendTo($linea);
    }
}

function InicializarModalNodo() {
    $('#btnGuardarNodo').on('click', GuardarNodo);
}

function AbrirModalNodo(tipo, nodo, $li) {
    var esLocal = tipo === 'local';
    var titulo = esLocal
        ? (nodo ? 'Editar Local' : 'Crear Local')
        : (nodo ? 'Editar Puesto de Trabajo' : 'Crear Puesto de Trabajo');

    var contextoNombre = '';
    var idPadre = '';

    if (nodo) {
        var $ancestro = $li.parent().closest('.gu-nodo');
        var padre = $ancestro.length ? $ancestro.data('nodo') : null;
        contextoNombre = padre ? padre.nombre : '';
    } else {
        var nodoActual = $li.data('nodo');
        contextoNombre = nodoActual ? nodoActual.nombre : '';
        idPadre = nodoActual ? nodoActual.id : '';
    }

    $('#guNodoTitulo').text(titulo);
    $('#guNodoTipo').val(tipo);
    $('#guNodoId').val(nodo ? nodo.id : '');
    $('#guNodoIdPadre').val(idPadre);
    $('#guNodoEtiquetaPadre').text(esLocal ? 'Centro de Costo' : 'Local');
    $('#guNodoContexto').text(contextoNombre);
    $('#guNodoNombre').val(nodo ? nodo.nombre : '');
    $('#guNodoDescripcion').val(nodo ? (nodo.descripcion || '') : '');
    $('#guNodoError').hide().text('');

    $('#modalNodo').modal('show');
    setTimeout(function () { $('#guNodoNombre').focus(); }, 400);
}

function GuardarNodo() {
    var tipo = $('#guNodoTipo').val();
    var id = $('#guNodoId').val();
    var nombre = $('#guNodoNombre').val();
    var descripcion = $('#guNodoDescripcion').val();
    var idPadre = $('#guNodoIdPadre').val();

    if (!nombre || nombre.trim() === '') {
        $('#guNodoError').text('El Nombre es obligatorio.').show();
        return;
    }

    var esLocal = tipo === 'local';
    var url;
    var data;

    if (id) {
        url = baseUrl + 'ctUbicaciones/' + (esLocal ? 'ModificarLocal' : 'ModificarPuestoTrabajo');
        data = esLocal
            ? { idLocal: id, nombre: nombre, descripcion: descripcion }
            : { idPuesto: id, nombre: nombre, descripcion: descripcion };
    } else {
        url = baseUrl + 'ctUbicaciones/' + (esLocal ? 'GuardarLocal' : 'GuardarPuestoTrabajo');
        data = esLocal
            ? { nombre: nombre, descripcion: descripcion, idCentroCosto: idPadre }
            : { nombre: nombre, descripcion: descripcion, idLocal: idPadre };
    }

    $('#btnGuardarNodo').prop('disabled', true);

    $.ajax({
        url: url,
        method: 'POST',
        data: data,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (respuesta) {
            $('#btnGuardarNodo').prop('disabled', false);
            if (respuesta.siNo) {
                MiApp.Mensajes.mostrarNotificacion("success", respuesta.objMensaje);
                $('#modalNodo').modal('hide');
                RecargarRamaPadre(esLocal, idPadre);
            } else {
                $('#guNodoError').text(respuesta.objMensaje).show();
            }
        },
        error: function (error) {
            $('#btnGuardarNodo').prop('disabled', false);
            $('#guNodoError').text('Ocurrió un error al guardar. Revise la consola.').show();
            console.log(error);
        }
    });
}

function ConfirmarEliminarNodo(tipo, nodo, $li) {
    var esLocal = tipo === 'local';
    var tipoTexto = esLocal ? 'Local' : 'Puesto de Trabajo';

    $.confirm({
        title: '!Confirmar eliminaci&oacute;n!',
        content: '&iquest;Est&aacute; seguro de que desea eliminar el ' + tipoTexto + ' <b>' + nodo.nombre + '</b>?',
        icon: 'fa fa-warning',
        type: 'orange',
        theme: 'light',
        animation: 'news',
        closeAnimation: 'news',
        buttons: {
            eliminar: {
                text: 'Eliminar',
                btnClass: 'btn-orange',
                action: function () {
                    EjecutarEliminarNodo(tipo, nodo, $li);
                }
            },
            cancelar: {
            }
        }
    });
}

function EjecutarEliminarNodo(tipo, nodo, $li) {
    var esLocal = tipo === 'local';
    var $padre = $li.parent().closest('.gu-nodo');
    var idPadre = $padre.length ? $padre.data('nodo').id : null;
    var idNodo = nodo.id;

    $.ajax({
        url: baseUrl + 'ctUbicaciones/' + (esLocal ? 'EliminarLocal' : 'EliminarPuestoTrabajo'),
        method: 'POST',
        data: esLocal ? { idLocal: idNodo } : { idPuesto: idNodo },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (respuesta) {
            if (!respuesta.siNo) {
                MiApp.Mensajes.mostrarNotificacion("error", respuesta.objMensaje);
                return;
            }

            MiApp.Mensajes.mostrarNotificacion("success", respuesta.objMensaje);

            var nodoSeleccionadoEsEste = nodoSeleccionado && nodoSeleccionado.id === idNodo;
            if (nodoSeleccionadoEsEste) {
                nodoSeleccionado = null;
                $('#tituloNivel').text('Seleccione una ubicaci&oacute;n');
                CargarGridAFT([]);
            }

            if (idPadre) {
                RecargarRamaPadre(esLocal, idPadre);
            } else {
                RecargarArbolYGrid();
            }
        },
        error: function (error) {
            MiApp.Mensajes.mostrarNotificacion("error", error);
        }
    });
}

function RecargarRamaPadre(esLocal, idPadre) {
    if (!idPadre) {
        RecargarArbolYGrid();
        return;
    }

    var selector = esLocal
        ? '.gu-nodo[data-tipo="centroCosto"][data-id="' + idPadre + '"]'
        : '.gu-nodo[data-tipo="local"][data-id="' + idPadre + '"]';

    var $li = $(selector).first();
    if ($li.length === 0) {
        RecargarArbolYGrid();
        return;
    }

    var $hijos = $li.children('ul').first();
    if ($hijos.length === 0) {
        $li.data('nodo').tieneHijos = true;
        ActualizarToggle($li, true);
        CargarHijos($li, ActualizarContadoresArbol);
    } else {
        $hijos.remove();
        CargarHijos($li, ActualizarContadoresArbol);
    }
}

function ActualizarToggle($li, abierta) {
    var $toggle = $li.children('.gu-nodo-linea').first().find('.gu-toggle').first();
    if (abierta) {
        $toggle.addClass('gu-abierta');
    } else {
        $toggle.removeClass('gu-abierta');
    }
}

function SincronizarToggle($li) {
    var $toggle = $li.children('.gu-nodo-linea').first().children('.gu-toggle').first();
    if ($toggle.length === 0) {
        return;
    }
    var nodo = $li.data('nodo');
    var visible = nodo && nodo.tipo === 'local' && nodo.tieneHijos;
    $toggle.css('visibility', visible ? 'visible' : 'hidden');
}

function SeleccionarNodo(nodo) {
    nodoSeleccionado = nodo;

    var data = {};
    if (nodo.tipo === 'centroCosto') {
        data = { tipo: 0, id: nodo.id };
    } else if (nodo.tipo === 'local') {
        data = { tipo: 1, id: nodo.id };
    } else if (nodo.tipo === 'puesto') {
        data = { tipo: 2, id: nodo.id };
    } else {
        data = { tipo: 3, id: '00000000-0000-0000-0000-000000000000' };
    }

    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerAFTs',
        method: 'POST',
        data: data,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $('#tituloNivel').text(data.titulo);
            CargarGridAFT(data.rows || []);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

/* ==================== Grid AFT ==================== */

function CargarGridAFT(rows) {
    filasGrid = rows || [];

    var source =
    {
        localdata: rows,
        datafields:
            [
                { name: 'noInventario', type: 'number' },
                { name: 'descripcion', type: 'string' },
                { name: 'clasificador', type: 'string' },
                { name: 'localActual', type: 'string' },
                { name: 'puestoActual', type: 'string' }
            ],
        datatype: "array"
    };

    var dataadapter = new $.jqx.dataAdapter(source);

    if (gridUbicacionInicializado) {
        $('#jqxGridAFTUbicacion').jqxGrid({ source: dataadapter });
        $('#jqxGridAFTUbicacion').jqxGrid('updatebounddata', 'cells');
        $('#jqxGridAFTUbicacion').jqxGrid('clearselection');
        AplicarVisibilidadColumnas();
        ActualizarBotones();
        return;
    }

    gridUbicacionInicializado = true;

    var rendererNoInv = function (row, column, value, defaultHTML, columnProperties, rowData) {
        var raw = rowData ? rowData.noInventario : value;
        return '<span data-noinventario="' + raw + '">' + defaultHTML + '</span>';
    };

    $('#jqxGridAFTUbicacion').jqxGrid(
        {
            width: '100%',
            height: 450,
            source: dataadapter,
            filterable: true,
            showfilterrow: true,
            sortable: true,
            autoheight: true,
            autorowheight: true,
            altrows: true,
            selectionmode: 'multiplerows',
            localization: getLocalization('es'),
            pagesize: 20,
            pagermode: 'simple',
            pageable: true,
            rendergridrows: function (obj) {
                return obj.data;
            },
            columns: [
                { text: 'No. Inventario', datafield: 'noInventario', width: 120, filtercondition: 'contains', cellsrenderer: rendererNoInv },
                { text: 'Descripci&oacute;n', datafield: 'descripcion', width: 380, filtercondition: 'contains', cellsrenderer: rendererNoInv },
                { text: 'Clasificador', datafield: 'clasificador', width: 180, filtercondition: 'contains', cellsrenderer: rendererNoInv },
                { text: 'Local', datafield: 'localActual', width: 160, filtercondition: 'contains', cellsrenderer: rendererNoInv },
                { text: 'Puesto', datafield: 'puestoActual', width: 160, filtercondition: 'contains', cellsrenderer: rendererNoInv }
            ]
        });

    AplicarVisibilidadColumnas();
    ActualizarBotones();
}

function AplicarVisibilidadColumnas() {
    var esSinUbicacion = nodoSeleccionado && nodoSeleccionado.tipo === 'sinUbicacion';
    if (esSinUbicacion) {
        $('#jqxGridAFTUbicacion').jqxGrid('hidecolumn', 'localActual');
        $('#jqxGridAFTUbicacion').jqxGrid('hidecolumn', 'puestoActual');
    } else {
        $('#jqxGridAFTUbicacion').jqxGrid('showcolumn', 'localActual');
        $('#jqxGridAFTUbicacion').jqxGrid('showcolumn', 'puestoActual');
    }
}

function ObtenerFilasSeleccionadas() {
    var rowsindexes = $('#jqxGridAFTUbicacion').jqxGrid('getselectedrowindexes');
    var filas = [];
    $.each(rowsindexes, function (i, idx) {
        var fila = $('#jqxGridAFTUbicacion').jqxGrid('getrowdata', idx);
        if (fila) {
            filas.push(fila);
        }
    });
    return filas;
}

function ObtenerNoInventariosSeleccionados() {
    var filas = ObtenerFilasSeleccionadas();
    var cadena = [];
    $.each(filas, function (i, fila) {
        cadena.push(fila.noInventario);
    });
    return cadena;
}

function ActualizarBotones() {
    var filas = ObtenerNoInventariosSeleccionados();
    $('#btnAsignarMover').prop('disabled', filas.length === 0);
    $('#btnHistorial').prop('disabled', filas.length !== 1);

    var nodoEsSinUbicacion = nodoSeleccionado && nodoSeleccionado.tipo === 'sinUbicacion';
    $('#btnQuitarUbicacion').prop('disabled', filas.length === 0 || nodoEsSinUbicacion);
}

/* ==================== Modal Asignar / Mover ==================== */

function InicializarModalAsignar() {
    CargarSelectCentroCostos('#ddlCentroCostoDestino');

    $('#chkAsignarPuesto').on('change', function () {
        var activo = $(this).is(':checked');
        $('#grupoPuestoDestino').toggle(activo);
        $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
    });

    $('#ddlCentroCostoDestino').on('change', function () {
        var idCentroCosto = $(this).val();
        $('#ddlLocalDestino').empty().append('<option value="">-- Seleccione --</option>');
        $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
        if (idCentroCosto) {
            CargarSelectLocales(idCentroCosto);
        }
    });

    $('#ddlLocalDestino').on('change', function () {
        var idLocal = $(this).val();
        $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
        if (idLocal && $('#chkAsignarPuesto').is(':checked')) {
            CargarSelectPuestos(idLocal);
        }
    });

    $('#btnAsignarMover').on('click', function () {
        $('#modalAsignar').modal('show');
    });

    $('#btnConfirmarAsignar').on('click', function () {
        var filas = ObtenerNoInventariosSeleccionados();
        if (filas.length === 0) {
            MiApp.Mensajes.mostrarNotificacion("error", "Debe seleccionar al menos un AFT.");
            return;
        }

        var asignaPuesto = $('#chkAsignarPuesto').is(':checked');
        var idDestino = asignaPuesto ? $('#ddlPuestoDestino').val() : $('#ddlLocalDestino').val();

        if (!idDestino) {
            MiApp.Mensajes.mostrarNotificacion("error", "Debe seleccionar el destino.");
            return;
        }

        var cadena = filas.join('|');
        ejecutarAsignacion(cadena, asignaPuesto ? null : idDestino, asignaPuesto ? idDestino : null);
    });
}

function ejecutarAsignacion(noInventarios, local, puesto) {
    var data = { noInventarios: noInventarios, local: local, puesto: puesto };

    $.ajax({
        url: baseUrl + 'ctUbicaciones/Asignar',
        method: "POST",
        data: data,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            if (data.siNo) {
                MiApp.Mensajes.mostrarNotificacion("success", data.objMensaje);
                $('#modalAsignar').modal('hide');
                $('#ddlCentroCostoDestino').val('').trigger('change');
                RecargarArbolYGrid();
            } else {
                MiApp.Mensajes.mostrarNotificacion("error", data.objMensaje);
            }
        },
        error: function (error) {
            MiApp.Mensajes.mostrarNotificacion("error", error);
        }
    });
}

function ActualizarContadoresArbol() {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerCentroCostos',
        method: 'POST',
        data: {},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            var contadores = {};
            if (data.contadores) {
                $.each(data.contadores, function (i, c) {
                    contadores[c.id] = c.contador;
                });
            }

            $('#arbolUbicaciones .gu-sinibli .gu-badge').text(data.sinUbicacion || 0);

            $('#arbolUbicaciones .gu-nodo').each(function () {
                var id = $(this).data('id');
                if (id) {
                    var valor = contadores[id] != null ? contadores[id] : 0;
                    $(this).find('.gu-badge').first().text(valor);
                }
            });
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function RecargarArbolYGrid() {
    if ($('#arbolUbicaciones').children().length > 0) {
        ActualizarContadoresArbol();
    } else {
        CargarArbolRaiz();
    }
    if (nodoSeleccionado) {
        SeleccionarNodo(nodoSeleccionado);
    }
}

/* ==================== Selectores destino ==================== */

function CargarSelectCentroCostos($sel) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerCentroCostos',
        method: 'POST',
        data: {},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $($sel).empty().append('<option value="">-- Seleccione --</option>');
            if (data.centrosCosto) {
                $.each(data.centrosCosto, function (i, centroCosto) {
                    $($sel).append('<option value="' + centroCosto.id + '">' + centroCosto.nombre + '</option>');
                });
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function CargarSelectLocales(idCentroCosto) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerLocales',
        method: 'POST',
        data: { idCentroCosto: idCentroCosto },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $('#ddlLocalDestino').empty().append('<option value="">-- Seleccione --</option>');
            if (data.locales) {
                $.each(data.locales, function (i, local) {
                    $('#ddlLocalDestino').append('<option value="' + local.id + '">' + local.nombre + '</option>');
                });
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function CargarSelectPuestos(idLocal) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerPuestos',
        method: 'POST',
        data: { idLocal: idLocal },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
            if (data.puestos) {
                $.each(data.puestos, function (i, puesto) {
                    $('#ddlPuestoDestino').append('<option value="' + puesto.id + '">' + puesto.nombre + '</option>');
                });
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

/* ==================== Drag & Drop Grid -> Árbol ==================== */

var arrastreActivo = null;
var instanteFinArrastre = 0;

function InicializarArrastreAFT() {
    $('#jqxGridAFTUbicacion').on('mousedown', '.jqx-grid-cell', function (e) {
        if (e.which !== 1) return;

        var $celda = $(this);
        var $nro = $celda.find('[data-noinventario]').first();
        if ($nro.length === 0) return;

        var noInventario = $nro.data('noinventario');
        var fila = null;
        $.each(filasGrid, function (i, f) {
            if (String(f.noInventario) === String(noInventario)) {
                fila = f;
                return false;
            }
        });
        if (!fila) return;

        var seleccionadas = ObtenerFilasSeleccionadas();
        var filaEnSeleccion = false;
        $.each(seleccionadas, function (i, f) {
            if (String(f.noInventario) === String(noInventario)) {
                filaEnSeleccion = true;
                return false;
            }
        });
        var filasAArrastrar = filaEnSeleccion ? seleccionadas : [fila];

        var inicioX = e.clientX, inicioY = e.clientY;
        var arrastrando = false;

        var move = function (me) {
            if (!arrastrando) {
                if (Math.abs(me.clientX - inicioX) > 5 || Math.abs(me.clientY - inicioY) > 5) {
                    arrastrando = true;
                    IniciarArrastre(filasAArrastrar);
                }
            }
            if (arrastrando) {
                MoverArrastre(me.clientX, me.clientY);
            }
        };

        var up = function (ue) {
            $(document).off('mousemove', move);
            $(document).off('mouseup', up);
            if (arrastrando) {
                FinalizarArrastre(ue.clientX, ue.clientY, filasAArrastrar);
            }
        };

        $(document).on('mousemove', move);
        $(document).on('mouseup', up);
    });
}

function IniciarArrastre(filas) {
    arrastreActivo = filas;
    $('body').addClass('gu-arrastrando');
    $('#jqxGridAFTUbicacion').jqxGrid('clearselection');

    var etiqueta = filas.length > 1
        ? filas.length + ' AFT seleccionados'
        : 'AFT No. ' + filas[0].noInventario;

    $('<div id="guDragGhost" class="gu-drag-ghost">' + etiqueta + '</div>')
        .appendTo('body');

    $('.gu-nodo-linea').addClass('gu-drop-objetivo');
}

function MoverArrastre(x, y) {
    $('#guDragGhost').css({ left: (x + 12) + 'px', top: (y + 12) + 'px' });

    $('.gu-drop-objetivo').removeClass('gu-drop-sobre');
    var $bajo = $(document.elementFromPoint(x, y)).closest('.gu-nodo-linea');
    if ($bajo.length) {
        $bajo.addClass('gu-drop-sobre');
    }
}

function FinalizarArrastre(x, y, filas) {
    $('#guDragGhost').remove();
    $('body').removeClass('gu-arrastrando');
    $('.gu-nodo-linea').removeClass('gu-drop-objetivo').removeClass('gu-drop-sobre');

    instanteFinArrastre = new Date().getTime();

    var $bajo = $(document.elementFromPoint(x, y)).closest('.gu-nodo-linea');
    var $nodo = $bajo.closest('.gu-nodo');
    var nodo = $nodo.data('nodo');

    if (!nodo || (nodo.tipo !== 'local' && nodo.tipo !== 'puesto')) {
        arrastreActivo = null;
        return;
    }

    arrastreActivo = null;

    var tipoTexto = nodo.tipo === 'puesto' ? 'puesto de trabajo' : 'local';
    var contenido = filas.length > 1
        ? '&iquest;Est&aacute; seguro de que desea asignar los <b>' + filas.length +
            '</b> AFT seleccionados (' + filas.map(function (f) { return f.noInventario; }).join(', ') +
            ') al ' + tipoTexto + ' <b>' + nodo.nombre + '</b>?'
        : '&iquest;Est&aacute; seguro de que desea asignar el AFT No. <b>' + filas[0].noInventario +
            '</b> al ' + tipoTexto + ' <b>' + nodo.nombre + '</b>?';

    $.confirm({
        title: '!Confirmar asignaci&oacute;n!',
        content: contenido,
        icon: 'fa fa-exchange',
        type: 'blue',
        theme: 'light',
        animation: 'news',
        closeAnimation: 'news',
        buttons: {
            confirmar: {
                text: 'Asignar',
                btnClass: 'btn-blue',
                action: function () {
                    var cadena = filas.map(function (f) { return f.noInventario; }).join('|');
                    if (nodo.tipo === 'local') {
                        ejecutarAsignacion(cadena, nodo.id, null);
                    } else {
                        ejecutarAsignacion(cadena, null, nodo.id);
                    }
                }
            },
            cancelar: function () {
            }
        }
    });
}

/* ==================== Panel deslizante Historial ==================== */

function InicializarPanelHistorial() {
    $('#btnHistorial').on('click', function () {
        HacerSolicitudHistorial();
    });

    $('#btnQuitarUbicacion').on('click', QuitarUbicacionSeleccionados);

    $('#btnCerrarHistorial').on('click', CerrarPanelHistorial);
    $('#btnCerrarHistorialFooter').on('click', CerrarPanelHistorial);
    $('#panelHistorialOverlay').on('click', CerrarPanelHistorial);
}

function QuitarUbicacionSeleccionados() {
    var filas = ObtenerNoInventariosSeleccionados();
    if (filas.length === 0) {
        MiApp.Mensajes.mostrarNotificacion("error", "Seleccione al menos un AFT.");
        return;
    }

    var noInventarios = filas.join('|');
    $.confirm({
        title: '!Advertencia de desasignaci&oacute;n!',
        content: '&iquest;Est&aacute; seguro de que desea quitar la ubicaci&oacute;n del AFT <b>' + filas.join(', ') + '</b>?',
        icon: 'fa fa-warning',
        type: 'orange',
        theme: 'light',
        animation: 'news',
        closeAnimation: 'news',
        buttons: {
            quitar: {
                text: 'Quitar ubicaci&oacute;n',
                btnClass: 'btn-orange',
                action: function () {
                    ejecutarQuitarUbicacion(noInventarios);
                }
            },
            cancelar: function () {
            }
        }
    });
}

function ejecutarQuitarUbicacion(noInventarios) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/QuitarUbicacion',
        method: 'POST',
        data: { noInventarios: noInventarios },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            if (data.siNo) {
                MiApp.Mensajes.mostrarNotificacion("success", data.objMensaje);
                RecargarArbolYGrid();
            } else {
                MiApp.Mensajes.mostrarNotificacion("error", data.objMensaje);
            }
        },
        error: function (error) {
            MiApp.Mensajes.mostrarNotificacion("error", error);
        }
    });
}

function HacerSolicitudHistorial() {
    var filas = ObtenerNoInventariosSeleccionados();
    if (filas.length !== 1) {
        MiApp.Mensajes.mostrarNotificacion("error", "Seleccione un único AFT para ver su historial.");
        return;
    }

    $('#guPanelTitulo').text('Historial del AFT ' + filas[0]);
    $('#guPanelBody').empty().append('<span class="gu-carga">Cargando...</span>');
    AbrirPanelHistorial();

    $.ajax({
        url: baseUrl + 'ctUbicaciones/Historial',
        method: "POST",
        data: { noInventario: filas[0] },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            RenderHistorial(data.rows);
        },
        error: function (error) {
            $('#guPanelBody').empty().append('<span class="gu-carga">Error al cargar el historial.</span>');
            console.log(error);
        }
    });
}

function AbrirPanelHistorial() {
    $('#panelHistorial').addClass('gu-abierto');
    $('#panelHistorialOverlay').addClass('gu-visible');
}

function CerrarPanelHistorial() {
    $('#panelHistorial').removeClass('gu-abierto');
    $('#panelHistorialOverlay').removeClass('gu-visible');
}

function RenderHistorial(rows) {
    var $body = $('#guPanelBody').empty();

    if (!rows || rows.length === 0) {
        $body.append('<span class="gu-carga">Sin movimientos registrados para este AFT.</span>');
        return;
    }

    var $timeline = $('<div class="gu-timeline"></div>').appendTo($body);

    $.each(rows, function (i, h) {
        var $item = $('<div class="gu-timeline-item"></div>').appendTo($timeline);

        var fecha = new Date(h.fechaMovimiento);
        var fechaTxt = fecha.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' }) +
            ' ' + fecha.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' });

        $('<div class="gu-timeline-fecha"></div>').text(fechaTxt).appendTo($item);

        var origen = '';
        if (h.puestoOrigen) {
            origen = '<span class="gu-timeline-etiqueta gu-etiqueta-puesto"><i class="fa fa-desktop"></i> ' + h.puestoOrigen + '</span>';
        }
        if (h.localOrigen) {
            origen += '<span class="gu-timeline-etiqueta gu-etiqueta-local"><i class="fa fa-map-marker"></i> ' + h.localOrigen + '</span>';
        }
        if (!origen) {
            origen = '<span class="gu-timeline-ruido">Sin ubicaci&oacute;n</span>';
        }

        var destino = '';
        if (h.puestoDestino) {
            destino = '<span class="gu-timeline-etiqueta gu-etiqueta-puesto"><i class="fa fa-desktop"></i> ' + h.puestoDestino + '</span>';
        }
        if (h.localDestino) {
            destino += '<span class="gu-timeline-etiqueta gu-etiqueta-local"><i class="fa fa-map-marker"></i> ' + h.localDestino + '</span>';
        }
        if (!destino) {
            destino = '<span class="gu-timeline-ruido">Sin ubicaci&oacute;n</span>';
        }

        var $detalle = $('<div class="gu-timeline-detalle"></div>').appendTo($item);
        $('<div>Origen: ' + origen + '</div>').appendTo($detalle);
        $('<div>Destino: ' + destino + '</div>').appendTo($detalle);
        if (h.usuarioRegistra) {
            $('<div class="gu-timeline-ruido">Usuario: ' + h.usuarioRegistra + '</div>').appendTo($detalle);
        }
    });
}