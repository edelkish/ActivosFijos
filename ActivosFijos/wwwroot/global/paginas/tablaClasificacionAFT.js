CargarTablaClasificadores();
CargarTablaAFTSinClasificar();
CargarTablaAFTClasificados();

function CargarTablaClasificadores() {
    
    $.ajax({
        url: baseUrl + 'ctActFijos/ListadoDeClasificadores',
        method: 'POST',
        data: {  },
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        success: function (data) {
            $('#gridClasificadores').html(data);

            var TableDatatablesButtons = function () {
                var e = function () {
                    var e = $("#tablaClasificadores");
                    e.dataTable({
                        language: {
                            url: baseUrl + "global/language/Spanish.json",
                            aria: {
                                sortAscending: ": activate to sort column ascending",
                                sortDescending: ": activate to sort column descending"
                            },
                            emptyTable: "No data available in table",
                            info: "Showing _START_ to _END_ of _TOTAL_ entries",
                            infoEmpty: "No entries found",
                            infoFiltered: "(filtered1 from _MAX_ total entries)",
                            lengthMenu: "_MENU_ entries",
                            search: "Search:",
                            zeroRecords: "No matching records found"
                        },
                        buttons: [],
                        order: [],
                        lengthMenu: [[10, 15, 20, -1], [10, 15, 20, "All"]],
                        pageLength: 20,
                        pagingType: "bootstrap_full_number",
                        dom: "<'row' <'col-md-12'B>><'row'<'col-md-6 col-sm-12'l><'col-md-6 col-sm-12'f>r><'table-scrollable't><'row'<'col-md-5 col-sm-12'i><'col-md-7 col-sm-12'p>>"
                    });

                    $('#tablaClasificadores').on('click', 'tr', function () {
                        if ($(this).hasClass('selected')) {
                            //$(this).removeClass('selected');
                            //idFilaSeleccionada = null;
                        }
                        else {
                            e.$('tr.selected').removeClass('selected');
                            $(this).addClass('selected');
                            idFilaSeleccionada = $(this).data('info');

                            //CargarTablaAFTClasificados();
                        }
                    });
                }

                return {
                    init: function () {
                        jQuery().dataTable && (e())
                    }
                }
            }();

            TableDatatablesButtons.init();
        },
        error: function (error) {
            console.log(error);
        },
        complete: function () {

        }
    });
}

function CargarTablaAFTSinClasificar() {
    var baseUrl = $('base').attr('href');
    var source =
    {
        url: baseUrl + 'ctActFijos/ListadoDeAFTSinClasificar',
        datafields:
            [
                { name: 'noInventario', type: 'number' },
                { name: 'descripcion', type: 'string' },
                { name: 'centroCosto', type: 'string' },
                { name: 'nombre', type: 'string' },
                { name: 'numero', type: 'number' }
            ],
        datatype: "json",
        type: "post",
        root: 'rows',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        beforeprocessing: function (data) {
            source.totalrecords = data.TotalRows;
        }
    };
    var dataadapter = new $.jqx.dataAdapter(source, {
        loadComplete: function (data) {
            //OnEndCallback();
        },
        loadError: function (xhr, status, error) {
            //OnEndCallback();
            alert(error);
        }
    });
    // initialize jqxGrid
    $("#jqxGridAFTSinClasificar").jqxGrid(
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
            selectionmode: 'multiplerowsadvanced',
            localization: getLocalization('es'),
            pagesize: 20,
            pagermode: 'simple',
            pageable: true,
            rendergridrows: function (obj) {
                return obj.data;
            },
            columns: [
                { text: 'No. Inventario', datafield: 'noInventario', width: 120, filtercondition: 'contains' },
                { text: 'Descripción', datafield: 'descripcion', width: 450, filtercondition: 'contains' },
                { text: 'Centro Costo', datafield: 'nombre', width: 350, filtercondition: 'contains' },
                { text: 'No. Centro Costo', datafield: 'numero', width: 130, cellsalign: 'center', filtercondition: 'contains' }
            ]
        });
}

