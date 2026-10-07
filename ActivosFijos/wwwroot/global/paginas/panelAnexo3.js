/* =========================================================
   Panel lateral de firmas del Anexo 3.
   Al marcar un checkbox se muestra el formulario del movimiento,
   con los mismos campos que tenia el modal, y se puede navegar
   entre los movimientos marcados.
   ========================================================= */

var ax3Movimientos = [];
var ax3Catalogos = { tiposMovimiento: [], responsables: [], cargos: [] };
var ax3Cargado = false;
var ax3Actual = null;
var ax3DireccionUEB = '';
var tiposConTransportador = ['traslado externo', 'enviado a reparar'];
var ax3FUNDAMENTACION_POR_DEFECTO = 'Por necesidad del mismo';
var camposAplicables = ['TipoMovimiento', 'Fundamentacion', 'InformeTecnico', 'NombreTecnico',
    'CargoTecnico', 'HechoPor', 'CargoHechoPor', 'TransportadorReceptor',
    'CargoTransportadorReceptor', 'Comprobante', 'DireccionReceptor',
    'ReceptorNombre', 'ReceptorArea'];

function ax3Clave(noInv, subNum) {
    return noInv + '|' + subNum;
}

/* La API devuelve el JSON en camelCase (default de System.Text.Json en MVC) mientras que
   los modelos C# son PascalCase. Sin esta conversion m.NoInventario y f.NombreTecnico
   llegan undefined, la clave del movimiento queda "undefined|undefined" y el formulario
   nunca llega a pintarse. El binding del servidor si es case-insensitive, por eso el
   guardado funcionaba y la lectura no. */
function ax3NormalizarFirmas(f) {
    var firmas = {
        NoInventario: f.noInventario,
        SubNumero: f.subNumero
    };
    $.each(camposAplicables, function (i, campo) {
        // El unico cambio de la convencion es la primera letra en minuscula.
        firmas[campo] = f[campo.charAt(0).toLowerCase() + campo.slice(1)] || '';
    });
    return firmas;
}

function ax3NormalizarMovimiento(m) {
    return {
        NoInventario: m.noInventario,
        SubNumero: m.subNumero,
        Descripcion: m.descripcion || '',
        AreaAnterior: m.areaAnterior || '',
        AreaActual: m.areaActual || '',
        ResponsableAnterior: m.responsableAnterior || '',
        ResponsableActual: m.responsableActual || '',
        SiFirmasCargadas: !!m.siFirmasCargadas,
        Firmas: ax3NormalizarFirmas(m.firmas || {})
    };
}

function ax3FechaIso() {
    // El input #Fecha viene en formato MM/YYYY.
    var partes = String($('#Fecha').val() || '').split('/');
    if (partes.length < 2) {
        return '';
    }
    var mes = ('0' + partes[0]).slice(-2);
    var anio = partes[1];
    if (anio.length === 2) {
        anio = '20' + anio;
    }
    return anio + '-' + mes + '-01';
}

/* ==================== Carga de datos ==================== */

function AbrirPanelAnexo3() {
    $('#ax3Panel').addClass('ax3-abierto');
    $('#ax3Overlay').addClass('ax3-visible');

    if (ax3Cargado) {
        SincronizarPanel(null);
        return;
    }

    CargarMovimientosAnexo3();
}

function ReiniciarPanelAnexo3() {
    // Antes de tirar el formulario hay que guardar lo que se estaba escribiendo: cambiar
    // el mes, el area o el responsable recarga la pagina y perderia lo tipeado.
    GuardarActual(true);
    ax3Movimientos = [];
    ax3Cargado = false;
    ax3Actual = null;
    ax3DestruirFormulario();
    $('#ax3Formulario').html('<div class="alert alert-info">Cambio el mes, el &aacute;rea o el responsable. Marqu&eacute; de nuevo los movimientos.</div>');
    $('#ax3Navegacion').hide();
    $('#ax3Receptor').hide();
    ActualizarPie();
}

