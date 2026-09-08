var gridUbicacionInicializado = false;
var filasGrid = [];

$(function () {
    CargarArbolRaiz();
    InicializarModalAsignar();
    InicializarPanelHistorial();
    InicializarArrastreAFT();

    $('#jqxGridAFTUbicacion').on('rowselect rowunselect', function () {
        ActualizarBotones();
    });
});

/* ==================== Árbol ==================== */

function CargarArbolRaiz() {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerCubiculos',
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

            if (data.cubos && data.cubos.length) {
                $.each(data.cubos, function (i, cubo) {
                    var $li = $('<li class="gu-nodo" data-id="' + cubo.id + '" data-tipo="cubiculo"></li>').appendTo($ulRaiz);
                    $li.data('nodo', cubo);
                    var $div = $('<div class="gu-nodo-linea"></div>').appendTo($li)
                        .on('click', function (e) {
                            e.stopPropagation();
                            ToggleNodo($li, cubo);
                        });

                    $('<span class="gu-toggle"><i class="fa fa-chevron-right"></i></span>').appendTo($div);
                    $('<span class="gu-icono gu-icono-cubiculo"><i class="fa fa-building"></i></span>').appendTo($div);
                    $('<span class="gu-nombre"></span>').text(cubo.nombre).appendTo($div);
                });
            } else {
                $('<span class="gu-carga">Sin cub&iacute;culos registrados.</span>').appendTo($arbol);
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

function CargarHijos($li) {
    var nodo = $li.data('nodo');
    var url = '';
    var data = {};

    if (nodo.tipo === 'cubiculo') {
        url = baseUrl + 'ctUbicaciones/ObtenerLocales';
        data = { idCubiculo: nodo.id };
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

            var hijos = nodo.tipo === 'cubiculo' ? (data.locales || []) : (data.puestos || []);
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
                if (permitirToggle) {
                    $('<span class="gu-toggle"><i class="fa fa-chevron-right"></i></span>').appendTo($div)
                        .on('click', function (e) {
                            e.stopPropagation();
                            ToggleNodo($liHijo);
                        });
                } else {
                    $('<span class="gu-toggle" style="visibility:hidden;"></span>').appendTo($div);
                }

                if (item.tipo === 'local') {
                    $('<span class="gu-icono gu-icono-local"><i class="fa fa-map-marker"></i></span>').appendTo($div);
                } else {
                    $('<span class="gu-icono gu-icono-puesto"><i class="fa fa-desktop"></i></span>').appendTo($div);
                }

                $('<span class="gu-nombre"></span>').text(item.nombre).appendTo($div);

                var contador = contadores[item.id] != null ? contadores[item.id] : item.contador;
                $('<span class="gu-badge"></span>').text(contador || 0).appendTo($div);

                $div.on('click', function (e) {
                    e.stopPropagation();
                    MarcarActivo($div);
                    SeleccionarNodo(item);
                });
            });
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

function SeleccionarNodo(nodo) {
    nodoSeleccionado = nodo;

    var data = {};
    if (nodo.tipo === 'local') {
        data = { tipo: 0, id: nodo.id };
    } else if (nodo.tipo === 'puesto') {
        data = { tipo: 1, id: nodo.id };
    } else {
        data = { tipo: 2, id: '00000000-0000-0000-0000-000000000000' };
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

function ObtenerNoInventariosSeleccionados() {
    var rowsindexes = $('#jqxGridAFTUbicacion').jqxGrid('getselectedrowindexes');
    var cadena = [];
    $.each(rowsindexes, function (i, idx) {
        var fila = $('#jqxGridAFTUbicacion').jqxGrid('getrowdata', idx);
        if (fila) {
            cadena.push(fila.noInventario);
        }
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
    CargarSelectCubiculos('#ddlCubiculoDestino');

    $('#chkAsignarPuesto').on('change', function () {
        var activo = $(this).is(':checked');
        $('#grupoPuestoDestino').toggle(activo);
        $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
    });

    $('#ddlCubiculoDestino').on('change', function () {
        var idCubiculo = $(this).val();
        $('#ddlLocalDestino').empty().append('<option value="">-- Seleccione --</option>');
        $('#ddlPuestoDestino').empty().append('<option value="">-- Seleccione --</option>');
        if (idCubiculo) {
            CargarSelectLocales(idCubiculo);
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
                $('#ddlCubiculoDestino').val('').trigger('change');
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

function RecargarArbolYGrid() {
    CargarArbolRaiz();
    if (nodoSeleccionado) {
        SeleccionarNodo(nodoSeleccionado);
    }
}

/* ==================== Selectores destino ==================== */

function CargarSelectCubiculos($sel) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerCubiculos',
        method: 'POST',
        data: {},
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $($sel).empty().append('<option value="">-- Seleccione --</option>');
            if (data.cubos) {
                $.each(data.cubos, function (i, cubo) {
                    $($sel).append('<option value="' + cubo.id + '">' + cubo.nombre + '</option>');
                });
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}

function CargarSelectLocales(idCubiculo) {
    $.ajax({
        url: baseUrl + 'ctUbicaciones/ObtenerLocales',
        method: 'POST',
        data: { idCubiculo: idCubiculo },
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

        var inicioX = e.clientX, inicioY = e.clientY;
        var arrastrando = false;

        var move = function (me) {
            if (!arrastrando) {
                if (Math.abs(me.clientX - inicioX) > 5 || Math.abs(me.clientY - inicioY) > 5) {
                    arrastrando = true;
                    IniciarArrastre(fila);
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
                FinalizarArrastre(ue.clientX, ue.clientY, fila);
            }
        };

        $(document).on('mousemove', move);
        $(document).on('mouseup', up);
    });
}

function IniciarArrastre(fila) {
    arrastreActivo = fila;
    $('body').addClass('gu-arrastrando');
    $('#jqxGridAFTUbicacion').jqxGrid('clearselection');

    $('<div id="guDragGhost" class="gu-drag-ghost">AFT No. ' + fila.noInventario + '</div>')
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

function FinalizarArrastre(x, y, fila) {
    $('#guDragGhost').remove();
    $('body').removeClass('gu-arrastrando');
    $('.gu-nodo-linea').removeClass('gu-drop-objetivo').removeClass('gu-drop-sobre');

    var $bajo = $(document.elementFromPoint(x, y)).closest('.gu-nodo-linea');
    var $nodo = $bajo.closest('.gu-nodo');
    var nodo = $nodo.data('nodo');

    if (!nodo || (nodo.tipo !== 'local' && nodo.tipo !== 'puesto')) {
        arrastreActivo = null;
        return;
    }

    arrastreActivo = null;

    var tipoTexto = nodo.tipo === 'puesto' ? 'puesto de trabajo' : 'local';
    $.confirm({
        title: '!Confirmar asignaci&oacute;n!',
        content: '&iquest;Est&aacute; seguro de que desea asignar el AFT No. <b>' + fila.noInventario +
            '</b> al ' + tipoTexto + ' <b>' + nodo.nombre + '</b>?',
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
                    if (nodo.tipo === 'local') {
                        ejecutarAsignacion(String(fila.noInventario), nodo.id, null);
                    } else {
                        ejecutarAsignacion(String(fila.noInventario), null, nodo.id);
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