function CargarTablaAFTClasificados() {
    var baseUrl = $('base').attr('href');
    var source =
    {
        url: baseUrl + 'ctActFijos/ListadoDeAFTClasificados',
        datafields:
            [
                { name: 'noInventario', type: 'number' },
                { name: 'descripcion', type: 'string' },
                { name: 'centroCosto', type: 'string' },
                { name: 'nombre', type: 'string' },
                { name: 'numero', type: 'number' },
                { name: 'clasificador', type: 'string' }
            ],
        datatype: "json",
        type: "post",
        root: 'rows',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
        },
        beforeprocessing: function (data) {
            source.totalrecords = data.TotalRows;
        }
    };
    var dataadapter = new $.jqx.dataAdapter(source, {
        loadComplete: function (data) {
            //OnEndCallback();
        },
        loadError: function (xhr, status, error) {
            //OnEndCallback();
            alert(error);
        }
    });
    // initialize jqxGrid
    $("#jqxGridAFTClasificados").jqxGrid(
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
            selectionmode: 'multiplerowsadvanced',
            localization: getLocalization('es'),
            pagesize: 20,
            pagermode: 'simple',
            pageable: true,
            rendergridrows: function (obj) {
                return obj.data;
            },
            columns: [
                { text: 'No. Inventario', datafield: 'noInventario', width: 120, filtercondition: 'contains' },
                { text: 'Descripción', datafield: 'descripcion', width: 380, filtercondition: 'contains' },
                { text: 'Centro Costo', datafield: 'nombre', width: 280, filtercondition: 'contains' },
                { text: 'No. Centro Costo', datafield: 'numero', width: 130, cellsalign: 'center', filtercondition: 'contains' },
                { text: 'Clasificador', datafield: 'clasificador', width: 200, align: 'center', cellsalign: 'center', filtercondition: 'contains' }
            ]
        });
}

$("#btnAsociar").on('click', function (event) {
    var rowsindexes = $('#jqxGridAFTSinClasificar').jqxGrid('getselectedrowindexes'),
        baseUrl = $('base').attr('href'), arrgelo = [], cadena = "";

    if (rowsindexes.length > 0) {
        for (var i = 0; i < rowsindexes.length-1; i++) {
            var fila = $('#jqxGridAFTSinClasificar').jqxGrid('getrowdata', rowsindexes[i]),
                noInventario = fila.noInventario;

            cadena += noInventario + "|";
            //arrgelo.push(noInventario);
        }

        var filaC = $('#jqxGridAFTSinClasificar').jqxGrid('getrowdata', rowsindexes[i]),
                noInventarioC = filaC.noInventario;

        cadena += noInventarioC;

        $.ajax({
            url: baseUrl + 'ctActFijos/GuardarAFTClasificados',
            method: "POST",
            data: { indexes: cadena, referencia: idFilaSeleccionada },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
            },
            success: function (data) {
                //if (data.siSatisfactorio == -1) {
                    $.confirm({
                        title: '!Advertencia!',
                        content: 'Datos actualizados', //'No todos los datos seleccionados han podido ser asociados satisfactoriamente. Posibles causas: </br> ' + data.mensajeE,
                        icon: 'fa fa-warning',
                        type: 'orange',
                        buttons: {
                            omg: {
                                text: 'Aceptar',
                                btnClass: 'btn-warning',
                            }
                        }
                    });

                    $("#jqxGridAFTSinClasificar").jqxGrid('updatebounddata', 'cells');
                    $('#jqxGridAFTSinClasificar').jqxGrid('clearselection');
                    $("#jqxGridAFTClasificados").jqxGrid('updatebounddata', 'cells');
                //} else if (data.id == 1) {
                //    $.confirm({
                //        title: '!Confirmación!',
                //        content: 'Los datos seleccionados han sido asociados satisfactoriamente.',
                //        icon: 'fa fa-check',
                //        type: 'green',
               //         buttons: {
               //             omg: {
               //                 text: 'Aceptar',
               //                 btnClass: 'btn-green',
               //             }
               //         }
              //      });

              //      $("#jqxgridCGA").jqxGrid('updatebounddata', 'cells');
                    //$("#jqxgridCGAsociar").jqxGrid('clearfilters');
             //   }
            },
            error: function (error) {
                console.log(error);
            },
            complete: function (comp) {

            }
        });
    } else {
        $.confirm({
            title: '!Operación incorrecta!',
            content: 'Debe seleccionar al menos una fila para que la operación sea válida.',
            icon: 'fa fa-close',
            type: 'red',
            theme: 'supervan', //light
            animation: 'news',
            closeAnimation: 'news',
            buttons: {
                omg: {
                    text: 'Intentar nuevamente',
                    btnClass: 'btn-red',
                }
            }
        });
    }
});