function CargarMovimientosAnexo3() {
    var fecha = ax3FechaIso();
    $('#ax3Fecha').val(fecha);
    $('#ax3Area').val($('#Area').val() || '');
    $('#ax3Responsable').val($('#Responsable').val() || '');

    ax3DestruirFormulario();
    $('#ax3Formulario').html('<div class="ax3-cargando">Cargando movimientos...</div>');
    $('#ax3Navegacion').hide();

    $.ajax({
        url: baseUrl + 'ctActFijos/ListadoFirmasAnexo3',
        method: 'POST',
        data: { fecha: fecha, area: $('#ax3Area').val(), responsable: $('#ax3Responsable').val() },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            if (data.siNo === false) {
                $('#ax3Formulario').empty();
                MiApp.Mensajes.mostrarNotificacion("error", data.objMensaje);
                return;
            }

            ax3Movimientos = (data.movimientos || []).map(ax3NormalizarMovimiento);
            ax3Catalogos = {
                tiposMovimiento: data.tiposMovimiento || [],
                responsables: data.responsables || [],
                cargos: data.cargos || []
            };

            // Aprobado y Autorizado son fijos: los mismos para todos los movimientos.
            $('#ax3AprobadoPor').text(data.aprobadoPor || '');
            $('#ax3CargoAprobadoPor').text(data.cargoAprobadoPor || '');
            $('#ax3AutorizadoPor').text(data.autorizadoPor || '');
            $('#ax3CargoAutorizadoPor').text(data.cargoAutorizadoPor || '');

            ax3DireccionUEB = data.direccionUEB || '';

            ax3Cargado = true;
            SincronizarPanel(null);
        },
        error: function (error) {
            $('#ax3Formulario').empty();
            MiApp.Mensajes.mostrarNotificacion("error", 'No se pudieron cargar los movimientos.');
            console.log(error);
        }
    });
}

function CerrarPanelAnexo3() {
    GuardarActual(true);
    $('#ax3Panel').removeClass('ax3-abierto');
    $('#ax3Overlay').removeClass('ax3-visible');
}

/* ==================== Marcados y navegacion ==================== */

// Los marcados, en el mismo orden en que vienen del servidor (area, responsable, inventario).
// Asi marcar o desmarcar no altera el recorrido.
function ax3MarcadosOrdenados() {
    var claves = {};
    $('#tablaActivosFijos .chkMovimientoAnexo3:checked').each(function () {
        var $fila = $(this).closest('tr');
        claves[ax3Clave(parseInt($fila.data('info'), 10), parseInt($fila.data('subnumero') || 0, 10))] = true;
    });

    return ax3Movimientos.filter(function (m) {
        return claves[ax3Clave(m.NoInventario, m.SubNumero)];
    });
}

function ax3MovimientoActual() {
    if (!ax3Actual) {
        return null;
    }
    var clave = ax3Actual;
    return ax3Movimientos.filter(function (m) {
        return ax3Clave(m.NoInventario, m.SubNumero) === clave;
    })[0] || null;
}

function ax3IndiceActual(marcados) {
    for (var i = 0; i < marcados.length; i++) {
        if (ax3Clave(marcados[i].NoInventario, marcados[i].SubNumero) === ax3Actual) {
            return i;
        }
    }
    return -1;
}

function ax3ClaveDeFila($fila) {
    return ax3Clave(parseInt($fila.data('info'), 10), parseInt($fila.data('subnumero') || 0, 10));
}

// claveMarcada es la del checkbox recien marcado, o null si se desmarco.
function SincronizarPanel(claveMarcada) {
    if (!ax3Cargado) {
        return;
    }

    GuardarActual(true);

    var marcados = ax3MarcadosOrdenados();

    if (marcados.length === 0) {
        ax3Actual = null;
    } else if (claveMarcada) {
        ax3Actual = claveMarcada;
    } else if (ax3IndiceActual(marcados) < 0) {
        ax3Actual = ax3Clave(marcados[0].NoInventario, marcados[0].SubNumero);
    }

    RenderFormularioAnexo3();
    ActualizarPie();
}

function NavegarAnexo3(delta) {
    var marcados = ax3MarcadosOrdenados();
    var destino = ax3IndiceActual(marcados) + delta;

    if (destino < 0 || destino >= marcados.length) {
        return;
    }

    GuardarActual(true);
    ax3Actual = ax3Clave(marcados[destino].NoInventario, marcados[destino].SubNumero);
    RenderFormularioAnexo3();
    ActualizarPie();
}

function ActualizarNavegacionAnexo3() {
    var marcados = ax3MarcadosOrdenados();
    var indice = ax3IndiceActual(marcados);
    var hay = indice >= 0;

    $('#ax3Navegacion').toggle(hay);
    $('#ax3Anterior').prop('disabled', !hay || indice <= 0);
    $('#ax3Siguiente').prop('disabled', !hay || indice >= marcados.length - 1);

    if (!hay) {
        $('#ax3Posicion').text('');
        $('#ax3MovActual').text('');
        $('#ax3DescActual').text('').removeAttr('title');
        return;
    }

    $('#ax3Posicion').text('Movimiento ' + (indice + 1) + ' de ' + marcados.length);

    var mov = marcados[indice];
    var etiqueta = mov.SubNumero !== 0 ? mov.NoInventario + '-' + mov.SubNumero : String(mov.NoInventario);
    $('#ax3MovActual').text('No. ' + etiqueta);
    $('#ax3DescActual').text(mov.Descripcion || '').attr('title', mov.Descripcion || '');
}

/* ==================== Formulario ==================== */

function ax3DestruirFormulario() {
    // select2 deja wrappers en el DOM: hay que destruirlos antes de vaciar el contenedor.
    $('#ax3Formulario select.select2me').each(function () {
        $(this).select2('destroy');
    });
}

function RenderFormularioAnexo3() {
    var $destino = $('#ax3Formulario');
    ax3DestruirFormulario();
    $destino.empty();

    var mov = ax3MovimientoActual();
    ActualizarNavegacionAnexo3();
    PintarReceptor(mov);

    if (!mov) {
        var mensaje = ax3Movimientos.length === 0
            ? 'No hay movimientos en la fecha y los filtros seleccionados.'
            : 'Marc&aacute; un movimiento en la tabla para cargarle las firmas.';
        $destino.html($('<div class="alert alert-warning"></div>').html(mensaje));
        return;
    }

    var $form = $('<div class="ax3-form" data-clave="' + ax3Clave(mov.NoInventario, mov.SubNumero) + '"></div>');
    var f = mov.Firmas || {};

    $form.append($('<div class="ax3-seccion">Tipo de Movimiento</div>'));
    var $pTipo = $('<div class="ax3-par"></div>').appendTo($form);
    var $cTipo = $('<div></div>').appendTo($pTipo);
    $('<label></label>').text('Tipo de Movimiento').appendTo($cTipo);
    var $selTipo = $('<select class="form-control select2me ax3-campo"></select>')
        .attr('data-campo', 'TipoMovimiento').appendTo($cTipo);
    $('<option value=""></option>').text('-- Seleccione --').appendTo($selTipo);
    $.each(ax3Catalogos.tiposMovimiento, function (i, t) {
        $('<option></option>').val(t).text(t).appendTo($selTipo);
    });
    var tipoDesdeMovimiento = (mov.TipoDeclarado || '').trim();
    if (!tipoDesdeMovimiento && f.TipoMovimiento) tipoDesdeMovimiento = f.TipoMovimiento;
    if (!tipoDesdeMovimiento) tipoDesdeMovimiento = 'Traslado Interno';
    $selTipo.val(tipoDesdeMovimiento);

    $form.append($('<div class="ax3-seccion">Fundamentaci&oacute;n de la operaci&oacute;n</div>'));
    $('<textarea class="form-control ax3-campo" rows="2" maxlength="500"></textarea>')
        .attr('data-campo', 'Fundamentacion')
        .val(f.Fundamentacion || ax3FUNDAMENTACION_POR_DEFECTO).appendTo($form);

    // El informe tecnico va dentro de la seccion del tecnico.
    $form.append($('<div class="ax3-seccion">Técnico</div>'));
    $('<label></label>').text('Informe Técnico').appendTo($form);
    $('<textarea class="form-control ax3-campo" rows="2" maxlength="500"></textarea>')
        .attr('data-campo', 'InformeTecnico').val(f.InformeTecnico || '').appendTo($form);

    var $pNombre = $('<div class="ax3-par"></div>').appendTo($form);
    // Quien rinde el informe tecnico casi siempre es quien hace el movimiento.
    $pNombre.append(ax3ParPersonaCargo('Nombre', 'NombreTecnico', 'CargoTecnico', f.NombreTecnico || mov.ResponsableAnterior));

    $form.append($('<div class="ax3-seccion">Hecho por</div>'));
    var $pHecho = $('<div class="ax3-par"></div>').appendTo($form);
    // Por defecto firma quien tenia el ActivoFijo en el mes anterior.
    $pHecho.append(ax3ParPersonaCargo('Nombre', 'HechoPor', 'CargoHechoPor', f.HechoPor || mov.ResponsableAnterior));

    $form.append($('<div class="ax3-seccion ax3-transportador">Transportador o Receptor</div>'));
    var $pTransp = $('<div class="ax3-par ax3-transportador"></div>').appendTo($form);
    $pTransp.append(ax3ParPersonaCargo('Nombre', 'TransportadorReceptor', 'CargoTransportadorReceptor', f.TransportadorReceptor));

    $form.append($('<div class="ax3-seccion">Comprobante</div>'));
    $('<label></label>').text('Comprobante No.').appendTo($form);
    $('<input type="text" class="form-control ax3-campo" maxlength="100" />')
        .attr('data-campo', 'Comprobante').val(f.Comprobante || '').appendTo($form);

    var $rapidas = $('<div class="ax3-acciones-rapidas"></div>').appendTo($form);
    $('<button type="button" class="btn btn-xs default" data-ax3-aplicar="todos"><i class="fa fa-copy"></i> Aplicar a todos</button>').appendTo($rapidas);
    $('<button type="button" class="btn btn-xs default" data-ax3-aplicar="vacios"><i class="fa fa-copy"></i> Aplicar a los vac&iacute;os</button>').appendTo($rapidas);

    $destino.append($form);

    AplicarVisibilidadTransportador($form, $selTipo.val());

    // Se inicializa recien con el contenedor visible: dentro de algo oculto select2 mide mal.
    $form.find('select.select2me').each(function () {
        $(this).select2({ theme: 'bootstrap', width: null });
    });

    $form.on('change', '.ax3-campo', function () {
        var tipo = $form.find('[data-campo="TipoMovimiento"]').val();
        AplicarVisibilidadTransportador($form, tipo);
        RefrescarDireccionReceptor(tipo);
        // Cambiar el tipo o el receptor puede juntar o separar grupos.
        ActualizarPie();
    });

    $rapidas.on('click', '[data-ax3-aplicar]', function () {
        AplicarAOtros($(this).attr('data-ax3-aplicar') === 'vacios');
    });

    ActualizarFirmasEnGrilla();
}