$("#btnCancelarClasificacion").on('click', function (event) {
    var rowsindexes = $('#jqxGridAFTClasificados').jqxGrid('getselectedrowindexes'),
        baseUrl = $('base').attr('href'), arrgelo = [], cadena = "";

    if (rowsindexes.length > 0) {
        for (var i = 0; i < rowsindexes.length-1; i++) {
            var fila = $('#jqxGridAFTClasificados').jqxGrid('getrowdata', rowsindexes[i]),
                noInventario = fila.noInventario;

            cadena += noInventario + "|";
            //arrgelo.push(noInventario);
        }

        var filaC = $('#jqxGridAFTClasificados').jqxGrid('getrowdata', rowsindexes[i]),
                noInventarioC = filaC.noInventario;

        cadena += noInventarioC;

        $.ajax({
            url: baseUrl + 'ctActFijos/GuardarAFTDesclasificados',
            method: "POST",
            data: { indexes: cadena },
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN", $('#RequestVerificationToken').val());
            },
            success: function (data) {
                //if (data.siSatisfactorio == -1) {
                    $.confirm({
                        title: '!Advertencia!',
                        content: 'Datos actualizados', //'No todos los datos seleccionados han podido ser asociados satisfactoriamente. Posibles causas: </br> ' + data.mensajeE,
                        icon: 'fa fa-warning',
                        type: 'orange',
                        buttons: {
                            omg: {
                                text: 'Aceptar',
                                btnClass: 'btn-warning',
                            }
                        }
                    });

                    $("#jqxGridAFTSinClasificar").jqxGrid('updatebounddata', 'cells');
                    $('#jqxGridAFTSinClasificar').jqxGrid('clearselection');
                    $("#jqxGridAFTClasificados").jqxGrid('updatebounddata', 'cells');
                //} else if (data.id == 1) {
                //    $.confirm({
                //        title: '!Confirmación!',
                //        content: 'Los datos seleccionados han sido asociados satisfactoriamente.',
                //        icon: 'fa fa-check',
                //        type: 'green',
               //         buttons: {
               //             omg: {
               //                 text: 'Aceptar',
               //                 btnClass: 'btn-green',
               //             }
               //         }
              //      });

              //      $("#jqxgridCGA").jqxGrid('updatebounddata', 'cells');
                    //$("#jqxgridCGAsociar").jqxGrid('clearfilters');
             //   }
            },
            error: function (error) {
                console.log(error);
            },
            complete: function (comp) {

            }
        });
    } else {
        $.confirm({
            title: '!Operación incorrecta!',
            content: 'Debe seleccionar al menos una fila para que la operación sea válida.',
            icon: 'fa fa-close',
            type: 'red',
            theme: 'supervan', //light
            animation: 'news',
            closeAnimation: 'news',
            buttons: {
                omg: {
                    text: 'Intentar nuevamente',
                    btnClass: 'btn-red',
                }
            }
        });
    }
});