// El cargo no se elige: se deriva del responsable. Solo si el responsable no tiene cargo
// asociado se listan los cargos para poder seleccionar uno.
function ax3ParPersonaCargo(etiqueta, campoPersona, campoCargo, seleccionado) {
    var $div = $('<div class="ax3-par-cargo"></div>');
    $('<label></label>').text(etiqueta).appendTo($div);
    var $cuerpo = $('<div></div>').appendTo($div);

    var $sel = $('<select class="form-control select2me ax3-campo"></select>').attr('data-campo', campoPersona);
    $('<option value=""></option>').text('-- Seleccione --').appendTo($sel);
    $.each(ax3Catalogos.responsables, function (i, r) {
        $('<option></option>').val(r.nombre).text(r.nombre).appendTo($sel);
    });
    $sel.val(seleccionado || '');
    $cuerpo.append($sel);

    PintarCargo($cuerpo, campoPersona, campoCargo);

    $sel.on('change', function () {
        PintarCargo($cuerpo, campoPersona, campoCargo);
    });

    return $div;
}

function ax3CargoDeResponsable(nombre) {
    if (!nombre) {
        return '';
    }
    for (var i = 0; i < ax3Catalogos.responsables.length; i++) {
        if (ax3Catalogos.responsables[i].nombre === nombre) {
            return ax3Catalogos.responsables[i].cargo || '';
        }
    }
    return '';
}

function PintarCargo($cuerpo, campoPersona, campoCargo) {
    // Antes de redibujar se guarda lo elegido, para no perderlo si el responsable
    // no tiene cargo y se vuelve a pintar el mismo control.
    var previo = $cuerpo.find('[data-campo="' + campoCargo + '"]').val() || '';
    var cargo = ax3CargoDeResponsable($cuerpo.find('[data-campo="' + campoPersona + '"]').val());

    $cuerpo.find('.ax3-cargo-caja').remove();

    var $caja = $('<div class="ax3-cargo-caja"></div>');

    if (cargo) {
        // El cargo no se elige: se muestra como texto, igual que Aprobado y Autorizado por.
        // El input oculto solo lleva el valor para que siga guardandose y limpiandose.
        $('<input type="hidden" class="ax3-campo" />')
            .attr('data-campo', campoCargo).val(cargo).appendTo($caja);
        $('<span class="ax3-fijo-valor ax3-cargo-derivable"></span>')
            .attr('data-cargo-texto', campoCargo).text(cargo).appendTo($caja);
    } else {
        var $sel = $('<select class="form-control ax3-campo"></select>').attr('data-campo', campoCargo);
        $('<option value=""></option>').text('-- Seleccione --').appendTo($sel);
        $.each(ax3Catalogos.cargos, function (i, c) {
            $('<option></option>').val(c).text(c).appendTo($sel);
        });
        $sel.val(previo);
        $caja.append($sel);
    }

    $cuerpo.append($caja);
}

// El transportador solo aplica a traslado externo y enviado a reparar.
function AplicarVisibilidadTransportador($contenedor, tipoMovimiento) {
    var aplica = tiposConTransportador.indexOf(String(tipoMovimiento || '').trim().toLowerCase()) >= 0;
    $contenedor.find('.ax3-transportador').toggle(aplica);

    if (!aplica) {
        var campos = 'TransportadorReceptor', cargo = 'CargoTransportadorReceptor';
        $contenedor.find('[data-campo="' + campos + '"]').val('').trigger('change.select2');
        $contenedor.find('[data-campo="' + cargo + '"]').val('').trigger('change.select2');
        // El cargo derivado se muestra en un span aparte del input que lleva el valor.
        $contenedor.find('[data-cargo-texto="' + campos + '"],[data-cargo-texto="' + cargo + '"]').text('');
    }
}

// El Receptor depende del movimiento: cambia al navegar, asi que no va en el catalogo fijo.
// Nombre y Area son editables y se guardan aparte: el submayor calcula quien recibio el
// Activo Fijo, pero el usuario puede corregirlo sin recalcular nada.
function PintarReceptor(mov) {
    var $caja = $('#ax3Receptor');

    if (!mov) {
        $caja.hide();
        return;
    }

    var f = mov.Firmas || {};

    $caja.show();
    PintarNombreReceptor(f.ReceptorNombre || mov.ResponsableActual || '');
    PintarAreaReceptor(f.ReceptorArea || mov.AreaActual || '');

    PintarDireccionReceptor(ax3AplicaDireccionPropia(f.TipoMovimiento), f.DireccionReceptor || '');
}

// Select con todos los responsables, preseleccionado con el que calculo el submayor.
function PintarNombreReceptor(valor) {
    var $sel = $('<select class="form-control ax3-campo"></select>').attr('data-campo', 'ReceptorNombre');
    $('<option value=""></option>').text('-- Seleccione --').appendTo($sel);
    $.each(ax3Catalogos.responsables, function (i, r) {
        $('<option></option>').val(r.nombre).text(r.nombre).appendTo($sel);
    });

    // Si el nombre guardado no esta en el catalogo se agrega: no se pierde lo tipeado.
    if (valor && !ax3TieneOpcion($sel, valor)) {
        $('<option></option>').val(valor).text(valor).appendTo($sel);
    }
    $sel.val(valor || '');

    $sel.on('change', function () {
        // El area se deriva del responsable elegido, igual que el cargo.
        $('#ax3ReceptorArea').val(ax3AreaDeResponsable($(this).val()));
        ActualizarPie();
    });

    $('#ax3ReceptorNombre').empty().append($sel);
}

function PintarAreaReceptor(valor) {
    $('#ax3ReceptorArea').empty().append(
        $('<input type="text" class="form-control ax3-campo" maxlength="150" />')
            .attr('data-campo', 'ReceptorArea').val(valor || '')
            .on('change', function () {
                ActualizarPie();
            }));
}

function ax3TieneOpcion($sel, valor) {
    var siTiene = false;
    $sel.find('option').each(function () {
        if ($(this).val() === valor) {
            siTiene = true;
            return false;
        }
    });
    return siTiene;
}

function ax3AreaDeResponsable(nombre) {
    if (!nombre) {
        return '';
    }
    for (var i = 0; i < ax3Catalogos.responsables.length; i++) {
        if (ax3Catalogos.responsables[i].nombre === nombre) {
            return ax3Catalogos.responsables[i].area || '';
        }
    }
    return '';
}

function ax3AplicaDireccionPropia(tipoMovimiento) {
    return tiposConTransportador.indexOf(String(tipoMovimiento || '').trim().toLowerCase()) >= 0;
}

// En traslado externo y enviado a reparar la direccion del receptor es distinta de la de la
// UEB y se captura. En los demas tipos se muestra la de la UEB, que sale de Parametros.
function PintarDireccionReceptor(editable, valor) {
    var $dato = $('#ax3ReceptorDireccion').empty();

    if (!editable) {
        $('<span class="ax3-fijo-valor"></span>').text(ax3DireccionUEB || '-').appendTo($dato);
        return;
    }

    $('<textarea class="form-control ax3-receptor-direccion ax3-campo" rows="2" maxlength="300"></textarea>')
        .attr('data-campo', 'DireccionReceptor').val(valor).appendTo($dato);
}

// El textarea de la direccion solo existe en esos dos tipos de movimiento, asi que hay que
// volver a dibujarlo cada vez que cambia el tipo. Se conserva lo que se estaba escribiendo.
function RefrescarDireccionReceptor(tipoMovimiento) {
    if (!$('#ax3Receptor').is(':visible')) {
        return;
    }

    var previo = $('#ax3ReceptorDireccion').find('[data-campo="DireccionReceptor"]').val() || '';
    PintarDireccionReceptor(ax3AplicaDireccionPropia(tipoMovimiento), previo);
}

/* ==================== Guardado ==================== */

function ax3LeerFormulario() {
    var $form = $('#ax3Formulario .ax3-form');
    if ($form.length === 0) {
        return null;
    }

    var partes = $form.attr('data-clave').split('|');
    var firmas = {
        NoInventario: parseInt(partes[0], 10),
        SubNumero: parseInt(partes[1], 10)
    };

    $form.find('.ax3-campo').each(function () {
        firmas[$(this).attr('data-campo')] = $(this).val() || '';
    });

    // El textarea de la direccion del receptor vive en el bloque de solo lectura,
    // fuera del formulario, asi que se lee aparte para que tambien se guarde.
    $('#ax3Receptor').find('.ax3-campo').each(function () {
        firmas[$(this).attr('data-campo')] = $(this).val() || '';
    });

    return firmas;
}

// Vuelca el formulario sobre los datos en memoria del movimiento actual y lo persiste.
// Se dispara al cambiar de movimiento, al desmarcar, al cerrar el panel y al generar.
function GuardarActual(silencioso) {
    var firmas = ax3LeerFormulario();
    if (!firmas) {
        return null;
    }

    var clave = ax3Clave(firmas.NoInventario, firmas.SubNumero);
    var mov = ax3MovimientoActual();

    if (mov && ax3Clave(mov.NoInventario, mov.SubNumero) === clave) {
        mov.Firmas = firmas;
    }

    // Marcar movimientos en blanco no debe generar escrituras: al marcar varios se pasa
    // por aqui una vez por cada uno.
    if (ax3FirmasVacias(firmas) && ax3FirmasVacias(mov ? mov.Firmas : null)) {
        return firmas;
    }

    EnviarFirmasAnexo3([clave], [firmas], false);

    if (!silencioso) {
        MiApp.Mensajes.mostrarNotificacion("success", 'Se guardaron las firmas del movimiento.');
    }

    return firmas;
}

function ax3FirmasVacias(firmas) {
    if (!firmas) {
        return true;
    }
    for (var i = 0; i < camposAplicables.length; i++) {
        if ((firmas[camposAplicables[i]] || '').trim()) {
            return false;
        }
    }
    return true;
}

function EnviarFirmasAnexo3(claves, firmas, avisar) {
    if (!firmas || firmas.length === 0) {
        return;
    }

    return $.ajax({
        url: baseUrl + 'ctActFijos/FirmasAnexo3',
        method: 'POST',
        data: { fecha: $('#ax3Fecha').val(), firmas: JSON.stringify(firmas) },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            if (!data.siNo) {
                MiApp.Mensajes.mostrarNotificacion("error", data.objMensaje);
                return;
            }
            ActualizarFirmasEnGrilla();
            if (avisar) {
                MiApp.Mensajes.mostrarNotificacion("success", data.objMensaje);
            }
        },
        error: function (error) {
            MiApp.Mensajes.mostrarNotificacion("error", 'No se pudieron guardar las firmas.');
            console.log(error);
        }
    });
}

function ActualizarFirmasEnGrilla() {
    var mov = ax3MovimientoActual();
    if (!mov) {
        return;
    }

    var f = mov.Firmas || {};
    var cargadas = !!(f.NombreTecnico || '').trim() && !!(f.HechoPor || '').trim();
    var $celda = $('#tablaActivosFijos td[data-firmas="' + ax3Clave(mov.NoInventario, mov.SubNumero) + '"]');

    $celda.html(cargadas
        ? '<span class="label label-success">Cargadas</span>'
        : '<span class="label label-default">Sin cargar</span>');
}

// Copia los valores del movimiento visible a los demas marcados. Se escribe sobre los datos
// en memoria de cada uno y se persiste, sin depender de que esten pintados.
function AplicarAOtros(soloVacios) {
    var origen = ax3LeerFormulario();
    if (!origen) {
        return;
    }

    var aplicadas = [];
    var claves = [];

    $.each(ax3MarcadosOrdenados(), function (i, m) {
        if (ax3Clave(m.NoInventario, m.SubNumero) === ax3Actual) {
            return;
        }

        var destino = m.Firmas || {};
        var cambios = 0;

        $.each(camposAplicables, function (j, campo) {
            var valor = origen[campo] || '';
            if (soloVacios && !!(destino[campo] || '').trim()) {
                return;
            }
            if ((destino[campo] || '') !== valor) {
                destino[campo] = valor;
                cambios++;
            }
        });

        if (cambios > 0) {
            destino.NoInventario = m.NoInventario;
            destino.SubNumero = m.SubNumero;
            m.Firmas = destino;
            aplicadas.push(destino);
            claves.push(ax3Clave(m.NoInventario, m.SubNumero));
        }
    });

    if (aplicadas.length === 0) {
        MiApp.Mensajes.mostrarNotificacion("info", soloVacios
            ? 'No hay movimientos marcados con campos vacios para completar.'
            : 'No hubo cambios que aplicar.');
        return;
    }

    EnviarFirmasAnexo3(claves, aplicadas, true);
}

/* ==================== Generacion ==================== */

// Lo que decide si dos activos pueden ir en el mismo anexo es el tipo de movimiento y
// quien recepciona: mismos datos que se imprimen, para que lo que se vea en el panel
// coincida con lo que se genera.
function ax3ClaveDeGrupo(m) {
    var f = m.Firmas || {};
    var tipoGrupo = (m.TipoDeclarado || '').trim();
    if (!tipoGrupo && f.TipoMovimiento) tipoGrupo = f.TipoMovimiento;
    if (!tipoGrupo) tipoGrupo = 'Traslado Interno';
    return [tipoGrupo.trim().toLowerCase(),
        (f.ReceptorNombre || m.ResponsableActual || '').trim(),
        (f.ReceptorArea || m.AreaActual || '').trim()].join('|');
}

function ax3GruposMarcados() {
    var grupos = [];

    $.each(ax3MarcadosOrdenados(), function (i, m) {
        var clave = ax3ClaveDeGrupo(m);
        var grupo = null;

        for (var j = 0; j < grupos.length; j++) {
            if (grupos[j].clave === clave) {
                grupo = grupos[j];
                break;
            }
        }

        if (!grupo) {
            grupo = { clave: clave, items: [] };
            grupos.push(grupo);
        }

        grupo.items.push(m);
    });

    return grupos;
}

// La opcion de agrupar solo se ofrece cuando de verdad hay algo que agrupar.
function ActualizarModoDeGeneracion() {
    var hayGrupo = false;
    $.each(ax3GruposMarcados(), function (i, g) {
        if (g.items.length > 1) {
            hayGrupo = true;
            return false;
        }
    });

    $('#ax3ModoWrap').toggle(hayGrupo);

    if (!hayGrupo) {
        $('#ax3ModoWrap input[value="por-activo"]').prop('checked', true);
    }
}

function ActualizarPie() {
    var n = ax3MarcadosOrdenados().length;
    $('#ax3Contador').text(n === 1 ? '1 movimiento marcado' : n + ' movimientos marcados');
    $('#ax3Generar').prop('disabled', n === 0);
    $('#ax3Generar').html('<i class="fa fa-file-text-o"></i> Generar Anexos de los marcados' + (n > 0 ? ' (' + n + ')' : ''));
    ActualizarModoDeGeneracion();
    ActualizarNavegacionAnexo3();
}

function GenerarAnexosAnexo3() {
    var marcados = ax3MarcadosOrdenados();

    if (marcados.length === 0) {
        MiApp.Mensajes.mostrarNotificacion("error", 'Marqu&eacute; al menos un movimiento para generar sus anexos.');
        return;
    }

    GuardarActual(true);

    var incompletos = [];
    $.each(marcados, function (i, m) {
        var f = m.Firmas || {};
        if (!(f.NombreTecnico || '').trim() || !(f.HechoPor || '').trim()) {
            incompletos.push(ax3Clave(m.NoInventario, m.SubNumero));
        }
    });

    var generar = function () {
        var claves = marcados.map(function (m) {
            return { NoInventario: m.NoInventario, SubNumero: m.SubNumero };
        });
        $('#ax3Movimientos').val(JSON.stringify(claves));
        $('#formGenerarAnexo3').submit();
    };

    if (incompletos.length === 0) {
        generar();
        return;
    }

    var detalle = incompletos.slice(0, 12).join(', ') +
        (incompletos.length > 12 ? ' y ' + (incompletos.length - 12) + ' mas' : '');

    $.confirm({
        title: '!Firmas incompletas!',
        content: '<b>' + incompletos.length + '</b> de ' + marcados.length +
            ' movimientos no tienen Nombre t&eacute;cnico o Hecho por.<br><br>' +
            '<span style="font-size:12px">' + detalle + '</span><br><br>Desea generar de todos modos?',
        icon: 'fa fa-warning',
        type: 'orange',
        theme: 'light',
        buttons: {
            generar: { text: 'Generar igual', btnClass: 'btn-orange', action: generar },
            revisar: { text: 'Revisar', btnClass: 'btn-default' }
        }
    });
}

/* ==================== Eventos ==================== */

$(function () {
    $('#ax3Cerrar').on('click', CerrarPanelAnexo3);
    $('#ax3Overlay').on('click', CerrarPanelAnexo3);
    $('#ax3Generar').on('click', GenerarAnexosAnexo3);
    $('#ax3Anterior').on('click', function () { NavegarAnexo3(-1); });
    $('#ax3Siguiente').on('click', function () { NavegarAnexo3(1); });

    $('#ax3MarcarTodos').on('click', function () {
        $('#tablaActivosFijos .chkMovimientoAnexo3').prop('checked', true);
        SincronizarPanel(UltimaClaveMarcada());
    });

    $('#ax3DesmarcarTodos').on('click', function () {
        $('#tablaActivosFijos .chkMovimientoAnexo3').prop('checked', false);
        SincronizarPanel(null);
    });

    // El partial se inyecta por AJAX dentro de #gridActivosFijos, asi que se delega sobre
    // ese contenedor: al cambiar de mes la tabla se reemplaza y el handler se mantiene.
    $('#gridActivosFijos').on('change', '.chkMovimientoAnexo3', function () {
        var clave = ax3ClaveDeFila($(this).closest('tr'));

        if (this.checked) {
            $('#ax3Panel').addClass('ax3-abierto');
            $('#ax3Overlay').addClass('ax3-visible');

            if (!ax3Cargado) {
                CargarMovimientosAnexo3();
                return;
            }
        }

        SincronizarPanel(this.checked ? clave : null);
    });

    $('#Fecha, #Area, #Responsable').on('change', function () {
        ReiniciarPanelAnexo3();
    });

    ActualizarPie();
});

function UltimaClaveMarcada() {
    var $ultimo = $('#tablaActivosFijos .chkMovimientoAnexo3:checked').last();
    return $ultimo.length === 0 ? null : ax3ClaveDeFila($ultimo.closest('tr'));
}